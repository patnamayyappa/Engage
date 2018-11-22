using FakeXrmEasy;
using Microsoft.Xrm.Sdk;
using Moq;
using System;
using System.Activities;
using System.Collections.Generic;
using Cmc.Core.Xrm.ServerExtension.Core;
using Microsoft.Xrm.Sdk.Workflow;
using IExecutionContext = Cmc.Core.Xrm.ServerExtension.Core.IExecutionContext;
using Autofac;
using Cmc.Core.Xrm.ServerExtension.Logging;
using Microsoft.Xrm.Sdk.Metadata;
using Cmc.Engage.Models;

namespace Cmc.Engage.Common.Plugins.Tests.Utilities
{
    /// <summary>
    /// Test Base is to provide some common mocking using for all Unit test cases.
    /// </summary>
    public class XrmUnitTestBase
    {
       
        /// <summary>
        /// Plugin Context Operation 
        /// </summary>
        public enum Operation
        {
            Create,
            Update,
            Delete,
            cmc_activatestudentgroups,
            RetrieveMultiple,
            cmc_createstudentsuccessplansfromtemplate,
            cmc_DeleteStaffAppointment,
            cmc_RetrieveStaffAvailabilityFromOfficeHours,
            cmc_CreateStaffAppointment,
            cmc_RetrieveStaffAppointments,
            cmc_todo,
            cmc_PredictRetentionAction,
            cmc_retrieveattributesandrelationshipsforentities,
            cmc_SurveyTemplateCreateQuestions,
            cmc_buildattributedisplay,
            cmc_CreateUpdateStaffSurveyQuestionResponses,
            cmc_CopyStaffSurveyTemplate,
            cmc_retrieveuserlookupsforentity,
            cmc_RetrieveMultiLingualValues,
            Associate,
            Disassociate
        }

        public Mock<IServiceProvider> InitializeMockService(XrmFakedContext xrmFakedContext, Entity target, Operation pluginOperation,string primaryEntityName=null)
        {
            var mockServiceProvicder = new Mock<IServiceProvider>();          
            ///Mock the Plugin Execution Context
            var mockPluginExecutionContext = xrmFakedContext.GetDefaultPluginContext();
            mockPluginExecutionContext.MessageName = pluginOperation.ToString();
            if (primaryEntityName != null)
            {
                mockPluginExecutionContext.PrimaryEntityName = primaryEntityName;
            }

            if(target != null)
            {
                ParameterCollection inputParameters = new ParameterCollection();
                inputParameters.Add(new KeyValuePair<string, object>("Target", target));
                mockPluginExecutionContext.InputParameters = inputParameters;
            }

            var mockOrganizationFactory = new Mock<IOrganizationServiceFactory>();
            mockOrganizationFactory.Setup(oSF => oSF.CreateOrganizationService(mockPluginExecutionContext.UserId)).Returns(xrmFakedContext.GetFakedOrganizationService());
            
            ///If get service is called with Type IOrganizationService return Mock Organization Service
            mockServiceProvicder.Setup(sp => sp.GetService(typeof(IOrganizationService))).Returns(xrmFakedContext.GetFakedOrganizationService());
            ///If get service is called with Type IOrganizationServiceFactory return Mock Organization Service Factory
            mockServiceProvicder.Setup(sp => sp.GetService(typeof(IOrganizationServiceFactory))).Returns(mockOrganizationFactory.Object);
            ///If get service is called with Type IPluginExecutionContext return Mock PluginExecutionContext setup earlier
            mockServiceProvicder.Setup(sp => sp.GetService(typeof(IPluginExecutionContext))).Returns(mockPluginExecutionContext);

            return mockServiceProvicder;
        }

        public Mock<IBingMapService> InitializeBingMapMockService()
        {
            var mockBingMapService = new Mock<IBingMapService>();
            var mockLatLog = new LatitudeLongitude();
            var lat = mockLatLog.GetType().GetProperty("Latitude"); 
            var log = mockLatLog.GetType().GetProperty("Longitude");
            lat.SetValue(mockLatLog,22.2);
            log.SetValue(mockLatLog, 44.2);
            mockBingMapService.Setup(r => r.GetCoordinatesFromAddress(It.IsAny<string>())).Returns(mockLatLog);
            return mockBingMapService;
        }


