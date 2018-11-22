using Cmc.Core.Xrm.ServerExtension.Core;
using IExecutionContext = Cmc.Core.Xrm.ServerExtension.Core.IExecutionContext;
using Microsoft.Xrm.Sdk;
using Autofac;

namespace Cmc.Engage.Application.Plugins
{
    public class AddApplicationFeesToInvoicePlugin : PluginBase, IPlugin 
    {
        public AddApplicationFeesToInvoicePlugin(string unsecuredParameters, string securedParameters)
            : base(unsecuredParameters, securedParameters)
        {
        }

        protected override void Execute(IExecutionContext context)
        {
            var invoiceService = context.IocScope.Resolve<IInvoiceService>();
            invoiceService.AddApplicationFeesToInvoice(context);
        }
    }
}
