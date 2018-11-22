using Cmc.Core.Xrm.ServerExtension.Core;

namespace Cmc.Engage.Common
{
    public interface IMarketingListService
    {
        void ActivateStudentGroup(IExecutionContext context);
        void StudentGroupAutoExpireLogic();
    }
}
