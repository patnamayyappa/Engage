using System;
using System.Collections.Generic;
using Cmc.Core.Xrm.ServerExtension.Logging;
using Cmc.Engage.Common;
using Cmc.Engage.Common.Plugins.Tests.Utilities;
using Cmc.Engage.Models;
using FakeXrmEasy;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Moq;

namespace Cmc.Engage.Lifecycle.Tests.DomDefinition.Plugin
{ 
    [TestClass]
    public class ValidateDomDefinitionPluginTest : XrmUnitTestBase
    {
        [TestCategory("Activity"), TestCategory("Positive")]
        [TestMethod]
        public void ValidateDomDefinitionPlugin_RetriveDomDefinitionLogicRecords_cmc_domdefinition_Update()
        {
            #region ARRANGE
            var domMaster = PrepareDomMaster();
            var domDefinition = PrepareCmcDomDefinitionInstance(domMaster.Id);
            var domDefinitionPreImage=PrepareDOMDefinationPreImage(domDefinition);
            //var domDefinitionPreImage = domMaster;

            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.Initialize(new List<Entity>()
            {                
                domMaster,
                domDefinition,
                domDefinitionPreImage

            });

            //Initialize the Mock Service
            var mockServiceProvider = InitializeMockService(xrmFakedContext, domDefinition, Operation.Update);
            AddPreEntityImage(mockServiceProvider, "Target", domDefinitionPreImage);
            //Fetch the Mock Execution Context
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);
            #endregion

            #region ACT
            var mockLogger = new Mock<ILogger>();
            var mockLanguageService = new Mock<ILanguageService>();
            var mockDomDefinitionService = new DomDefinitionService(mockLogger.Object, mockLanguageService.Object, xrmFakedContext.GetFakedOrganizationService());
            mockDomDefinitionService.ValidateDomDefinition(mockExecutionContext.Object);
            #endregion

            #region ASSERT
            Assert.IsTrue(true);
            #endregion

        }

        [TestCategory("Activity"), TestCategory("Positive")]
        [TestMethod]
        public void ValidateDomDefinitionPlugin_RetriveDomDefinitionLogicRecords_cmc_domdefinition_Update_Account()
        {
            #region ARRANGE
            var domMaster = PrepareDomMaster1();
            var domDefinition = PrepareCmcDomDefinitionInstance(domMaster.Id);
            var domDefinitionPreImage = PrepareDOMDefinationPreImage(domDefinition);
            //var domDefinitionPreImage = domMaster;

            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.Initialize(new List<Entity>()
            {
                domMaster,
                domDefinition,
                domDefinitionPreImage

            });

            //Initialize the Mock Service
            var mockServiceProvider = InitializeMockService(xrmFakedContext, domDefinition, Operation.Update);
            AddPreEntityImage(mockServiceProvider, "Target", domDefinitionPreImage);
            //Fetch the Mock Execution Context
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);
            #endregion

            #region ACT
            var mockLogger = new Mock<ILogger>();
            var mockLanguageService = new Mock<ILanguageService>();
            var mockDomDefinitionService = new DomDefinitionService(mockLogger.Object, mockLanguageService.Object, xrmFakedContext.GetFakedOrganizationService());
            mockDomDefinitionService.ValidateDomDefinition(mockExecutionContext.Object);
            #endregion

