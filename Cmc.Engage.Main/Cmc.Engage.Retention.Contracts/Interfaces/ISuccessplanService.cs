

//using Core.Xrm.ServerExtension.Core;

namespace Cmc.Engage.Retention { 
    public interface ISuccessplanService
    {
        void CopySuccessPlanTemplate(Core.Xrm.ServerExtension.Core.IExecutionContext context);
        void SetSuccessPlanStatus(Core.Xrm.ServerExtension.Core.IExecutionContext context);        
    }
}
