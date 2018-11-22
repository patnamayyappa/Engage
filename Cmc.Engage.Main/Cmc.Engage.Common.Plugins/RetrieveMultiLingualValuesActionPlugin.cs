using Autofac;
using Cmc.Core.Xrm.ServerExtension.Core;

namespace Cmc.Engage.Common.Plugins
{
    public class RetrieveMultiLingualValuesActionPlugin : PluginBase
    {
        public RetrieveMultiLingualValuesActionPlugin(string unsecuredParameters, string securedParameters)
            : base(unsecuredParameters, securedParameters)
        {

        }

        protected override void Execute(IExecutionContext context)
        {
            var contactService = context.IocScope.Resolve<ILanguageService>();
            contactService.RetrieveMultiLingualValues(context);
        }
    }
}
