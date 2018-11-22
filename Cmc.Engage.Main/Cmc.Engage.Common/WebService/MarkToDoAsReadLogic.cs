using System;
using System.Collections.Generic;
using System.Linq;
using Cmc.Core.Xrm.ServerExtension.Core;
using Cmc.Core.Xrm.ServerExtension.Logging;
using Cmc.Engage.Models;
using Microsoft.Xrm.Sdk;

namespace Cmc.Engage.Common
{
    public class MarkToDoAsReadLogic 
    {
        private IOrganizationService _orgService;
        private ILogger _traceService;

        public MarkToDoAsReadLogic(ILogger traceService, IOrganizationService orgService)
        {
             _orgService = orgService ?? throw new ArgumentNullException(nameof(orgService));
            _traceService = traceService ?? throw new ArgumentNullException(nameof(traceService));
        }       
        public void MarkToDosAsRead(List<Guid?> toDoIds, Guid studentId)
        {
            _traceService.Trace($"Entered: {nameof(MarkToDosAsRead)}");
            var validToDoIds = _orgService.RetrieveMultipleAll($@"
                <fetch count='5000' aggregate='false' distinct='false' mapping='logical'>
                  <entity name='cmc_todo'>
                    <attribute name='cmc_todoid' />
                    <filter>
                      <condition attribute='cmc_todoid' operator='in'>
                        <value>{string.Join("</value><value>", toDoIds.Where(toDoId => toDoId != null))}</value>
                      </condition>
                      <condition attribute='cmc_assignedtostudentid' operator='eq' value='{studentId}' />
                    </filter>
                  </entity>
                </fetch>").Entities.Select(toDo => toDo.Id);

            for (int i = 0; i < toDoIds.Count; i++)
            {
                Guid? toDoId = toDoIds[i];
                if (toDoId == null)
                {
                    _traceService.Trace($"Id for ToDo at index {i} is null, skipping");
                    continue;
                }
                else if (validToDoIds.Contains(toDoId.Value) == false)
                {
                    _traceService.Trace($"Id for To Do at index {i}, {toDoId}, is not valid for Student {studentId}, skipping");
                    continue;
                }
                UpdateToDoToRead(toDoId.Value);
            }
        }

        private void UpdateToDoToRead(Guid toDoId)
        {
            Entity updateToDo = new Entity(cmc_todo.EntityLogicalName) { Id = toDoId };
            updateToDo["cmc_readunread"] = new OptionSetValue((int)cmc_readunread.Read);
            _orgService.Update(updateToDo);
        }
    }
}
