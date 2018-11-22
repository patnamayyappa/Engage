using Autofac;
using Cmc.Core.Xrm.ServerExtension.Core;
using Microsoft.Xrm.Sdk;
using IExecutionContext = Cmc.Core.Xrm.ServerExtension.Core.IExecutionContext;

namespace Cmc.Engage.Lifecycle.Plugins
{
    /// <summary>
    /// perform to complete or cancel the trip 
    /// </summary>
    public class CompleteOrCancelTripPlugin : PluginBase, IPlugin
    {
        /// <summary>
        /// perform to complete or cancel the trip 
        /// </summary>
        /// <param name="unsecuredParameters"></param>
        /// <param name="securedParameters"></param>
        public CompleteOrCancelTripPlugin(string unsecuredParameters, string securedParameters)
            : base(unsecuredParameters, securedParameters) { }
        protected override void Execute(IExecutionContext context)
        {
            var createUpdateTrip = context.IocScope.Resolve<ITripService>();
            createUpdateTrip.CompleteOrCancelTrip(context);
        }
    }

}
