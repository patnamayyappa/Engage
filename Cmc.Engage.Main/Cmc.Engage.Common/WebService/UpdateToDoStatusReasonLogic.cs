using System;
using Cmc.Core.Xrm.ServerExtension.Logging;
using Cmc.Engage.Models;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace Cmc.Engage.Common
{
    public class UpdateToDoStatusReasonLogic
    {
        private IOrganizationService _orgService;
        private ILogger _traceService;

        public UpdateToDoStatusReasonLogic(ILogger tracer, IOrganizationService organizationService)
        {
            _traceService = tracer ?? throw new ArgumentNullException(nameof(tracer));
            _orgService = organizationService ?? throw new ArgumentNullException(nameof(organizationService));
        }
        public bool UpdateStatusReason(Guid toDoId, int toDoStatus, string closeComment, Guid contactId)
        {
            _traceService.Trace($"Entered: {nameof(UpdateStatusReason)}");

            var toDo = _orgService.Retrieve(cmc_todo.EntityLogicalName, toDoId, new ColumnSet(
                "cmc_assignedtostudentid", "cmc_studentcancomplete", "cmc_ownershiptype",
                "cmc_requiredoptional", "statecode", "statuscode")).ToEntity<cmc_todo>();

            if (toDo.cmc_assignedtostudentid == null ||
                toDo.cmc_assignedtostudentid.Id != contactId)
            {
                _traceService.Trace($"Contact {contactId} cannot access To Do {toDoId},  exiting.");
                return false;
            }

            int state;
            int status;

            if (toDo.statuscode != null && toDo.statuscode.Value == cmc_todo_statuscode.Waived)
            {
                _traceService.Trace($"To Do {toDoId} was waived by the Staff. Student cannot change the Status.");
                return false;
            }
            if (toDo.cmc_studentcancomplete == false && toDo.statuscode != null &&
                toDo.statuscode.Value == cmc_todo_statuscode.Complete)
            {
                _traceService.Trace($"To Do {toDoId} was completed by the Staff. Student cannot change the Status.");
                return false;
            }

            switch (toDoStatus)
            {
                case (int)ToDoStatuses.Incomplete:
                    state = (int)cmc_todoState.Active;
                    status = (int)cmc_todo_statuscode.Incomplete;
                    break;
                case (int)ToDoStatuses.Complete:
                    if (toDo.cmc_studentcancomplete ?? false)
                    {
                        state = (int)cmc_todoState.Inactive;
                        status = (int)cmc_todo_statuscode.Complete;
                    }
                    else
                    {
                        state = (int)cmc_todoState.Active;
                        status = (int)cmc_todo_statuscode.MarkedasComplete;
                    }
                    break;
                case (int)ToDoStatuses.Canceled:
                    if (toDo.cmc_requiredoptional != null && toDo.cmc_requiredoptional.Value == (int)cmc_requiredoptional.Required)
                    {
                        _traceService.Trace($"To Do {toDoId} is required. STuddent cannot cancel the To Do.");
                        return false;
                    }

                    state = (int)cmc_todoState.Inactive;
                    status = (int)cmc_todo_statuscode.Canceled;
                    break;
                default:
                    _traceService.Trace($"Invalid To Do Status {toDoStatus}, exiting");
                    return false;
            }

            Entity updateToDo = new Entity("cmc_todo") { Id = toDoId };
            updateToDo["statecode"] = new OptionSetValue(state);
            updateToDo["statuscode"] = new OptionSetValue(status);

            if (status == (int)cmc_todo_statuscode.Canceled)
            {
                updateToDo["cmc_cancelreason"] = new OptionSetValue((int)cmc_todocancelreason.Student);
            }

            if (!string.IsNullOrWhiteSpace(closeComment))
            {
                updateToDo["cmc_completioncancellationcomment"] = closeComment;
            }

            _orgService.Update(updateToDo);
            return true;
        }

        internal enum ToDoStatuses
        {
            Incomplete = 0,
            Complete = 1,
            Canceled = 2
        }
    }
}
