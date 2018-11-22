using IExecutionContext = Cmc.Core.Xrm.ServerExtension.Core.IExecutionContext;
using Cmc.Core.Xrm.ServerExtension.Logging;
using Microsoft.Xrm.Sdk;
using Cmc.Core.Xrm.ServerExtension.Core;
using Cmc.Engage.Models;

namespace Cmc.Engage.Application
{
    public class InvoiceService : IInvoiceService
    {
        private IOrganizationService _orgService;
        private readonly ILogger _logger;

        public InvoiceService(IOrganizationService orgService, ILogger logger)
        {
            _orgService = orgService;
            _logger = logger;
        }

        #region AddApplicationFeesToInvoicePlugin
        public void AddApplicationFeesToInvoice(IExecutionContext executionContext)
        {
            _logger.Trace("Starting AddApplicationFeesToInvoicePlugin");
            var serviceProvider = executionContext.XrmServiceProvider;
            var pluginContext = serviceProvider.GetPluginExecutionContext();

            _logger.Trace("Retrieving Target.");
            var target = pluginContext.GetInputParameter<Entity>("Target").ToEntity<Invoice>();
            CreateApplicationFeeProducts(target);
            _logger.Trace("Exiting AddApplicationFeesToInvoicePlugin");
        }

        private void CreateApplicationFeeProducts(Invoice invoice)
        {
            if (invoice.cmc_applicationid == null || invoice.PriceLevelId == null)
            {
                _logger.Trace("Application or Price List are not set. No Application Fees will be created.");
                return;
            }

            _logger.Trace("Retrieving Active Fees");
            // Retrieve Products for the related Price List that are both Active and Fees.
            // Only Active Products can be added to an Invoice.
            var fees = _orgService.RetrieveMultipleAll(
                $@"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                      <entity name='productpricelevel'>
                        <attribute name='productid' />
                        <attribute name='uomid' />
                        <attribute name='amount' />
                        <order attribute='productid' descending='false' />
                        <filter type='and'>
                          <condition attribute='pricelevelid' operator='eq' value='{invoice.PriceLevelId.Id}' />
                        </filter>
                        <link-entity name='product' from='productid' to='productid' link-type='inner'>
                          <filter type='and'>
                            <condition attribute='cmc_isfeeitem' operator='eq' value='1' />
                            <condition attribute='statecode' operator='eq' value='{(int)ProductState.Active}' />
                          </filter>
                        </link-entity>
                      </entity>
                    </fetch>");

            _logger.Trace("Creating Fees on the Invoice.");
            var invoiceId = invoice.ToEntityReference();
            // Microsoft uses an undocumented method to create Invoice Details for existing products,
            // so they must be manually created here from the related Price List item.
            foreach (var record in fees.Entities)
            {
                var fee = record.ToEntity<ProductPriceLevel>();
                var invoiceProduct = new InvoiceDetail()
                {
                    InvoiceId = invoiceId,
                    IsPriceOverridden = true,
                    PricePerUnit = fee.Amount,
                    ProductId = fee.ProductId,
                    Quantity = 1,
                    UoMId = fee.UoMId
                };

                _orgService.Create(invoiceProduct);
            }
        }
        #endregion
    }
}
