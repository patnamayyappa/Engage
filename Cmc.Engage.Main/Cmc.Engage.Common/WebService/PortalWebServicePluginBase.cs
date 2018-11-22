using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Xml.Linq;
using Autofac;
using Cmc.Core.Xrm.ServerExtension.Core;
using Cmc.Core.Xrm.ServerExtension.Logging;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;
using Newtonsoft.Json;
using IExecutionContext = Cmc.Core.Xrm.ServerExtension.Core.IExecutionContext;

namespace Cmc.Engage.Common
{
    public class PortalWebServicePluginBase : IPortalWebServicePluginBaseLogic
    {
        private IOrganizationService _orgService;
        private readonly ILogger _tracer;
        public PortalWebServicePluginBase(ILogger tracer, IOrganizationService orgService)
        {
            _tracer = tracer ?? throw new ArgumentNullException(nameof(tracer));
            _orgService = orgService ?? throw new ArgumentException(nameof(orgService));
        }
        //protected abstract object Process(IExcutionContext context, string logicClassName, string inputData);
        public void Run(IExecutionContext context)
        {

            var serviceProvider = context.XrmServiceProvider;
            var pluginContext = serviceProvider.GetPluginExecutionContext();
            var entityName = pluginContext.PrimaryEntityName;
            String logicClassName = null;
            String data = null;
            var query = (QueryBase)pluginContext.InputParameters["Query"];
            var retrieveMultipleOutput = (EntityCollection)pluginContext.OutputParameters["BusinessEntityCollection"];

            GetDataFromRetrieveMultiple(query, ref logicClassName, ref data);
            retrieveMultipleOutput.Entities.Clear();

            var processedResults = ProcessResults(context, logicClassName, data);

            var dataAttributeRequest = new RetrieveAttributeRequest
            {
                EntityLogicalName = entityName,
                LogicalName = "cmc_data",
                RetrieveAsIfPublished = false
            };

            var dataAttributeResponse = (RetrieveAttributeResponse)_orgService.Execute(dataAttributeRequest);
            var dataAttributeMetadata = (MemoAttributeMetadata)dataAttributeResponse.AttributeMetadata;

            List<string> dataResults = Split(processedResults, dataAttributeMetadata.MaxLength.Value);
            foreach (var dataResult in dataResults)
            {
                var returnEntity = new Entity(entityName)
                {
                    Id = Guid.NewGuid()
                };
                returnEntity[entityName + "id"] = returnEntity.Id;
                returnEntity["statecode"] = new OptionSetValue(0);
                returnEntity["cmc_data"] = dataResult;

                retrieveMultipleOutput.Entities.Add(returnEntity);
            }
        }
        private string ProcessResults(IExecutionContext context, string logicClassName, string data)
        {
            var logicObject = (PortalWebServiceLogicBase)context.IocScope.ResolveNamed<PortalWebServiceLogicBase>(Type.GetType(logicClassName)?.Name); //pute moke class in side 
            if (logicObject == null)
            {
                throw new InvalidPluginExecutionException(String.Format(
                    "Logic class does not exist: {0}", logicClassName));
            }
            var output = logicObject.DoWork(context, data);

            //var output = Process(serviceProvider, logicClassName, data);
            return JsonConvert.SerializeObject(output);
        }
        private void GetDataFromRetrieveMultiple(QueryBase query, ref String logicClassName, ref String data)
        {
            if (query.GetType() == typeof(QueryExpression))
            {
                var qe = (QueryExpression)query;
                StripValuesFromQuery(qe.Criteria, ref logicClassName, ref data);
            }
            else if (query.GetType() == typeof(FetchExpression))
            {
                var fe = (FetchExpression)query;
                StripValuesFromQuery(fe.Query, ref logicClassName, ref data);
            }
        }
        private void StripValuesFromQuery(FilterExpression filter, ref String logicClassName, ref String data)
        {
            if (filter != null && filter.Conditions != null)
            {
                foreach (var condition in filter.Conditions)
                {
                    if (condition.Values.Count == 1)
                    {
                        if (condition.AttributeName == "cmc_data")
                        {
                            data = WebUtility.HtmlDecode(Uri.UnescapeDataString(condition.Values[0] as String));
                        }
                        else if (condition.AttributeName == "cmc_logicclassname")
                        {
                            logicClassName = WebUtility.HtmlDecode(Uri.UnescapeDataString(condition.Values[0] as String));
                        }
                    }
                }
            }

            foreach (FilterExpression subFilter in filter.Filters)
            {
                StripValuesFromQuery(subFilter, ref logicClassName, ref data);
            }
        }
        private void StripValuesFromQuery(string query, ref string logicClassName, ref string data)
        {
            var queryXml = XDocument.Parse(query);
            logicClassName = WebUtility.HtmlDecode(Uri.UnescapeDataString(
                (from x in queryXml.Descendants("condition")
                 where x.Attribute("attribute") != null
                     && x.Attribute("attribute").Value == "cmc_logicclassname"
                     && x.Attribute("value") != null
                 select x.Attribute("value").Value).FirstOrDefault()));

            data = WebUtility.HtmlDecode(Uri.UnescapeDataString(
                (from x in queryXml.Descendants("condition")
                 where x.Attribute("attribute") != null
                    && x.Attribute("attribute").Value == "cmc_data"
                    && x.Attribute("value") != null
                 select x.Attribute("value").Value).FirstOrDefault()));
        }
        private List<string> Split(string str, int chunkSize)
        {
            List<string> result = new List<string>();

            int stringLength = str.Length;
            for (int i = 0; i < stringLength; i += chunkSize)
            {
                if (i + chunkSize > stringLength)
                {
                    // Take the remainder of the string
                    result.Add(str.Substring(i));
                }
                else
                {
                    // Take from current position to chunk size
                    result.Add(str.Substring(i, chunkSize));
                }
            }

            return result;
        }
    }
}
