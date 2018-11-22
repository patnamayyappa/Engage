using Microsoft.Xrm.Sdk;
using IExecutionContext = Cmc.Core.Xrm.ServerExtension.Core.IExecutionContext;
namespace Cmc.Engage.Lifecycle
{
    public interface IInboundInterestService
    {
        EntityReference RetrieveInboundInterestContactLookup(string attributeName, EntityReference inboundInterestId);
        void OverrideInitialSourceDetailsForContact(IExecutionContext context);
    }
}
