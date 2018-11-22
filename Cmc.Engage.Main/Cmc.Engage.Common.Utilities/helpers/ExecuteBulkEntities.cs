using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using System.Collections.Generic;
using System.Linq;

namespace Cmc.Engage.Common.Utilities
{
    public static class ExecuteBulkEntities
    {
        private const int BatchSize = 50;

        private static OrganizationResponse BulkUpdate(IOrganizationService service, List<Entity> entities)
        {
            var multipleRequest = new ExecuteMultipleRequest
            {
                Settings = new ExecuteMultipleSettings
                {
                    ContinueOnError = true,
                    ReturnResponses = true
                },
                Requests = new OrganizationRequestCollection()
            };
            foreach (var entity in entities)
            {
                var updateRequest = new UpdateRequest { Target = entity };
                multipleRequest.Requests.Add(updateRequest);
            }

            return service.Execute(multipleRequest);
        }

        public static void BulkUpdateBatch(IOrganizationService service, List<Entity> entities)
        {
            if (entities?.Any() != true) return;
            var totalCount = entities.Count;
            var startRowIndex = 0;
            while (totalCount > 0)
            {
                var data = entities.Skip(startRowIndex).Take(BatchSize).ToList();
                var response = BulkUpdate(service, data);
                totalCount -= BatchSize;
                startRowIndex += BatchSize;
            }
        }
    }
}
