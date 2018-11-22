using Autofac;
using Cmc.Core.Xrm.ServerExtension.Core;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IExecutionContext = Cmc.Core.Xrm.ServerExtension.Core.IExecutionContext;

namespace Cmc.Engage.Lifecycle.Plugins
{
    /// <summary>
    /// 
    /// </summary>
    public class CreateUpdateTripPlugin : PluginBase, IPlugin
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="unsecuredParameters"></param>
        /// <param name="securedParameters"></param>
        public CreateUpdateTripPlugin(string unsecuredParameters, string securedParameters)
            : base(unsecuredParameters, securedParameters) { }
        protected override void Execute(IExecutionContext context)
        {
            var createUpdateTrip = context.IocScope.Resolve<ITripService>();
             createUpdateTrip.CreateUpdateTripService(context);
        }
    }
}
