using Cmc.Core.Xrm.ServerExtension.Core;

namespace Cmc.Engage.Retention
{
    public interface IPredictRetentionActionPluginService
    {
        void Run(IExecutionContext context);
    }
}
