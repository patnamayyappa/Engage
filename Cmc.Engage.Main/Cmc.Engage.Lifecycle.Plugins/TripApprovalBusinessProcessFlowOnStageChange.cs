using Autofac;
using Cmc.Core.Xrm.ServerExtension.Core;

namespace Cmc.Engage.Lifecycle.Plugins
{
    /// <summary>
    /// On Business Process Flow Update 
    /// </summary>
    public class TripApprovalBusinessProcessFlowOnStageChange : PluginBase
    {
        /// <summary>
        /// Pass secured and unsecured Parameter
        /// </summary>
        /// <param name="unsecuredParameters"></param>
        /// <param name="securedParameters"></param>
        public TripApprovalBusinessProcessFlowOnStageChange(string unsecuredParameters, string securedParameters)
            : base(unsecuredParameters, securedParameters) { }
        /// <summary>
        /// Execute 
        /// </summary>
        /// <param name="context">Execution context </param>
        protected override void Execute(IExecutionContext context)
        {
            var tripApprovalProcessService = context.IocScope.Resolve<ITripApprovalProcessService>();
            tripApprovalProcessService.TripApprovalProcessExecute(context);
        }
    }
}
