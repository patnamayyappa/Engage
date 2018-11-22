using System;
using System.Collections.Generic;
using System.Linq;
using Cmc.Core.Xrm.ServerExtension.Core;
using Cmc.Core.Xrm.ServerExtension.Logging;
using IExecutionContext = Cmc.Core.Xrm.ServerExtension.Core.IExecutionContext;
using Newtonsoft.Json;
using Cmc.Engage.Models;
using Microsoft.Xrm.Sdk;

namespace Cmc.Engage.Common
{
    public class GetTodosPortalLogic : PortalWebServiceLogicBase
    {
        private ILogger traceService;
        private readonly GetPortalUserLanguageCode _portalLanguageLogic;
        private readonly GetTodosLogic _todosLogic;
        private readonly ILanguageService _translationLogic;
        private readonly IOrganizationService _orgService;
        public GetTodosPortalLogic(ILogger tracer, IOrganizationService orgService, GetPortalUserLanguageCode portalLanguageLogic, GetTodosLogic todosLogic, ILanguageService translationLogic)
        {
            traceService = tracer ?? throw new ArgumentNullException(nameof(tracer));
            _portalLanguageLogic = portalLanguageLogic ?? throw new ArgumentNullException(nameof(portalLanguageLogic));
            _todosLogic = todosLogic ?? throw new ArgumentNullException(nameof(todosLogic));
            _translationLogic = translationLogic ?? throw new ArgumentNullException(nameof(translationLogic));
            _orgService = orgService;
        }

        public override object DoWork(IExecutionContext context, string inputData)
        {

            var serviceProvider = context.XrmServiceProvider;
            var pluginContext = serviceProvider.GetPluginExecutionContext();

            var input = JsonConvert.DeserializeObject<Input>(inputData);

            if (input == null || !input.StudentId.HasValue)
            {
                traceService.Trace("Input was missing student id.");
                return new Dictionary<string, Output>();
            }

            var languageCode = _portalLanguageLogic.GetUserLanguageCode(input.StudentId, input.WebsiteId);
            var todos = _todosLogic.Get(input.StudentId.Value, input.StatusCode);
            _translationLogic.TranslatePicklistValues(todos, languageCode);
            return todos.ToDictionary(key => key.Id,
                value => new Output(value));

        }

        public class Input
        {
            public Guid? StudentId { get; set; }
            public Guid? WebsiteId { get; set; }
            public int StatusCode { get; set; }
        }
        public class Output
        {
            public DateTime? completedCanceledDate { get; set; }
            public string completionCancellationComment { get; set; }
            public string description { get; set; }
            public DateTime? dueDate { get; set; }
            public bool isUnread { get; set; }
            public bool isNew { get; set; }
            public bool isRequired { get; set; }
            public bool studentCanComplete { get; set; }
            public string category { get; set; }
            public string name { get; set; }
            public string id { get; set; }
            public string owner { get; set; }
            public string ownerTitle { get; set; }
            public bool isIncomplete { get; set; }
            public bool isMarkedAsComplete { get; set; }
            public bool isComplete { get; set; }
            public bool isCanceled { get; set; }
            public bool isWaived { get; set; }
            public string statusReason { get; set; }
            public string successPlan { get; set; }

            public Output()
            {

            }

            public Output(cmc_todo todo)
            {
                completedCanceledDate = todo.cmc_completedcanceleddate;
                completionCancellationComment = todo.cmc_completioncancellationcomment;
                description = todo.cmc_description;
                dueDate = todo.cmc_duedate;
                isUnread = todo.cmc_readunread?.Value == (int)cmc_readunread.Unread;
                isNew = todo.cmc_readunread?.Value == (int)cmc_readunread.Unread ? true : todo.cmc_readdate?.AddHours(24) > DateTime.Now;
                isRequired = todo.cmc_requiredoptional?.Value == (int)cmc_requiredoptional.Required;
                studentCanComplete = todo.cmc_studentcancomplete ?? false;
                category = todo.cmc_todocategoryid?.Name;
                name = todo.cmc_todoname;
                id = todo.Id.ToString();
                owner = todo.GetAttributeValue<AliasedValue>("owner.fullname")?.Value as string;
                ownerTitle = todo.GetAttributeValue<AliasedValue>("owner.title")?.Value as string;
                isIncomplete = todo.statuscode == cmc_todo_statuscode.Incomplete;
                isMarkedAsComplete = todo.statuscode == cmc_todo_statuscode.MarkedasComplete;
                isComplete = todo.statuscode == cmc_todo_statuscode.Complete;
                isCanceled = todo.statuscode == cmc_todo_statuscode.Canceled;
                isWaived = todo.statuscode == cmc_todo_statuscode.Waived;
                statusReason = todo.FormattedValues.Contains("statuscode") ? todo.FormattedValues["statuscode"] : null;
                successPlan = todo.GetAttributeValue<AliasedValue>("successPlan.cmc_portaldescription")?.Value as string;
            }
        }
    }
}
