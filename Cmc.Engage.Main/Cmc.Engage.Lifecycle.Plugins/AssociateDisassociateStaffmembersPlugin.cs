using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Cmc.Core.Xrm.ServerExtension.Core;
using Microsoft.Xrm.Sdk;
using IExecutionContext = Cmc.Core.Xrm.ServerExtension.Core.IExecutionContext;

namespace Cmc.Engage.Lifecycle.Plugins
{
    /// <summary>
    /// Trip Staff members are added up when staff members are added to Trip Activity
    /// </summary>
    public class AssociateDisassociateStaffmembersPlugin : PluginBase, IPlugin
    {
        /// <summary>
        /// Constructor to pass secured and unsecured parameters.
        /// </summary>
        public AssociateDisassociateStaffmembersPlugin(string unsecuredParameters, string securedParameters)
            : base(unsecuredParameters, securedParameters) { }

        protected override void Execute(IExecutionContext context)
        {
            var tripactivityService = context.IocScope.Resolve<ITripActivityService>();
            tripactivityService.AssociateDisassociateStaffmembers(context);
        }
    }
}
