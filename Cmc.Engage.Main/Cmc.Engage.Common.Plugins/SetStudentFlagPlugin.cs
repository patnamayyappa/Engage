using Autofac;
using Cmc.Core.Xrm.ServerExtension.Core;
using Microsoft.Xrm.Sdk;
using IExecutionContext = Cmc.Core.Xrm.ServerExtension.Core.IExecutionContext;

namespace Cmc.Engage.Common.Plugins
{
    /// <summary>
    /// set's the isStudent flag based on the selection made in Contact Type. 
    /// </summary>
    public class SetStudentFlagPlugin : PluginBase, IPlugin
    {
        /// <summary>
        /// override the execute method.
        /// </summary>
        /// <param name="context"></param>
        /// 
        public SetStudentFlagPlugin(string unsecuredParameters, string securedParameters)
            : base(unsecuredParameters, securedParameters)
        {

        }

        protected override void Execute(IExecutionContext context)
        {
            var contactService = context.IocScope.Resolve<IContactService>();
            contactService.SetStudentFlag(context);
        }
    }
}
