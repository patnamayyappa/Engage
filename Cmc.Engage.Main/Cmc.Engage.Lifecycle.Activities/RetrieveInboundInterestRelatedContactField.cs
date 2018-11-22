using System.Activities;
using Autofac;
using Cmc.Core.Xrm.ServerExtension.Core;
using Cmc.Engage.Models;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Workflow;

namespace Cmc.Engage.Lifecycle.Activities
{
    /// <summary>
    /// Act as a base class and retrieves the Inbound Interest
    /// </summary>
    public abstract class RetrieveInboundInterestRelatedContactField : ActivityBase
    {
        protected void Execute(IActivityExecutionContext executionContext, string attribute, OutArgument<EntityReference> outArgumentId)
        {
            var inboundInterestId = InboundInterestId.Get(executionContext.ActivityContext);

            var inboundInterestService = executionContext.IocScope.Resolve<IInboundInterestService>();
            var attributeValue = inboundInterestService.RetrieveInboundInterestContactLookup(
                attribute, inboundInterestId);

            outArgumentId.Set(executionContext.ActivityContext, attributeValue);
        }

        /// <summary>
        ///  The Inbound Interest's Contact.
        /// </summary>
        [ReferenceTarget(Lead.EntityLogicalName)]
        [RequiredArgument]
        [Input("Inbound Interest")]
        public InArgument<EntityReference> InboundInterestId { get; set; }
    }


}
