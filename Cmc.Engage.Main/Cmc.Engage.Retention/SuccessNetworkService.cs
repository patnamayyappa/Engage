using System;
using System.Collections.Generic;
using System.Linq;
using Cmc.Core.Xrm.ServerExtension.Core;
using Cmc.Core.Xrm.ServerExtension.Logging;
using Cmc.Engage.Common;
using Cmc.Engage.Common.Utilities.Helpers;
using Cmc.Engage.Models;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;


namespace Cmc.Engage.Retention
{
    public class SuccessNetworkService : ISuccessNetworkService
    {
        private readonly ILogger _loger;
        private IOrganizationService _orgService;
        private IContactService _contactService;

        public SuccessNetworkService(IOrganizationService orgService, ILogger loger, IContactService contactService)
        {
        
            _loger = loger;
            _contactService = contactService;          
            _orgService = orgService;
        }

        public void SuccessNetworkAssignment(IOrganizationService organizationService)
        {
            AssignSuccessNetwork(organizationService);
        }

        private void AssignSuccessNetwork(IOrganizationService organizationService)
        {
            _loger.Info("Retrieving active success network assignment and rules");
            var assignmentRules = RetrieveSuccessNetworkAssignmentRules();
            if (assignmentRules == null || assignmentRules.Any() != true)
            {
                _loger.Info("No groups found.");
                return;
            }

            //var contactService = new ContactService(null,null,_orgService,null);
            foreach (var assignment in assignmentRules)
                try
                {
                    var listType = assignment.Type ?? false;
                    var successnetworkassignmentId =
                        assignment.GetAliasedAttributeValue<Guid>("snl.cmc_successnetworkassignmentid");
                    var assigntoid =
                        assignment.GetAliasedAttributeValue<EntityReference>("sn.cmc_assigntoid");
                    var staffroleid =
                        assignment.GetAliasedAttributeValue<EntityReference>("sn.cmc_staffroleid");
                    var ownerdetails =
                        assignment.OwnerId;
                    Guid? ownerid = null;
                    if (ownerdetails != null)
                    {
                        if (ownerdetails.LogicalName == SystemUser.EntityLogicalName)
                        {
                            ownerid = ownerdetails.Id;
                        }
                    }

                    var students = listType ? _contactService.RetriveMembersForDynamicType(assignment.Query, ownerid, organizationService) : _contactService.RetriveMembersForStaticType(assignment.Id, ownerid, organizationService);
                    if (students == null)
                    {
                        _loger.Info("There are no students matched for success network assignment Id " + successnetworkassignmentId);
                        continue;
                    }
                    _loger.Info($"{students.Count} students matched assignment'");
                    using (var executeMultipleBuffer = new ExecuteMultipleBuffer(_orgService))
                    {
                        foreach (var student in students)
                        {
                            var exists = DoesStudentSuccessNetworkExist(student.Id, assigntoid.Id, staffroleid.Id);
                            if (!exists)
                            {
                                _loger.Info(
                                 $"Success Network doesn't exist for Student '{student.FullName}'.  Creating...");

                                var studentSuccessNetwork = new cmc_successnetwork
                                {
                                    cmc_staffmemberid = assigntoid,
                                    cmc_staffroleid = staffroleid,
                                    cmc_studentid = new EntityReference("contact", student.Id)
                                };

                                var newSuccessNetworkId = _orgService.Create(studentSuccessNetwork);

                                var associateRequest = new AssociateRequest
                                {
                                    Relationship = new Relationship("cmc_successnetwork_successnetworkassignment"),
                                    Target = new EntityReference(
                                        cmc_successnetworkassignment.EntityLogicalName, successnetworkassignmentId),
                                    RelatedEntities = new EntityReferenceCollection
                                    {
                                        new EntityReference(cmc_successnetwork.EntityLogicalName, newSuccessNetworkId)
                                    }
                                };

                                _orgService.Execute(associateRequest);
                            }
                        }

                        var studentSuccessNetworkAssignments =
                            RetrieveStudentSuccessNetworkAssignments(successnetworkassignmentId);
                        foreach (var studentSuccessNetworkAssignment in studentSuccessNetworkAssignments)
                        {
                            var relatedStudentId = (EntityReference)studentSuccessNetworkAssignment
                                .GetAttributeValue<AliasedValue>("cmc_successnetwork1.cmc_studentid").Value;
                            var studentExists = students.Where(s => s.Id == relatedStudentId.Id).Count() > 0;
                            if (!studentExists)
                            {
                                _loger.Info(
                                    $"Assignment rule doesn't match for Student '{relatedStudentId.Name}' anymore.  Deactivating...");

                                var inactivateStudentSuccessNetwork = new cmc_successnetwork();
                                inactivateStudentSuccessNetwork.Id =
                                    studentSuccessNetworkAssignment.cmc_successnetworkid.Value;
                                inactivateStudentSuccessNetwork.statecode = cmc_successnetworkState.Inactive;

                                executeMultipleBuffer.Update(inactivateStudentSuccessNetwork);

                                var disassociateRequest = new DisassociateRequest();
                                disassociateRequest.Relationship =
                                    new Relationship("cmc_successnetwork_successnetworkassignment");
                                disassociateRequest.Target = new EntityReference(
                                    cmc_successnetworkassignment.EntityLogicalName,
                                    studentSuccessNetworkAssignment.cmc_successnetworkassignmentid.Value);
                                disassociateRequest.RelatedEntities = new EntityReferenceCollection
                                {
                                    new EntityReference(cmc_successnetwork.EntityLogicalName,
                                        studentSuccessNetworkAssignment.cmc_successnetworkid.Value)
                                };

                                executeMultipleBuffer.Execute(disassociateRequest);
                            }
                        }
                    }

                    _loger.Info($"Finished processing Assignment...");
                }
                catch (Exception e)
                {
                    _loger.Fatal($"Error while processing Assignment: {e}");
                }
        }