        public static cmc_configuration GetActiveConfiguration()
        {
            cmc_configuration config = new cmc_configuration() {
                Id = Guid.NewGuid(),
                cmc_bingmapapikey= "ApHcQX8UM8ulfNCQgjrGRFu5-He1C0BC2cFh9VtoJbKQG7FNzOT-0t_zau-a_LBh",
                cmc_batchgeocodesize=10,
                cmc_stoplegacycheck=false,
                cmc_retentionpredictionapikey = "bstXj9QbPUeX4bw0Pwyozo10R0j+OGEQr1zAJCWHSsTmUf5S7SgfxIQmrqHH1Lze9M4UKLkSkAsCB5ykiztAkQ==",
                cmc_retentionpredictionapiurl = "https://ussouthcentral.services.azureml.net/workspaces/15cc478a2f4344fca0eb3b2a534f5de2/services/c54a84046bd048c4a08fa9495b9cdea1/execute?api-version=2.0&details=true",
                cmc_tripapprovalassignworkflow = new Workflow() { Id = Guid.NewGuid() }.ToEntityReference()
            };           
            return config;
        } 

        public void AddPreEntityImage(Mock<IServiceProvider> mockServiceProvider, string imageName, Entity imageEntity)
        {
            var mockPluginExecutionContext = (IPluginExecutionContext)mockServiceProvider.Object.GetService(typeof(IPluginExecutionContext));
            mockPluginExecutionContext.PreEntityImages.Add(new KeyValuePair<string, Entity>(imageName, imageEntity));
        }

        public void AddPostEntityImage(Mock<IServiceProvider> mockServiceProvider, string imageName, Entity imageEntity)
        {
            var mockPluginExecutionContext = (IPluginExecutionContext)mockServiceProvider.Object.GetService(typeof(IPluginExecutionContext));
            mockPluginExecutionContext.PostEntityImages.Add(new KeyValuePair<string, Entity>(imageName, imageEntity));
        }

        public void AddInputParameters(Mock<IServiceProvider> mockServiceProvider, string inputName, object input)
        {
            var mockPluginExecutionContext = (IPluginExecutionContext)mockServiceProvider.Object.GetService(typeof(IPluginExecutionContext));
            mockPluginExecutionContext.InputParameters.Add(new KeyValuePair<string, object>(inputName, input));
        }

        public void AddOutputParameters(Mock<IServiceProvider> mockServiceProvider, string outputName, object output)
        {
            var mockPluginExecutionContext = (IPluginExecutionContext)mockServiceProvider.Object.GetService(typeof(IPluginExecutionContext));
            mockPluginExecutionContext.OutputParameters.Add(new KeyValuePair<string, object>(outputName, output));
        }
        public Mock<IExecutionContext> GetMockExecutionContext(Mock<IServiceProvider> mockServiceProvider)
        {
            //Mock the Cmc.Core.Xrm.ServerExtension.IExecutionContext Instance 
            var mockExecutionContext = new Mock<IExecutionContext>();
            //Set up the IExecutionContext with the fakeIServiceProvider instance.
            mockExecutionContext.SetupGet(exePreContext => exePreContext.XrmServiceProvider)
                .Returns(mockServiceProvider.Object);
            return mockExecutionContext;
        }

        public ContainerBuilder MockIocScope(ILogger mockLogger,XrmFakedContext xrmFakedContext)
        {
            var containerBuilder = new ContainerBuilder();
            containerBuilder.RegisterInstance(mockLogger);
            containerBuilder.RegisterInstance(xrmFakedContext.GetFakedOrganizationService());
            return containerBuilder;
        }

