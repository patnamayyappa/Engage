using Autofac;
using Cmc.Core.Xrm.ServerExtension.Core;

namespace Cmc.Engage.Common.Plugins
{
    public class SetLegacyStudentPlugin: PluginBase
    {
        public SetLegacyStudentPlugin(string unsecuredParameters, string securedParameters)
            : base(unsecuredParameters, securedParameters)
        {

        }

        protected override void Execute(IExecutionContext context)
        {
            var contactService = context.IocScope.Resolve<IContactService>();
            contactService.SetLegacyStudent(context);
        }
    }
}
