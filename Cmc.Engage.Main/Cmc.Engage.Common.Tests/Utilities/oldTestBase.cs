using FakeXrmEasy;
using Microsoft.Xrm.Sdk;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cmc.Engage.Common.Plugins.Tests.Utilities
{
    /// <summary>
    /// Test Base is to provide some common mocking using for all Unit test cases.
    /// </summary>
    public class TestBase
    {
        /// <summary>
        /// Plugin Context Operation 
        /// </summary>
        public enum Operation
        {
            Create,
            Update,
            Delete,
            cmc_activatestudentgroups
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
        public Mock<IServiceProvider> InitializeMockService(XrmFakedContext mockPluginContext, Entity target, Operation pluginOperation, Entity targetPostImage = null)
        {
            var mockServiceProvicder = new Mock<IServiceProvider>();

            ///Mock the Plugin Execution Context
            var mockPluginExecutionContext = mockPluginContext.GetDefaultPluginContext();
            mockPluginExecutionContext.MessageName = pluginOperation.ToString();

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

        public void AddPreEntityImage(XrmFakedContext mockPluginContext, string imageName, Entity imageEntity)
        {
            var mockPluginExecutionContext = mockPluginContext.GetDefaultPluginContext();
            mockPluginExecutionContext.PreEntityImages.Add(new KeyValuePair<string, Entity>(imageName, imageEntity));
        }

        public void AddPostEntityImage(XrmFakedContext mockPluginContext, string imageName, Entity imageEntity)
        {
            var mockPluginExecutionContext = mockPluginContext.GetDefaultPluginContext();
            mockPluginExecutionContext.PostEntityImages.Add(new KeyValuePair<string, Entity>(imageName, imageEntity));
        }

        public void AddInputParameters(XrmFakedContext mockPluginContext, string inputName, object input)
        {
            var mockPluginExecutionContext = mockPluginContext.GetDefaultPluginContext();
            mockPluginExecutionContext.InputParameters.Add(new KeyValuePair<string, object>(inputName, input));
        }
    }
}
