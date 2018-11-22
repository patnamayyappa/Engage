using Autofac;
using Cmc.Core.Xrm.ServerExtension.Core;
using Microsoft.Xrm.Sdk;
using IExecutionContext = Cmc.Core.Xrm.ServerExtension.Core.IExecutionContext;

namespace Cmc.Engage.Common.Plugins
{
    public class RetrieveAttributesRelationshipsForEntitiesPlugin : PluginBase, IPlugin
    {

        public RetrieveAttributesRelationshipsForEntitiesPlugin(string unsecuredParameters, string securedParameters) 
            : base(unsecuredParameters, securedParameters)
        {

        }

        protected override void Execute(IExecutionContext context)
        {
            var CustomAttributePicker = context.IocScope.Resolve<ICustomAttributePickerUIService>();
            CustomAttributePicker.RetrieveAttributesRelationshipsForEntities(context);
        }
    }
}

