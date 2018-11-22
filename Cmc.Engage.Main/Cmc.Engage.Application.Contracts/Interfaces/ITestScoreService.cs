using IExecutionContext = Cmc.Core.Xrm.ServerExtension.Core.IExecutionContext;
namespace Cmc.Engage.Application
{
    public interface ITestScoreService
    {
        void SetTestSuperandBestScores(IExecutionContext context);
    }
}
