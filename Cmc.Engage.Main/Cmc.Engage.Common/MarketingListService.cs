using System;
using System.Collections.Generic;
using IExecutionContext = Cmc.Core.Xrm.ServerExtension.Core.IExecutionContext;
using System.Linq;
using Cmc.Core.Xrm.ServerExtension.Core;
using Cmc.Core.Xrm.ServerExtension.Logging;
using Microsoft.Xrm.Sdk;
using Cmc.Engage.Common.Utilities;
using Cmc.Engage.Common.Utilities.Helpers;
using Cmc.Engage.Models;



namespace Cmc.Engage.Common
{
    /// <summary>
    /// Activate the student group.
    /// </summary>
    public class MarketingListService : IMarketingListService
    {
        private readonly ILogger _tracer;
        private IOrganizationService _orgService;
      
        public MarketingListService(ILogger tracer, IOrganizationService orgService)
        {
            _tracer = tracer; 
            _orgService = orgService;
        }
        public void ActivateStudentGroup(IExecutionContext context)
        {
            var serviceProvider = context.XrmServiceProvider;
            var pluginContext = serviceProvider.GetPluginExecutionContext();
            //_orgService = serviceProvider.CreateOrganizationServiceAsCurrentUser();
            _tracer.Trace("Reading input");
            var marketingLists = pluginContext.GetInputParameter<EntityCollection>("StudentGroups");
            ActivateStudentGroup(marketingLists);

        }

       

        private void ActivateStudentGroup(EntityCollection marketingLists)
        {
            _tracer.Trace($"Entered: ActivateStudentGroup");
            ExecuteBulkEntities.BulkUpdateBatch(_orgService, marketingLists.Entities.ToList());
            _tracer.Trace("All Student Group Updated Successfully");
        }

        public void StudentGroupAutoExpireLogic()
        {
            AutoExpire();
        }

        public void AutoExpire()
        {
             _tracer.Info($"Retrieving need to expire student groups");
            var studentGroups = RetrieveNeedToExpireStudentGroups();
            if (studentGroups?.Any() == true)
            {
                  _tracer.Info($"{studentGroups.Count()} student groups found.");
                foreach (var studentGroup in studentGroups)
                {
                    UpdateStudentGroup(studentGroup);
                }
                 _tracer.Info($"Inactivate completed for all {studentGroups.Count()} student groups.");
            }
        }
        /// <summary>
        /// used to deactivate student group 
        /// </summary>
        /// <param name="studentGroup"></param>
        private void UpdateStudentGroup(List studentGroup)
        {
              _tracer.Info($"Inactivate started for \" {studentGroup.ListName} \" ");
            using (var executeMultipleBuffer = new ExecuteMultipleBuffer(_orgService))
            {
                executeMultipleBuffer.Update(new List
                {
                    ListId = studentGroup.ListId,
                    StateCode = ListState.Inactive
                });
            }
            _tracer.Info($"Inactivate completed for \" {studentGroup.ListName} \" ");
        }
        /// <summary>
        /// used to get need to deactivate the student group
        /// </summary>
        /// <returns> Student group </returns>
        private IEnumerable<List> RetrieveNeedToExpireStudentGroups()
        {
            var fetch =
                $@"<fetch>
			        <entity name='list'>
                       <attribute name='listid'/>
                       <attribute name='listname'/>
                       <attribute name='statecode' />
				       <filter type='and'>                              
                              <condition attribute='statecode' operator='eq' value='0' />
                              <condition attribute ='cmc_marketinglisttype' operator='eq' value='{(int)cmc_list_cmc_marketinglisttype.StudentGroup}' />
                              <condition attribute = 'cmc_expirationdate' operator='le' value= '{DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") }' />                            
                       </filter>
			        </entity>
                    </fetch>";
            /// Date formate accepted by fetchxml 'yyyy-MM-dd'      
              var studentGroups = _orgService.RetrieveMultipleAll(fetch);
            var data = studentGroups.Entities.Count <= 0 ? null : studentGroups.Entities.Cast<List>();
            return data;
        }

    }
}
