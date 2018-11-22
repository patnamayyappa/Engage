using System;
using Cmc.Core.Xrm.ServerExtension.Core;
using Cmc.Core.Xrm.ServerExtension.Logging;
using Cmc.Engage.Common.Utilities;
using Cmc.Engage.Models;
using Microsoft.Xrm.Sdk;

namespace Cmc.Engage.Common
{
    public class RetrieveNumberOfUnreadToDosLogic 
    {
        private IOrganizationService _orgService;
        private ILogger _trace;

        public RetrieveNumberOfUnreadToDosLogic(ILogger tracer, IOrganizationService organizationService)
        {
            _trace = tracer ?? throw new ArgumentNullException(nameof(tracer));
            _orgService = organizationService ?? throw new ArgumentNullException(nameof(organizationService));
        }
        public int RetrieveNumberOfUnreadToDos(Guid studentId)
        {
            var academicPeriod = AcademicPeriodHelper.GetCurrentAcademicPeriod(_orgService, studentId);
            if (academicPeriod == null)
            {
                //if no academicPeriod exists use min date to cancel out that criteria, 1/1/1753 is minimum date allowed by CRM
                academicPeriod = new mshied_academicperiod()
                {
                    mshied_StartDate = new DateTime(1753, 1, 1),
                    mshied_EndDate = new DateTime(2100, 1, 1)
                };
            }

            return _orgService.RetrieveMultipleAll(
                $@"<fetch>
                     <entity name='cmc_todo'>
                       <attribute name='cmc_todoid' />
                       <filter type='and'>
                         <condition attribute='cmc_assignedtostudentid' operator='eq' value='{studentId}' />
                         <condition attribute='cmc_ownershiptype' operator='eq' value='{(int)cmc_ownershiptype.StudentOwned}'/>
                         <condition attribute='statuscode' operator='eq' value='{(int)cmc_todo_statuscode.Incomplete}'/>
                         <condition attribute='cmc_readunread' operator='eq' value='{(int)cmc_readunread.Unread}'/>
                         <filter type='or'>
                           <filter type='and'>
                             <condition attribute='cmc_requiredoptional' operator='eq' value='{(int)cmc_requiredoptional.Required}'/>
                             <condition attribute='cmc_duedate' operator='on-or-before' value='{academicPeriod.mshied_EndDate}'/>
                           </filter>
                           <filter type='and'>
                             <condition attribute='cmc_requiredoptional' operator='eq' value='{(int)cmc_requiredoptional.Optional}'/>
                             <condition attribute='cmc_duedate' operator='on-or-before' value='{academicPeriod.mshied_EndDate}'/>
                             <condition attribute='cmc_duedate' operator='on-or-after' value='{academicPeriod.mshied_StartDate}'/>
                           </filter>
                         </filter>
                       </filter>
                     </entity>
                   </fetch>").Entities.Count;
        }
    }
}
