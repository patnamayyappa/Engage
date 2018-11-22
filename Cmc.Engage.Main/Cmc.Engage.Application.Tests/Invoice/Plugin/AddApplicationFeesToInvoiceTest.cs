using Cmc.Core.Xrm.ServerExtension.Core;
using Cmc.Core.Xrm.ServerExtension.Logging;
using Cmc.Engage.Common.Plugins.Tests.Utilities;
using Cmc.Engage.Models;
using FakeXrmEasy;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Cmc.Engage.Application.Tests.Invoice.Plugin
{
    [TestClass]
    public class AddApplicationFeesToInvoiceTest : XrmUnitTestBase
    {
        [TestCategory("Plugin"), TestCategory("Positive")]
        [TestMethod]
        public void AddApplicationFeesToInvoice_CreateSingleFee()
        {
            #region Arrange
            var priceList = PreparePriceList();
            var product = PrepareProduct();
            var unitOfMeasure = PrepareUnitOfMeasure();
            var priceListItem = PrepareProductPriceListItem(product.ToEntityReference(),
                priceList.ToEntityReference(), unitOfMeasure.ToEntityReference());

            var application = PrepareApplication();

            var invoice = PrepareInvoice(application.ToEntityReference(), 
                priceList.ToEntityReference());

            var xrmFakedContext = new XrmFakedContext();
            var initializeRecords = new List<Entity>()
            {
                priceList,
                product,
                unitOfMeasure,
                priceListItem,
                application
            };
            xrmFakedContext.Initialize(initializeRecords);
            #endregion

            #region ACT
            var mockLogger = new Mock<ILogger>();
            var mockServiceProvider = InitializeMockService(xrmFakedContext, invoice, Operation.Create);
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);
            var mockOrgService = xrmFakedContext.GetFakedOrganizationService();
            var invoiceService = new InvoiceService(mockOrgService, mockLogger.Object);

            invoiceService.AddApplicationFeesToInvoice(mockExecutionContext.Object);
            #endregion

            #region Assert
            var invoiceProducts = RetrieveInvoiceProducts(mockOrgService, invoice.ToEntityReference());

            Assert.AreEqual(1, invoiceProducts.Entities.Count);

            AssertInvoiceProduct(invoiceProducts.Entities.First().ToEntity<InvoiceDetail>(),
                priceListItem);
            #endregion
        }

        [TestCategory("Plugin"), TestCategory("Negative")]
        [TestMethod]
        public void AddApplicationFeesToInvoice_CreateNoFee_EmptyPriceList()
        {
            #region Arrange
            var priceList = PreparePriceList();
            var application = PrepareApplication();

            var invoice = PrepareInvoice(application.ToEntityReference(),
                priceList.ToEntityReference());

            var xrmFakedContext = new XrmFakedContext();
            var initializeRecords = new List<Entity>()
            {
                priceList,
                application
            };
            xrmFakedContext.Initialize(initializeRecords);
            #endregion

            #region ACT
            var mockLogger = new Mock<ILogger>();
            var mockServiceProvider = InitializeMockService(xrmFakedContext, invoice, Operation.Create);
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);
            var mockOrgService = xrmFakedContext.GetFakedOrganizationService();
            var invoiceService = new InvoiceService(mockOrgService, mockLogger.Object);

            invoiceService.AddApplicationFeesToInvoice(mockExecutionContext.Object);
            #endregion

            #region Assert
            AssertNoProducts(mockOrgService, invoice);
            #endregion
        }

        [TestCategory("Plugin"), TestCategory("Negative")]
        [TestMethod]
        public void AddApplicationFeesToInvoice_CreateNoFee_NonActiveProduct()
        {
            #region Arrange
            var priceList = PreparePriceList();
            var product = PrepareProduct(stateCode: ProductState.Draft);
            var unitOfMeasure = PrepareUnitOfMeasure();
            var priceListItem = PrepareProductPriceListItem(product.ToEntityReference(),
                priceList.ToEntityReference(), unitOfMeasure.ToEntityReference());

            var application = PrepareApplication();

            var invoice = PrepareInvoice(application.ToEntityReference(),
                priceList.ToEntityReference());

            var xrmFakedContext = new XrmFakedContext();
            var initializeRecords = new List<Entity>()
            {
                priceList,
                product,
                unitOfMeasure,
                priceListItem,
                application
            };
            xrmFakedContext.Initialize(initializeRecords);
            #endregion

            #region ACT
            var mockLogger = new Mock<ILogger>();
            var mockServiceProvider = InitializeMockService(xrmFakedContext, invoice, Operation.Create);
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);
            var mockOrgService = xrmFakedContext.GetFakedOrganizationService();
            var invoiceService = new InvoiceService(mockOrgService, mockLogger.Object);

            invoiceService.AddApplicationFeesToInvoice(mockExecutionContext.Object);
            #endregion

            #region Assert
            AssertNoProducts(mockOrgService, invoice);
            #endregion
        }

        [TestCategory("Plugin"), TestCategory("Negative")]
        [TestMethod]
        public void AddApplicationFeesToInvoice_CreateNoFee_NonFee()
        {
            #region Arrange
            var priceList = PreparePriceList();
            var product = PrepareProduct(isFee: false);
            var unitOfMeasure = PrepareUnitOfMeasure();
            var priceListItem = PrepareProductPriceListItem(product.ToEntityReference(),
                priceList.ToEntityReference(), unitOfMeasure.ToEntityReference());

            var application = PrepareApplication();

            var invoice = PrepareInvoice(application.ToEntityReference(),
                priceList.ToEntityReference());

            var xrmFakedContext = new XrmFakedContext();
            var initializeRecords = new List<Entity>()
            {
                priceList,
                product,
                unitOfMeasure,
                priceListItem,
                application
            };
            xrmFakedContext.Initialize(initializeRecords);
            #endregion

            #region ACT
            var mockLogger = new Mock<ILogger>();
            var mockServiceProvider = InitializeMockService(xrmFakedContext, invoice, Operation.Create);
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);
            var mockOrgService = xrmFakedContext.GetFakedOrganizationService();
            var invoiceService = new InvoiceService(mockOrgService, mockLogger.Object);

            invoiceService.AddApplicationFeesToInvoice(mockExecutionContext.Object);
            #endregion

            #region Assert
            AssertNoProducts(mockOrgService, invoice);
            #endregion
        }

        [TestCategory("Plugin"), TestCategory("Negative")]
        [TestMethod]
        public void AddApplicationFeesToInvoice_CreateNoFee_NoApplication()
        {
            #region Arrange
            var priceList = PreparePriceList();
            var product = PrepareProduct();
            var unitOfMeasure = PrepareUnitOfMeasure();
            var priceListItem = PrepareProductPriceListItem(product.ToEntityReference(),
                priceList.ToEntityReference(), unitOfMeasure.ToEntityReference());

            var invoice = PrepareInvoice(null, priceList.ToEntityReference());

            var xrmFakedContext = new XrmFakedContext();
            var initializeRecords = new List<Entity>()
            {
                priceList,
                product,
                unitOfMeasure,
                priceListItem,
            };
            xrmFakedContext.Initialize(initializeRecords);
            #endregion

            #region ACT
            var mockLogger = new Mock<ILogger>();
            var mockServiceProvider = InitializeMockService(xrmFakedContext, invoice, Operation.Create);
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);
            var mockOrgService = xrmFakedContext.GetFakedOrganizationService();
            var invoiceService = new InvoiceService(mockOrgService, mockLogger.Object);

            invoiceService.AddApplicationFeesToInvoice(mockExecutionContext.Object);
            #endregion

            #region Assert
            AssertNoProducts(mockOrgService, invoice);
            #endregion
        }

        [TestCategory("Plugin"), TestCategory("Negative")]
        [TestMethod]
        public void AddApplicationFeesToInvoice_CreateNoFee_NoPriceList()
        {
            #region Arrange
            var application = PrepareApplication();
            var invoice = PrepareInvoice(application.ToEntityReference(),
                null);

            var xrmFakedContext = new XrmFakedContext();
            var initializeRecords = new List<Entity>()
            {
                application
            };
            xrmFakedContext.Initialize(initializeRecords);
            #endregion

            #region ACT
            var mockLogger = new Mock<ILogger>();
            var mockServiceProvider = InitializeMockService(xrmFakedContext, invoice, Operation.Create);
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);
            var mockOrgService = xrmFakedContext.GetFakedOrganizationService();
            var invoiceService = new InvoiceService(mockOrgService, mockLogger.Object);

            invoiceService.AddApplicationFeesToInvoice(mockExecutionContext.Object);
            #endregion

            #region Assert
            AssertNoProducts(mockOrgService, invoice);
            #endregion
        }

        [TestCategory("Plugin"), TestCategory("Positive")]
        [TestMethod]
        public void AddApplicationFeesToInvoice_CreateMultipleFee()
        {
            #region Arrange
            var priceList = PreparePriceList();
            var unitOfMeasure = PrepareUnitOfMeasure();

            var product1 = PrepareProduct();
            var priceListItem1 = PrepareProductPriceListItem(product1.ToEntityReference(),
                priceList.ToEntityReference(), unitOfMeasure.ToEntityReference());

            var product2 = PrepareProduct();
            var priceListItem2 = PrepareProductPriceListItem(product2.ToEntityReference(),
                priceList.ToEntityReference(), unitOfMeasure.ToEntityReference());

            var product3 = PrepareProduct(false);
            var priceListItem3 = PrepareProductPriceListItem(product3.ToEntityReference(),
                priceList.ToEntityReference(), unitOfMeasure.ToEntityReference());

            var product4 = PrepareProduct(stateCode: ProductState.Inactive);
            var priceListItem4 = PrepareProductPriceListItem(product4.ToEntityReference(),
                priceList.ToEntityReference(), unitOfMeasure.ToEntityReference());

            var product5 = PrepareProduct(false, ProductState.UnderRevision);
            var priceListItem5 = PrepareProductPriceListItem(product5.ToEntityReference(),
                priceList.ToEntityReference(), unitOfMeasure.ToEntityReference());

            var application = PrepareApplication();

            var invoice = PrepareInvoice(application.ToEntityReference(),
                priceList.ToEntityReference());

            var xrmFakedContext = new XrmFakedContext();
            var initializeRecords = new List<Entity>()
            {
                priceList,
                product1,
                product2,
                product3,
                product4,
                product5,
                unitOfMeasure,
                priceListItem1,
                priceListItem2,
                priceListItem3,
                priceListItem4,
                priceListItem5,
                application
            };
            xrmFakedContext.Initialize(initializeRecords);
            #endregion

            #region ACT
            var mockLogger = new Mock<ILogger>();
            var mockServiceProvider = InitializeMockService(xrmFakedContext, invoice, Operation.Create);
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);
            var mockOrgService = xrmFakedContext.GetFakedOrganizationService();
            var invoiceService = new InvoiceService(mockOrgService, mockLogger.Object);

            invoiceService.AddApplicationFeesToInvoice(mockExecutionContext.Object);
            #endregion

            #region Assert
            var invoiceProducts = RetrieveInvoiceProducts(mockOrgService, invoice.ToEntityReference());

            Assert.AreEqual(2, invoiceProducts.Entities.Count);

            foreach(var record in invoiceProducts.Entities)
            {
                var invoiceProduct = record.ToEntity<InvoiceDetail>();
                AssertInvoiceProduct(invoiceProduct, 
                    invoiceProduct.ProductId?.Id == product1.Id ? priceListItem1 : priceListItem2);
            }
            #endregion
        }

        public Entity PreparePriceList()
        {
            return new Entity("pricelevel")
            {
                Id = Guid.NewGuid()
            };
        }

        public Product PrepareProduct(bool isFee = true,
            ProductState stateCode = ProductState.Active)
        {
            return new Product()
            {
                ProductId = Guid.NewGuid(),
                cmc_isfeeitem = isFee,
                StateCode = stateCode
            };
        }

        public UoM PrepareUnitOfMeasure()
        {
            return new UoM
            {
                UoMId = Guid.NewGuid()
            };
        }

        public ProductPriceLevel PrepareProductPriceListItem(EntityReference productId, EntityReference priceLevelId, EntityReference uomId)
        {
            return new ProductPriceLevel()
            {
                // Use a random value for Money just for extra validation that 
                // Amount is being copied
                Amount = new Money(new Random().Next()),
                PriceLevelId = priceLevelId,
                ProductPriceLevelId = Guid.NewGuid(),
                ProductId = productId,
                UoMId = uomId
            };
        }

        public cmc_application PrepareApplication()
        {
            return new cmc_application()
            {
                cmc_applicationId = Guid.NewGuid()
            };
        }

        public Models.Invoice PrepareInvoice(EntityReference applicationId, EntityReference priceLevelId)
        {
            return new Models.Invoice()
            {
                cmc_applicationid = applicationId,
                PriceLevelId = priceLevelId
            };
        }

        private EntityCollection RetrieveInvoiceProducts(IOrganizationService orgService, EntityReference invoiceId)
        {
            return orgService.RetrieveMultiple(new FetchExpression(
                $@"<fetch>
                     <entity name='invoicedetail'>
                       <attribute name='ispriceoverridden' />
                       <attribute name='priceperunit' />
                       <attribute name='productid' />
                       <attribute name='quantity' />
                       <attribute name='uomid' />
                       <filter>
                         <condition attribute='invoiceid' operator='eq' value='{invoiceId.Id}' />
                       </filter>
                     </entity>
                   </fetch>"));
        }

        private void AssertInvoiceProduct(InvoiceDetail invoiceDetail, ProductPriceLevel product)
        {
            Assert.AreEqual(true, invoiceDetail.IsPriceOverridden);
            Assert.AreEqual(product.ProductId?.Id, invoiceDetail.ProductId?.Id);
            Assert.AreEqual(product.Amount?.Value, invoiceDetail.PricePerUnit?.Value);
            Assert.AreEqual(1, invoiceDetail.Quantity);
            Assert.AreEqual(product.UoMId?.Id, invoiceDetail.UoMId?.Id);
        }

        private void AssertNoProducts(IOrganizationService orgService, Models.Invoice invoice)
        {
            var invoiceProducts = RetrieveInvoiceProducts(orgService, invoice.ToEntityReference());
            Assert.AreEqual(0, invoiceProducts.Entities.Count);
        }
    }
}
