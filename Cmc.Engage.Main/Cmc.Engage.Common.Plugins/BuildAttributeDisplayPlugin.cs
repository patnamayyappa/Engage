using Autofac;
using Cmc.Core.Xrm.ServerExtension.Core;
using Microsoft.Xrm.Sdk;
using IExecutionContext = Cmc.Core.Xrm.ServerExtension.Core.IExecutionContext;

namespace Cmc.Engage.Common.Plugins
{
    public class BuildAttributeDisplayPlugin : PluginBase, IPlugin
    {

        public BuildAttributeDisplayPlugin(string unsecuredParameters, string securedParameters) : base(unsecuredParameters, securedParameters)
        {
        }

        protected override void Execute(IExecutionContext context)
        {
            var BuildAttributeDisplay = context.IocScope.Resolve<ICustomAttributePickerUIService>();
            BuildAttributeDisplay.RetrieveLocalizedAttributeNamesToDisplay(context);
        }
    }
}