        private bool DoesStudentSuccessNetworkExist(Guid contactId, Guid assignToId, Guid roleId)
        {
            var fb = new FetchBuilder();
            fb.EntityName = cmc_successnetwork.EntityLogicalName;
            fb.AddCondition("statecode", "eq", 0);
            fb.AddCondition("cmc_studentid", "eq", contactId);
            fb.AddCondition("cmc_staffmemberid", "eq", assignToId);
            fb.AddCondition("cmc_staffroleid", "eq", roleId);

            return _orgService.RetrieveMultiple(new FetchExpression(fb.ToString())).Entities.Count > 0;
        }


        private IEnumerable<T> RetrieveAll<T>(FetchBuilder fetchBuilder)
            where T : Entity
        {
            EntityCollection collection;
            do
            {
                collection = _orgService.RetrieveMultiple(new FetchExpression(fetchBuilder.ToString()));
                fetchBuilder.Page++;
                fetchBuilder.PagingCookie = collection.PagingCookie;
                foreach (T entity in collection.Entities) yield return entity;
            } while (collection.MoreRecords);
        }

        private IEnumerable<cmc_successnetwork_successnetworkassignment> RetrieveStudentSuccessNetworkAssignments(
            Guid successNetworkAssignmentId)
        {
            var fb = new FetchBuilder();
            fb.EntityName = cmc_successnetwork_successnetworkassignment.EntityLogicalName;
            fb.AddAttribute("cmc_successnetworkassignmentid");
            fb.AddAttribute("cmc_successnetworkid");
            fb.AddCondition("cmc_successnetworkassignmentid", "eq", successNetworkAssignmentId);
            var link = fb.AddLinkEntity(cmc_successnetwork.EntityLogicalName, "cmc_successnetworkid",
                "cmc_successnetworkid");
            link.AddAttribute("cmc_studentid");

            return RetrieveAll<cmc_successnetwork_successnetworkassignment>(fb).ToList();
        }

        private List<List> RetrieveSuccessNetworkAssignmentRules()
        {
            var fetch =
                $@"<fetch>
			        <entity name='list'>
                                <attribute name='type' />
                                 <attribute name='listid'/>
                                 <attribute name='query' />
                                 <attribute name='listname' />
                                 <attribute name='ownerid' />
				        <link-entity name='cmc_successnetworkassignment_list' to='listid' from='listid' alias='snl'>	
					        <all-attributes />
                            <link-entity name='cmc_successnetworkassignment' to='cmc_successnetworkassignmentid' from='cmc_successnetworkassignmentid' alias='sn'>	
					            <all-attributes />
                                <filter>
                                    <condition attribute='statecode' operator='eq' value='0' />
                                </filter>
				            </link-entity>
				        </link-entity>
                        <filter>
                            <condition attribute='statecode' operator='eq' value='0' />
                             <condition attribute ='cmc_marketinglisttype' operator='eq' value=' { (int)cmc_list_cmc_marketinglisttype.StudentGroup}' />
                            <condition attribute ='createdfromcode' operator='eq' value=' { (int)list_createdfromcode.Contact}' />
                        </filter>
			        </entity>
		        </fetch>";

            var list = _orgService.RetrieveMultipleAll(fetch);
            return list.Entities.Count <= 0 ? null : list.Entities.Cast<List>().ToList();
        }

    }
}