        #region TestBaseOld
        /// <summary>
        /// This method will provide a mock IServiceProvider which containts
        ///     1. Mock Organization Service.
        ///     2. Mock Plugin Execution Context.
        ///     3. Mock Organization Service Factory.
        /// It will setup the Plugin Execution context to have the entites setup 
        /// for the context as a Post Operation image.
        /// </summary>
        /// <param name="mockPluginContext"></param>
        /// <param name="target"></param>
        /// <param name="pluginOperation"></param>
        /// <returns></returns>
        public Mock<IServiceProvider> GetMockedIServiceProviderWithPostEntityImage(XrmFakedContext mockPluginContext, Entity target, Operation pluginOperation, string tragetName = "Target")
        {

            var mockServiceProvicder = new Mock<IServiceProvider>();

            ///Mock the Plugin Execution Context
            var mockPluginExecutionContext = mockPluginContext.GetDefaultPluginContext();
            mockPluginExecutionContext.MessageName = pluginOperation.ToString();

            mockPluginExecutionContext.PostEntityImages.Add(new KeyValuePair<string, Entity>(tragetName, target));

            ParameterCollection inputParameters = new ParameterCollection();
            inputParameters.Add(new KeyValuePair<string, object>("Target", target));
            mockPluginExecutionContext.InputParameters = inputParameters;

            ///Setup the Create Organization Factory CreateOrganizationService() with Current User to returnMock OrganizationService.
            var mockOrganizationFactory = new Mock<IOrganizationServiceFactory>();
            mockOrganizationFactory.Setup(oSF => oSF.CreateOrganizationService(mockPluginExecutionContext.UserId)).Returns(mockPluginContext.GetFakedOrganizationService());

            ///If get service is called with Type IOrganizationService return Mock Organization Service
            mockServiceProvicder.Setup(sp => sp.GetService(typeof(IOrganizationService))).Returns(mockPluginContext.GetFakedOrganizationService());
            ///If get service is called with Type IOrganizationServiceFactory return Mock Organization Service Factory
            mockServiceProvicder.Setup(sp => sp.GetService(typeof(IOrganizationServiceFactory))).Returns(mockOrganizationFactory.Object);
            ///If get service is called with Type IPluginExecutionContext return Mock PluginExecutionContext setup earlier
            mockServiceProvicder.Setup(sp => sp.GetService(typeof(IPluginExecutionContext))).Returns(mockPluginExecutionContext);

            return mockServiceProvicder;
        }

        /// <summary>
        /// /// This method will provide a mock IServiceProvider which containts
        ///     1. Mock Organization Service.
        ///     2. Mock Plugin Execution Context.
        ///     3. Mock Organization Service Factory.
        /// It will setup the Plugin Execution context to have the entites setup 
        /// for the context as a Pre Operation image.
        /// </summary>
        /// <param name="mockPluginContext"></param>
        /// <param name="target"></param>
        /// <param name="pluginOperation"></param>
        /// <returns></returns>
        public Mock<IServiceProvider> GetMockedIServiceProviderWithPreEntityImage(XrmFakedContext mockPluginContext, Entity target, Operation pluginOperation, Entity targetPostImage = null)
        {

            var mockServiceProvicder = new Mock<IServiceProvider>();

            ///Mock the Plugin Execution Context
            var mockPluginExecutionContext = mockPluginContext.GetDefaultPluginContext();
            mockPluginExecutionContext.MessageName = pluginOperation.ToString();
            mockPluginExecutionContext.PreEntityImages.Add(new KeyValuePair<string, Entity>("PreImage", target));
            if (targetPostImage != null)
            {
                mockPluginExecutionContext.PostEntityImages.Add(new KeyValuePair<string, Entity>("PostImage", targetPostImage));
            }
            ParameterCollection inputParameters = new ParameterCollection();
            inputParameters.Add(new KeyValuePair<string, object>("Target", target));
            mockPluginExecutionContext.InputParameters = inputParameters;

            ///Setup the Create Organization Factory CreateOrganizationService() with Current User to returnMock OrganizationService.
            var mockOrganizationFactory = new Mock<IOrganizationServiceFactory>();
            mockOrganizationFactory.Setup(oSF => oSF.CreateOrganizationService(mockPluginExecutionContext.UserId)).Returns(mockPluginContext.GetFakedOrganizationService());

            ///If get service is called with Type IOrganizationService return Mock Organization Service
            mockServiceProvicder.Setup(sp => sp.GetService(typeof(IOrganizationService))).Returns(mockPluginContext.GetFakedOrganizationService());
            ///If get service is called with Type IOrganizationServiceFactory return Mock Organization Service Factory
            mockServiceProvicder.Setup(sp => sp.GetService(typeof(IOrganizationServiceFactory))).Returns(mockOrganizationFactory.Object);
            ///If get service is called with Type IPluginExecutionContext return Mock PluginExecutionContext setup earlier
            mockServiceProvicder.Setup(sp => sp.GetService(typeof(IPluginExecutionContext))).Returns(mockPluginExecutionContext);

            return mockServiceProvicder;
        }


