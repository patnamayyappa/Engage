using Autofac;
using Cmc.Core.Xrm.ServerExtension.Core;
using Microsoft.Xrm.Sdk;
using IExecutionContext = Cmc.Core.Xrm.ServerExtension.Core.IExecutionContext;

namespace Cmc.Engage.Retention.Plugins
{
  public  class ModifyRetrieveToDosPlugin: PluginBase, IPlugin
    {
        public ModifyRetrieveToDosPlugin(string unsecuredParameters, string securedParameters)
            : base(unsecuredParameters, securedParameters)
        {

        }

        protected override void Execute(IExecutionContext context)
        {
            var ModifyRetrieveToDo = context.IocScope.Resolve<IToDoService>();
            ModifyRetrieveToDo.ModifyRetrieveToDos(context);
        }
    }
}
