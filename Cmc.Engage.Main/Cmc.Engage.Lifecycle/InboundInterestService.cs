using System;
using System.Collections.Generic;
using System.Linq;
using Cmc.Core.Xrm.ServerExtension.Core;
using Cmc.Core.Xrm.ServerExtension.Logging;
using Cmc.Engage.Common.Utilities.Constants;
using Cmc.Engage.Models;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using IExecutionContext = Cmc.Core.Xrm.ServerExtension.Core.IExecutionContext;

namespace Cmc.Engage.Lifecycle
{
    public class InboundInterestService : IInboundInterestService
    {
        private IOrganizationService _orgService;
        private readonly ILogger _tracer;

        public InboundInterestService(ILogger tracer, IOrganizationService orgService)
        {
            _tracer = tracer ?? throw new ArgumentNullException(nameof(tracer));           
             _orgService = orgService;
        }

        #region RetrieveInboundInterestRelatedContact[Field] Workflow Activity

        public EntityReference RetrieveInboundInterestContactLookup(string attributeName, EntityReference inboundInterestId)
        {
            _tracer.Trace($"Retrieving {attributeName} from Inbound Interest {inboundInterestId.Id}");

            return _orgService.RetrieveMultiple(new FetchExpression($@"
                <fetch top='1' version='1.0' output-format='xml-platform' mapping='logical'>
                  <entity name='contact'>
                    <attribute name='{attributeName}' />
                      <link-entity name='lead' from='customerid' to='contactid'>
                        <filter>
                          <condition attribute='leadid' operator='eq' value='{inboundInterestId.Id}' />
                        </filter>
                      </link-entity>
                  </entity>
                </fetch>")).Entities.FirstOrDefault()?
                .GetAttributeValue<EntityReference>(attributeName);
        }

        #endregion
        #region OverrideInitialSourceforContact


        public void OverrideInitialSourceDetailsForContact(IExecutionContext context)
        {
            _tracer.Trace("OverrideInitialSourceforContact Method Start");
            var serviceProvider = context.XrmServiceProvider;
            var pluginContext = serviceProvider.GetPluginExecutionContext();
            var input = pluginContext.GetInputParameter<Entity>("Target").ToEntity<Lead>();

            if (pluginContext.MessageName.ToLower() == Constants.Create)
            {
                CreateInitialSource(input);
            }
            else if (pluginContext.MessageName.ToLower() == Constants.Update)
            {
                var preImage = pluginContext.GetPreEntityImage("Lead").ToEntity<Lead>();
                var postimage = pluginContext.GetPostEntityImage<Entity>("Lead").ToEntity<Lead>();
                UpdateInboundInterest(postimage, preImage);
            }
            _tracer.Trace("OverrideInitialSourceforContact Method Exit");
        }


        private void CreateInitialSource(Lead newInboundInterest)
        {
            _tracer.Trace("CreateInitialSource Method Start");

            var relatedContactId = newInboundInterest.CustomerId;
            var inboundInterestList = GetInboundInterestsForContact(relatedContactId);

            //if one inbound interest is there,then we have to set the primarg flag to true.
            if (inboundInterestList == null)
            {
                newInboundInterest.cmc_Primary = true;
            }
            else
            {
                //only one inbound interest's primary can set to true,thats why making all other inbound interest's primary flag to false.
                if (newInboundInterest.cmc_Primary == true)
                {
                    ResetPrimaryForOtherInboundInterests(inboundInterestList);
                }
            }
            if (newInboundInterest.cmc_Primary == true)
                UpdateInitialSourceDetailsForContact(newInboundInterest);
            _tracer.Trace("CreateInitialSource Method Exit");
        }

        private void UpdateInboundInterest(Lead updatedInboundInterest, Lead previousInboundInterest)
        {
            _tracer.Trace("UpdateInboundInterest Method Start");
        
            if (updatedInboundInterest.cmc_Primary == true)
            {

                var relatedRecord = previousInboundInterest.CustomerId;
                var inboundInterestList = GetInboundInterestsForContact(relatedRecord);
                if (inboundInterestList == null) return;

                if (inboundInterestList.Count > 1 && previousInboundInterest.cmc_Primary != true)
                {                  
                    var otherInboundInterestsForContact =
                        inboundInterestList.Where(x => x.Id != updatedInboundInterest.Id).ToList();
                    _tracer.Trace("Updating the other InboundInterests to false.");
                    ResetPrimaryForOtherInboundInterests(otherInboundInterestsForContact);

                }

                UpdateInitialSourceDetailsForContact(updatedInboundInterest);
            }

            _tracer.Trace("UpdateInboundInterest Method Exit");
        }



        private void ResetPrimaryForOtherInboundInterests(List<Lead> relatedInboundInterestList)
        {
            _tracer.Trace("ResetPrimaryForOtherInboundInterests Method Start");
            foreach (var inboundInterest in relatedInboundInterestList.Where(x => x.cmc_Primary == true))
            {
                _orgService.Update(new Lead
                {
                    Id = inboundInterest.Id,
                    cmc_Primary = false
                });
            }
            _tracer.Trace("ResetPrimaryForOtherInboundInterests Method Exit");
        }

        private List<Lead> GetInboundInterestsForContact(EntityReference customer)
        {
            _tracer.Trace("GetInboundInterestsForContact Method Start");
            if (customer == null)
            {
                _tracer.Error("customer Id is null");
                return null;
            }
            _tracer.Trace($"customer Id is {customer.Id}");

            var fetch =
                $@"<fetch>
                    <entity name='lead'>
                        <attribute name='cmc_primary'/>                      
                        <filter>
                            <condition attribute='customerid' operator='eq' value='{customer.Id}'/>
                            <condition attribute='statecode' operator='eq' value='0'/>
                        </filter>
                    </entity>
                </fetch>";

            var relatedInboundInterestList = _orgService.RetrieveMultiple(new FetchExpression(fetch));
            var data = relatedInboundInterestList.Entities.Count <= 0 ? null
                : relatedInboundInterestList.Entities.Cast<Lead>().ToList();
            _tracer.Trace("Inbound Interest count for " + customer.Id + " is " + relatedInboundInterestList.Entities.Count);
            _tracer.Trace("Get Inbound Interest List For Contact  Method Exit");
            return data;


        }

        public void UpdateInitialSourceDetailsForContact(Lead inboundInterest)
        {
            _tracer.Trace("UpdateInitialSourceDetailsForContact Method Start");
          
         
            if (inboundInterest.CustomerId == null )
            {
                _tracer.Error("InBound Interest has no customer. Not copying InBound Interest fields");
                return;
            }
            if (string.IsNullOrEmpty(inboundInterest.CustomerId.LogicalName)
                || inboundInterest.CustomerId.LogicalName != Contact.EntityLogicalName)

            {
                _tracer.Error("InBound Interest has no contact customer. So Not copying InBound Interest fields");
                return;
            }
            if (inboundInterest.cmc_Primary == false)
            {
                _tracer.Error("InBound Interest has Primary false. Not copying InBound Interest fields");
                return;
            }
            _tracer.Trace("copying data from primary inbound interest to contact initial source.");
            var contact = new Contact
            {
                Id = inboundInterest.CustomerId.Id,
                cmc_sourcedate = inboundInterest.cmc_sourcedate,
                cmc_sourcecampusid = inboundInterest.cmc_sourcecampusid,
                cmc_srcprogramid = inboundInterest.cmc_sourceprgmid,
                cmc_srcpogramlevelid = inboundInterest.cmc_sourceprgmlevelid,
                cmc_expectedstartid = inboundInterest.cmc_expectedstartid,
                cmc_sourcemethodid = inboundInterest.cmc_sourcemethodid,
                cmc_sourcecategoryid = inboundInterest.cmc_sourcecategoryid,
                cmc_sourcesubcategoryid = inboundInterest.cmc_sourcesubcategoryid,
                cmc_sourcereferringcontactid = inboundInterest.cmc_sourcereferringcontactid,
                cmc_sourcereferringorganizationid = inboundInterest.cmc_sourcereferringorganizationid,
                cmc_sourcereferringstaffid = inboundInterest.cmc_sourcereferringstaffid,
                cmc_sourcecampaignid = inboundInterest.cmc_sourcecampaignid
            };
            _orgService.Update(contact);
            _tracer.Trace("UpdateInitialSourceDetailsForContact Method Exit");
        }

      

        #endregion
    }
}
