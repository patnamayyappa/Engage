using System;
using System.Linq;
using Cmc.Core.Xrm.ServerExtension.Core;
using Cmc.Core.Xrm.ServerExtension.Logging;
using Cmc.Engage.Common.Utilities;
using Cmc.Engage.Models;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;


namespace Cmc.Engage.Retention
{
    public class ToDoService : IToDoService
    {
        private readonly ILogger _tracer;
        private IOrganizationService _orgService;

        public ToDoService(ILogger tracer, IOrganizationService orgService)
        {
            _tracer = tracer ?? throw new ArgumentException(nameof(tracer));         
            _orgService = orgService;
        }
        #region Modify Retrieve ToDos
        public void ModifyRetrieveToDos(Core.Xrm.ServerExtension.Core.IExecutionContext context)
        {
            var serviceProvider = context.XrmServiceProvider;
            var pluginContext = serviceProvider.GetPluginExecutionContext();
            //_orgService = serviceProvider.CreateSystemOrganizationService();
            _tracer.Trace("Starting ModifyRetrieveToDosPlugin");
            var query = pluginContext.InputParameters["Query"];

            pluginContext.InputParameters["Query"] = ModifyQuery(query);
        }
        private object ModifyQuery(object query)
        {
            QueryExpression queryExpression;
            if (query is FetchExpression)
            {
                _tracer.Trace("Query is FetchExpression");
                queryExpression = ((FetchXmlToQueryExpressionResponse)_orgService.Execute(
                    new FetchXmlToQueryExpressionRequest()
                    {
                        FetchXml = ((FetchExpression)query).Query
                    })).Query;

            }
            else if (query is QueryExpression)
            {
                _tracer.Trace("Query is QueryExpression");
                queryExpression = (QueryExpression)query;
            }
            else
            {
                _tracer.Trace("Query is unknown type. Modifications not performed");
                return query;
            }

            _tracer.Trace("Checking if Academic Period filter is on the first criteria");
            bool filterToDosByAcademicPeriod = RemoveRequestToFilterToDosByAcademicPeriod(queryExpression.Criteria);

            if (filterToDosByAcademicPeriod == false)
            {
                _tracer.Trace("Checking if Academic Period filter is on the sub-criteria");
                foreach (var filter in queryExpression.Criteria.Filters)
                {
                    filterToDosByAcademicPeriod = RemoveRequestToFilterToDosByAcademicPeriod(
                        filter);

                    if (filterToDosByAcademicPeriod == true)
                    {
                        break;
                    }
                }
            }

            if (filterToDosByAcademicPeriod)
            {
                _tracer.Trace("Filtering To Dos by Academic Period.");
                FilterToDosByAcademicPeriod(queryExpression);
            }

            return queryExpression;
        }

        private bool RemoveRequestToFilterToDosByAcademicPeriod(FilterExpression filter)
        {
            bool filterToDos = false;
            for (int i = 0; i < filter.Conditions.Count; i++)
            {
                var condition = filter.Conditions[i];
                if (condition.AttributeName == "cmc_todoname" && condition.Operator == ConditionOperator.Equal &&
                    condition.Values.FirstOrDefault() as string == "{Filter By Academic Period}")
                {
                    filter.Conditions.Remove(condition);
                    filterToDos = true;
                    break;
                }
            }

            return filterToDos;
        }

        private void FilterToDosByAcademicPeriod(QueryExpression query)
        {
            _tracer.Trace("Finding Student Id in first level criteria.");
            var studentId = RetrieveAssignedToStudentId(query.Criteria);

            if (studentId == null)
            {
                _tracer.Trace("Finding Student Id in sub-criteria.");
                foreach (var filter in query.Criteria.Filters)
                {
                    studentId = RetrieveAssignedToStudentId(filter);
                    if (studentId != null)
                    {
                        break;
                    }
                }
            }

            if (studentId == null)
            {
                _tracer.Trace("Student Id not found. Filter not applied");
                return;
            }

            var academicPeriod = AcademicPeriodHelper.GetCurrentAcademicPeriod(_orgService, studentId.Value);

            if (academicPeriod == null)
            {
                _tracer.Trace("No Academic Period found. Default values will be used");
                academicPeriod = AcademicPeriodHelper.GetDefaultAcaedmicPeriod();
            }

            _tracer.Trace("Adding Academic Period filter");

            query.Criteria.Filters.Add(new FilterExpression(LogicalOperator.Or)
            {
                Filters =
                {
                    new FilterExpression(LogicalOperator.And)
                    {
                        Conditions =
                        {
                            new ConditionExpression("cmc_requiredoptional", ConditionOperator.Equal, (int)cmc_requiredoptional.Optional),
                            new ConditionExpression("cmc_duedate", ConditionOperator.OnOrBefore, academicPeriod.mshied_EndDate)
                        }
                    },
                    new FilterExpression(LogicalOperator.And)
                    {
                        Conditions =
                        {
                            new ConditionExpression("cmc_duedate", ConditionOperator.OnOrAfter, academicPeriod.mshied_StartDate),
                            new ConditionExpression("cmc_duedate", ConditionOperator.OnOrBefore, academicPeriod.mshied_EndDate)
                        }
                    }
                }
            });
        }

        private Guid? RetrieveAssignedToStudentId(FilterExpression filter)
        {
            return filter.Conditions.FirstOrDefault(
                condition => condition.AttributeName == "cmc_assignedtostudentid" &&
                condition.Operator == ConditionOperator.Equal)?.Values?.FirstOrDefault() as Guid?;
        }
    }
    #endregion
}
