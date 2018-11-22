using Cmc.Core.Xrm.ServerExtension.Core;

namespace Cmc.Engage.Retention
{
    public interface ICopySuccessPlanTemplateService
    {
        void Run(IExecutionContext context);
    }
}