        /// <summary>
        /// This method will provide a mock IServiceProvider which containts
        ///     1. Mock Organization Service.
        ///     2. Mock Plugin Execution Context.
        ///     3. Mock Organization Service Factory.
        /// It will setup the Plugin Execution context to have the entites setup 
        /// for the context as a Post Operation image.
        /// </summary>
        /// <param name="mockPluginContext"></param>
        /// <param name="target"></param>
        /// <param name="pluginOperation"></param>
        /// <returns></returns>
        public Mock<IServiceProvider> GetMockedIServiceProviderWithPostEntityCollectionImage(XrmFakedContext mockPluginContext, Entity target, Operation pluginOperation, string tragetName = "Target")
        {

            var mockServiceProvicder = new Mock<IServiceProvider>();

            ///Mock the Plugin Execution Context
            var mockPluginExecutionContext = mockPluginContext.GetDefaultPluginContext();
            mockPluginExecutionContext.MessageName = pluginOperation.ToString();

            ///Setup the Create Organization Factory CreateOrganizationService() with Current User to returnMock OrganizationService.

            //mockPluginExecutionContext.PostEntityImages.Add(new KeyValuePair<string, Entity>(tragetName, target));

            EntityCollection baseEntityCollection = new EntityCollection();
            baseEntityCollection.Entities.Add(target);

            ParameterCollection inputParameters = new ParameterCollection();
            inputParameters.Add(new KeyValuePair<string, object>(tragetName, baseEntityCollection));
            mockPluginExecutionContext.InputParameters = inputParameters;

            var mockOrganizationFactory = new Mock<IOrganizationServiceFactory>();
            mockOrganizationFactory.Setup(oSF => oSF.CreateOrganizationService(mockPluginExecutionContext.UserId)).Returns(mockPluginContext.GetFakedOrganizationService());

            ///If get service is called with Type IOrganizationService return Mock Organization Service
            mockServiceProvicder.Setup(sp => sp.GetService(typeof(IOrganizationService))).Returns(mockPluginContext.GetFakedOrganizationService());
            ///If get service is called with Type IOrganizationServiceFactory return Mock Organization Service Factory
            mockServiceProvicder.Setup(sp => sp.GetService(typeof(IOrganizationServiceFactory))).Returns(mockOrganizationFactory.Object);
            ///If get service is called with Type IPluginExecutionContext return Mock PluginExecutionContext setup earlier
            mockServiceProvicder.Setup(sp => sp.GetService(typeof(IPluginExecutionContext))).Returns(mockPluginExecutionContext);

            return mockServiceProvicder;
        }

        public Mock<IActivityExecutionContext> InitializeActivityMockService(XrmFakedContext xrmFakedContext)
        {
            var mockActivityExecutionContext = new Mock<IActivityExecutionContext>();
            var mockCodeActivityContext = new Mock<CodeActivityContext>();

            ///Mock the Plugin Execution Context
            var mockWorkflowContext = xrmFakedContext.GetDefaultWorkflowContext();

            var mockOrganizationService = xrmFakedContext.GetFakedOrganizationService();

            var mockOrganizationFactory = new Mock<IOrganizationServiceFactory>();

            mockOrganizationFactory.Setup(oSf => oSf.CreateOrganizationService(mockWorkflowContext.UserId)).Returns(mockOrganizationService);

            mockCodeActivityContext.Setup(x => x.GetExtension<IWorkflowContext>()).Returns(mockWorkflowContext);

            mockCodeActivityContext.Setup(x => x.GetExtension<IOrganizationServiceFactory>())
                .Returns(mockOrganizationFactory.Object);

            mockCodeActivityContext.Setup(x => x.GetExtension<IOrganizationService>())
                .Returns(mockOrganizationService);

            mockActivityExecutionContext.Setup(x => x.ActivityContext).Returns(mockCodeActivityContext.Object);

            ///If get service is called with Type IOrganizationService return Mock Organization Service
            //mockServiceProvicder.Setup(sp => sp.GetService(typeof(IOrganizationService))).Returns(xrmFakedContext.GetFakedOrganizationService());
            /////If get service is called with Type IOrganizationServiceFactory return Mock Organization Service Factory
            //mockServiceProvicder.Setup(sp => sp.GetService(typeof(IOrganizationServiceFactory))).Returns(mockOrganizationFactory.Object);
            /////If get service is called with Type IPluginExecutionContext return Mock PluginExecutionContext setup earlier
            //mockServiceProvicder.Setup(sp => sp.GetService(typeof(IWorkflowContext))).Returns(mockWorkflowContext);

            return mockActivityExecutionContext;
        }

        #endregion
    }

    //Get or Set the AttributeType using OwnerAttributeMetadata
    public sealed class OwnerAttributeMetadata : AttributeMetadata
    {
        public OwnerAttributeMetadata() : base(AttributeTypeCode.Owner)
        {

        }
    }
}
