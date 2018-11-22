using IExecutionContext = Cmc.Core.Xrm.ServerExtension.Core.IExecutionContext;

namespace Cmc.Engage.Application
{
    public interface IRecommendationService
    {
        void SendThankyouEmail(IExecutionContext context);
    }
}
