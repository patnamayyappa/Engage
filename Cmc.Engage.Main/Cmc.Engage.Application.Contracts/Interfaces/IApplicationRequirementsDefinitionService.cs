using IExecutionContext = Cmc.Core.Xrm.ServerExtension.Core.IExecutionContext;

namespace Cmc.Engage.Application.Contracts.Interfaces
{
    public interface IApplicationRequirementsDefinitionService
    {
        void CreateUpdateApplicationRequirementsDefinitionDetail(IExecutionContext context);
    }
}
