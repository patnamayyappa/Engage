using Autofac;
using Cmc.Core.Xrm.ServerExtension.Core;
using Microsoft.Xrm.Sdk;
using IExecutionContext = Cmc.Core.Xrm.ServerExtension.Core.IExecutionContext;

namespace Cmc.Engage.Common.Plugins
{
    public class CreateStudentSuccessPlansFromTemplatePlugin : PluginBase, IPlugin
    {

        public CreateStudentSuccessPlansFromTemplatePlugin(string unsecuredParameters, string securedParameters)
            : base(unsecuredParameters, securedParameters)
        {

        }

        protected override void Execute(IExecutionContext context)
        {
            var CreateStudentSuccessPlansFromTemplate = context.IocScope.Resolve<IContactService>();
            CreateStudentSuccessPlansFromTemplate.CreateSuccessPlansForSelectedStudent(context);
        }
    }
}
