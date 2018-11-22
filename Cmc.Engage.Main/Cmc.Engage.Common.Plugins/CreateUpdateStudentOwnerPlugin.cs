using Autofac;
using Cmc.Core.Xrm.ServerExtension.Core;
using Microsoft.Xrm.Sdk;
using IExecutionContext = Cmc.Core.Xrm.ServerExtension.Core.IExecutionContext;

namespace Cmc.Engage.Common.Plugins
{
    /// <summary>
    /// to created and update the student owner for given contact.
    /// </summary>
    public class CreateUpdateStudentOwnerPlugin : PluginBase, IPlugin
    {
        /// <summary>
        /// override the execute method.
        /// </summary>
        /// <param name="context"></param>
        /// 
        public CreateUpdateStudentOwnerPlugin(string unsecuredParameters, string securedParameters)
            : base(unsecuredParameters, securedParameters)
        {

        }

        protected override void Execute(IExecutionContext context)
        {
            var contactService = context.IocScope.Resolve<IContactService>();
            contactService.CreateUpdateStudentOwner(context);
        }
    }
}
