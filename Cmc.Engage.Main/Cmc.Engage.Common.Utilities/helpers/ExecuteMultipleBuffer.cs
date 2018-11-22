using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;

namespace Cmc.Engage.Common.Utilities.Helpers
{
    public class ExecuteMultipleBuffer : IDisposable
    {
        private class ExecuteMultipleItem
        {
            internal OrganizationRequest Request { get; set; }
            internal Action<Task<OrganizationResponse>> Handler { get; set; }
        }

        private List<ExecuteMultipleItem> _queue = new List<ExecuteMultipleItem>();
        private IOrganizationService _organizationService;
        public int MaxBufferSize { get; set; } = 100;
        public bool ContinueOnError { get; set; } = false;

        public ExecuteMultipleBuffer(IOrganizationService organizationService)
        {
            _organizationService = organizationService;
        }

        public void Create(Entity target, Action<Task<OrganizationResponse>> handler = null)
        {
            Execute(new CreateRequest { Target = target }, handler);
        }

        public void Update(Entity target, Action<Task<OrganizationResponse>> handler = null)
        {
            Execute(new UpdateRequest { Target = target }, handler);
        }

        public void Execute(OrganizationRequest request, Action<Task<OrganizationResponse>> handler = null)
        {
            _queue.Add(new ExecuteMultipleItem { Request = request, Handler = handler });

            if (_queue.Count == MaxBufferSize)
            {
                Flush();
            }
        }

        public void Flush()
        {
            var executeMultipleRequest = new ExecuteMultipleRequest()
            {
                Settings = new ExecuteMultipleSettings
                {
                    ContinueOnError = ContinueOnError,
                    ReturnResponses = true
                },
                Requests = new OrganizationRequestCollection()
            };

            executeMultipleRequest.Requests.AddRange(_queue.Select(t => t.Request));
            HashSet<int> pendingRequests = new HashSet<int>(Enumerable.Range(0, _queue.Count));

            var executeMultipleResponse = (ExecuteMultipleResponse)_organizationService.Execute(executeMultipleRequest);
            for (int i = 0; i < executeMultipleResponse.Responses.Count; i++)
            {
                Task<OrganizationResponse> task;
                var response = executeMultipleResponse.Responses[i];
                var source = _queue[response.RequestIndex];
                if (response.Fault != null)
                {
                    var exception = new FaultException<OrganizationServiceFault>(response.Fault, response.Fault.Message);
                    task = TaskFromException<OrganizationResponse>(exception);
                }
                else
                {
                    task = Task.FromResult(response.Response);
                }
                source.Handler?.Invoke(task);
                pendingRequests.Remove(response.RequestIndex);
            }

            if (pendingRequests.Count > 0)
            {
                var cancelledTcs = new TaskCompletionSource<OrganizationResponse>();
                cancelledTcs.SetCanceled();
                foreach (var i in pendingRequests)
                {
                    _queue[i].Handler?.Invoke(cancelledTcs.Task);
                }
            }

            _queue.Clear();
        }

        private static Task<T> TaskFromException<T>(Exception exception)
        {
            var promise = new TaskCompletionSource<T>();
            promise.SetException(exception);
            return promise.Task;
        }

        public void Dispose()
        {
            Flush();
        }
    }
}