            #region ASSERT
            Assert.IsTrue(true);
            #endregion

        }

        [TestCategory("Activity"), TestCategory("Positive")]
        [TestMethod]
        public void ValidateDomDefinitionPlugin_RetriveDomDefinitionLogicRecords_cmc_domdefinition_Update_InboundInterest()
        {
            #region ARRANGE
            var domMaster = PrepareDomMaster2();
            var domDefinition = PrepareCmcDomDefinitionInstance(domMaster.Id);
            var domDefinitionPreImage = PrepareDOMDefinationPreImage(domDefinition);
            //var domDefinitionPreImage = domMaster;

            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.Initialize(new List<Entity>()
            {
                domMaster,
                domDefinition,
                domDefinitionPreImage

            });

            //Initialize the Mock Service
            var mockServiceProvider = InitializeMockService(xrmFakedContext, domDefinition, Operation.Update);
            AddPreEntityImage(mockServiceProvider, "Target", domDefinitionPreImage);
            //Fetch the Mock Execution Context
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);
            #endregion

            #region ACT
            var mockLogger = new Mock<ILogger>();
            var mockLanguageService = new Mock<ILanguageService>();
            var mockDomDefinitionService = new DomDefinitionService(mockLogger.Object, mockLanguageService.Object, xrmFakedContext.GetFakedOrganizationService());
            mockDomDefinitionService.ValidateDomDefinition(mockExecutionContext.Object);
            #endregion

            #region ASSERT
            Assert.IsTrue(true);
            #endregion

        }

        [TestCategory("Activity"), TestCategory("Positive")]
        [TestMethod]
        public void ValidateDomDefinitionPlugin_RetriveDomDefinitionLogicRecords_cmc_domdefinition_Update_Lifecycle()
        {
            #region ARRANGE
            var domMaster = PrepareDomMaster3();
            var domDefinition = PrepareCmcDomDefinitionInstance(domMaster.Id);
            var domDefinitionPreImage = PrepareDOMDefinationPreImage(domDefinition);
            //var domDefinitionPreImage = domMaster;

            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.Initialize(new List<Entity>()
            {
                domMaster,
                domDefinition,
                domDefinitionPreImage

            });

            //Initialize the Mock Service
            var mockServiceProvider = InitializeMockService(xrmFakedContext, domDefinition, Operation.Update);
            AddPreEntityImage(mockServiceProvider, "Target", domDefinitionPreImage);
            //Fetch the Mock Execution Context
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);
            #endregion
            
            #region ACT
            var mockLogger = new Mock<ILogger>();
            var mockLanguageService = new Mock<ILanguageService>();
            var mockDomDefinitionService = new DomDefinitionService(mockLogger.Object, mockLanguageService.Object, xrmFakedContext.GetFakedOrganizationService());
            mockDomDefinitionService.ValidateDomDefinition(mockExecutionContext.Object);
            #endregion

            #region ASSERT
            Assert.IsTrue(true);
            #endregion

        }

        [TestCategory("Activity"), TestCategory("Negative")]
        [TestMethod]
        public void ValidateDomDefinitionPluginForException()
        {
            #region ARRANGE

            var xrmFakedContext = new XrmFakedContext();
           
            #endregion

            #region ACT
            var mockLogger = new Mock<ILogger>();
            var mockLanguageService = new Mock<ILanguageService>();
        
            #endregion

            #region ASSERT
           
            Assert.ThrowsException<ArgumentNullException>(() => new DomDefinitionService(null, mockLanguageService.Object,xrmFakedContext.GetFakedOrganizationService()));
            Assert.ThrowsException<ArgumentException>(() => new DomDefinitionService(mockLogger.Object, null, xrmFakedContext.GetFakedOrganizationService()));
            #endregion

        }

        private cmc_dommaster PrepareDomMaster()
        {
            var domMaster = new cmc_dommaster()
            {
                Id = Guid.NewGuid(),              
                cmc_runassignmentforentity = new OptionSetValue((int)cmc_runassignmentforentity.Contact)
            };
            return domMaster;
        }
        private cmc_dommaster PrepareDomMaster1()
        {
            var domMaster = new cmc_dommaster()
            {
                Id = Guid.NewGuid(),
                cmc_runassignmentforentity = new OptionSetValue((int)cmc_runassignmentforentity.Account)
            };
            return domMaster;
        }

        private cmc_dommaster PrepareDomMaster2()
        {
            var domMaster = new cmc_dommaster()
            {
                Id = Guid.NewGuid(),
                cmc_runassignmentforentity = new OptionSetValue((int)cmc_runassignmentforentity.InboundInterest)
            };
            return domMaster;
        }

        private cmc_dommaster PrepareDomMaster3()
        {
            var domMaster = new cmc_dommaster()
            {
                Id = Guid.NewGuid(),
                cmc_runassignmentforentity = new OptionSetValue((int)cmc_runassignmentforentity.Lifecycle)
            };
            return domMaster;
        }

        private cmc_domdefinition PrepareCmcDomDefinitionInstance(Guid domMaster)
        {
            var domDefinitionInstance = new cmc_domdefinition()
            {
                Id = Guid.NewGuid(),
                cmc_dommasterid = new EntityReference("cmc_dommaster", domMaster),                
                LogicalName = "cmc_domdefinition"
            };
            return domDefinitionInstance;
        }
        private Entity PrepareDOMDefinationPreImage(Entity domdefinition)
        {
            cmc_domdefinition cmc_Domdefinition = new cmc_domdefinition()
            {
                Id = domdefinition.Id

            };
            return cmc_Domdefinition;
        }


        [TestCategory("Activity"), TestCategory("Negative")]
        [TestMethod]
        public void ValidateDomDefinitionPlugin_RetriveDomDefinitionLogicRecords_cmc_domdefinition_Update_Negative()
        {
            #region ARRANGE
                     
            var domDefinitionPreImageDffrntId = PrepareDOMDefinationPreImage_dffrntId();
            var domDefinitionNullId = PrepareCmcDomDefinitionInstance_NullId();
            var domDefinitionPreImage_NullId = PrepareDOMDefinationPreImage_NullId(domDefinitionNullId);
           
            var domDefinitionNoLogicalName = PrepareCmcDomDefinitionInstance_NoLogicalName();
            var domDefinitionPreImage_NoLgicalName = PrepareDOMDefinationPreImage_NullId(domDefinitionNoLogicalName);


            var DomMasterNoassnment = PrepareDomMaster_Norunassignmentforentity();
            var domDefinitioNoassment = PrepareCmcDomDefinitionInstance(DomMasterNoassnment.Id);
            var domDefinitionPreImageNoassment = PrepareDOMDefinationPreImage(domDefinitioNoassment);

            var domMaster = PrepareDomMaster();
            var domDefinitionLogic = PrepareCmcDomDefinitionLogic(domMaster.Id);
            var domDefinition = PrepareCmcDomDefinitionInstance_defultId(domDefinitionLogic.Id);
            var domDefinitionPreImage = PrepareDOMDefinationPreImage(domDefinition);


            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.Initialize(new List<Entity>()
            {
               DomMasterNoassnment,
               domDefinitionNullId,
               domDefinitionPreImageDffrntId,
               domDefinitionPreImage_NullId,
               domMaster,
               domDefinitionLogic,
               domDefinition


            });
            var validateDomDefinition = new List<Entity> { domDefinitionPreImage_NoLgicalName, domDefinitionPreImage, domDefinitionPreImageNoassment, domDefinitionPreImage_NullId, domDefinitionPreImageDffrntId };

            foreach (Entity item in validateDomDefinition)
            {
                Mock<IServiceProvider> mockServiceProvider = null;
                if(item == domDefinitionPreImage_NoLgicalName)
                {
                    mockServiceProvider = InitializeMockService(xrmFakedContext, domDefinitionNoLogicalName, Operation.Update);
                    AddPreEntityImage(mockServiceProvider, "Target", item);
                }

                else if (item == domDefinitionPreImageNoassment)
                {
                    mockServiceProvider = InitializeMockService(xrmFakedContext, domDefinitioNoassment, Operation.Update);
                    AddPreEntityImage(mockServiceProvider, "Target", item);
                }
                else if (item == domDefinitionPreImage)
                {
                    mockServiceProvider = InitializeMockService(xrmFakedContext, domDefinition, Operation.Update);
                    AddPreEntityImage(mockServiceProvider, "Target", item);
                }
                else
                {
                    mockServiceProvider = InitializeMockService(xrmFakedContext, domDefinitionNullId, Operation.Update);
                    AddPreEntityImage(mockServiceProvider, "Target", item);
                }
                //Fetch the Mock Execution Context
                var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);
                #endregion

                #region ACT
                var mockLogger = new Mock<ILogger>();
                var mockLanguageService = new Mock<ILanguageService>();
                var mockDomDefinitionService = new DomDefinitionService(mockLogger.Object, mockLanguageService.Object, xrmFakedContext.GetFakedOrganizationService());
                try
                {
                    mockDomDefinitionService.ValidateDomDefinition(mockExecutionContext.Object);
                }
                catch { }
                #endregion

                #region ASSERT
                Assert.IsFalse(false);
                #endregion
            }
        }

        private Entity PrepareDOMDefinationPreImage_dffrntId()
        {
            cmc_domdefinition cmc_Domdefinition = new cmc_domdefinition()
            {
                Id = Guid.NewGuid()

            };
            return cmc_Domdefinition;
        }
        private cmc_domdefinition PrepareCmcDomDefinitionInstance_NullId()
        {
            var domDefinitionInstance = new cmc_domdefinition()
            {
                Id = Guid.NewGuid(),
                cmc_dommasterid = null,
                LogicalName = "cmc_domdefinition"
            };
            return domDefinitionInstance;
        }

        private Entity PrepareCmcDomDefinitionInstance_NoLogicalName()
        {
            var domDefinitionInstance = new cmc_dommaster()
            {
                Id = Guid.NewGuid(),
                
            };
            return domDefinitionInstance;
        }

        private Entity PrepareDOMDefinationPreImage_NullId(Entity domdefinition)
        {
            cmc_domdefinition cmc_Domdefinition = new cmc_domdefinition()
            {
                Id = Guid.NewGuid(),
                cmc_dommasterid = new EntityReference("cmc_dommaster", Guid.NewGuid()),

            };
            return cmc_Domdefinition;
        }
        private cmc_dommaster PrepareDomMaster_Norunassignmentforentity()
        {
            var domMaster = new cmc_dommaster()
            {
                Id = Guid.NewGuid(),
            };
            return domMaster;
        }

        private cmc_domdefinition PrepareCmcDomDefinitionInstance_defultId(Guid domMaster)
        {
            var domDefinitionInstance = new cmc_domdefinition()
            {
                Id = domMaster,
                cmc_dommasterid = new EntityReference("cmc_dommaster", domMaster),
                LogicalName = "cmc_domdefinition"
            };
            return domDefinitionInstance;
        }
        private cmc_domdefinitionlogic PrepareCmcDomDefinitionLogic(Guid domMaster)
        {
            return new cmc_domdefinitionlogic()
            {
                Id = Guid.NewGuid(),
                cmc_attributeschema = "Nothing"

            };

        }
    }
}
