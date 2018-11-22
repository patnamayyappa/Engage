using IExecutionContext = Cmc.Core.Xrm.ServerExtension.Core.IExecutionContext;

namespace Cmc.Engage.Application
{
    public interface IApplicationService
    {
        void SetupApplicationRequirements(IExecutionContext executionContext);
        void SetDefaultFields(IExecutionContext executionContext);
    }
}
