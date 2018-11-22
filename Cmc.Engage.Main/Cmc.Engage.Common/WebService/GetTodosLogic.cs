using System;
using System.Collections.Generic;
using System.Linq;
using Cmc.Core.Xrm.ServerExtension.Logging;
using Cmc.Engage.Common.Utilities;
using Cmc.Engage.Models;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace Cmc.Engage.Common
{
    public class GetTodosLogic
    {
        //private IOrganizationService _orgService;
        private ILogger _traceService;
        private IOrganizationService _orgService;
        public GetTodosLogic(ILogger traceService, IOrganizationService organizationService)
        {
            _traceService = traceService ?? throw new ArgumentNullException(nameof(traceService));
            _orgService = organizationService ?? throw new ArgumentNullException(nameof(organizationService));
        }
        public IList<cmc_todo> Get(Guid studentId, int statusCode)
        {
            var filters = GetFilters(statusCode, studentId);

            var fetch = $@"<fetch>
              <entity name='cmc_todo'>
                <attribute name='cmc_todoid' />
                <attribute name='cmc_todoname' />
                <attribute name='cmc_todocategoryid' />
                <attribute name='cmc_studentcancomplete' />
                <attribute name='cmc_requiredoptional' />
                <attribute name='cmc_readunread' />
                <attribute name='cmc_readdate' />
                <attribute name='ownerid' />
                <attribute name='cmc_duedate' />
                <attribute name='cmc_description' />
                <attribute name='cmc_assignedtostudentid' />
                <attribute name='statuscode' />
                <attribute name='cmc_completioncancellationcomment' />
                <attribute name='cmc_completedcanceleddate' />	
                <link-entity name='systemuser' from='systemuserid' to='owninguser' link-type='outer' alias='owner'>
                  <attribute name='title' />
                  <attribute name='fullname' />
                </link-entity>
                <link-entity name='cmc_successplan' from='cmc_successplanid' to='cmc_successplanid' link-type='outer' alias='successPlan'>
                  <attribute name='cmc_portaldescription' />
                </link-entity>
	            <filter type='and'>
                  <condition attribute='cmc_assignedtostudentid' operator='eq' value='{studentId}' />
                  <condition attribute='cmc_ownershiptype' operator='eq' value='{(int)cmc_ownershiptype.StudentOwned}'/>
                  {filters}
                </filter>
              </entity>
            </fetch>";

            _traceService.Trace($"GetTodoFetch: {fetch}");

            return _orgService.RetrieveMultiple(new FetchExpression(fetch)).Entities.ToList().ConvertAll(x => x.ToEntity<cmc_todo>());
        }

        private string GetFilters(int statusCode, Guid studentId)
        {
            var academicPeriod = AcademicPeriodHelper.GetCurrentAcademicPeriod(_orgService, studentId);
            if (academicPeriod == null)
            {
                //if no academicPeriod exists use min date to cancel out that criteria, 1/1/1753 is minimum date allowed by CRM
                academicPeriod = AcademicPeriodHelper.GetDefaultAcaedmicPeriod();
            }

            switch (statusCode)
            {
                case (int)cmc_todo_statuscode.Incomplete:
                    return
                        $@"<condition attribute='statuscode' operator='eq' value='{(int)cmc_todo_statuscode.Incomplete}'/>
                            <filter type='or'>
                                <filter type='and'>
                                    <condition attribute='cmc_requiredoptional' operator='eq' value='{(int)cmc_requiredoptional.Required}'/>
                                    <condition attribute='cmc_duedate' operator='on-or-before' value='{academicPeriod.mshied_EndDate}'/>
                                </filter>
                                <filter type='and'>
                                    <condition attribute='cmc_duedate' operator='on-or-before' value='{academicPeriod.mshied_EndDate}'/>
                                    <condition attribute='cmc_duedate' operator='on-or-after' value='{academicPeriod.mshied_StartDate}'/>
                                </filter>
                            </filter>";
                case (int)cmc_todo_statuscode.Complete:
                    return $@"<filter type='and'>
                                   <condition attribute='cmc_duedate' operator='on-or-before' value='{academicPeriod.mshied_EndDate}'/>
                                   <condition attribute='cmc_duedate' operator='on-or-after' value='{academicPeriod.mshied_StartDate}'/>
                                <filter type='or'>
                                    <condition attribute='statuscode' operator='eq' value='{(int)cmc_todo_statuscode.Complete}'/>
                                    <condition attribute='statuscode' operator='eq' value='{(int)cmc_todo_statuscode.MarkedasComplete}'/>
                                </filter>
                              </filter>";
                case (int)cmc_todo_statuscode.Canceled:
                case (int)cmc_todo_statuscode.Waived:
                    return $@"<filter type='and'>
                                   <condition attribute='cmc_duedate' operator='on-or-before' value='{academicPeriod.mshied_EndDate}'/>
                                   <condition attribute='cmc_duedate' operator='on-or-after' value='{academicPeriod.mshied_StartDate}'/>
                                <filter type='or'>
                                    <condition attribute='statuscode' operator='eq' value='{(int)cmc_todo_statuscode.Canceled}'/>
                                    <condition attribute='statuscode' operator='eq' value='{(int)cmc_todo_statuscode.Waived}'/>
                                </filter>
                              </filter>";
            }

            return null;
        }
    }
}
