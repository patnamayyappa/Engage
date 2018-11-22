using Cmc.Core.Xrm.ServerExtension.Core;

namespace Cmc.Engage.Retention
{
    public interface ISetSuccessPlanStatusPluginService
    {
        void Run(IExecutionContext context);
    }
}
