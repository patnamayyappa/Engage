using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cmc.Core.Xrm.ServerExtension.Core;
using Cmc.Core.Xrm.ServerExtension.Logging;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;
using Newtonsoft.Json;

namespace Cmc.Engage.Common
{
    //RetrieveMultiLingualValuesService
    public class LanguageService : ILanguageService
    {
        private IOrganizationService _orgService;
        private ILogger _tracer;
        public LanguageService(ILogger tracer, IOrganizationService orgService)
        {      
            _tracer = tracer ?? throw new ArgumentException(nameof(tracer));           
            _orgService = orgService;
        }
        public string Get(string key, Guid? userId = null)
        {
            var result = Get(new[] { key });
            return result.Count > 0 && string.IsNullOrEmpty(result.First().Value) == false ? result.First().Value : key;
        }

        public Dictionary<string, string> Get(IList<string> keys, Guid? userId = null)
        {
            var languageCode = GetCurrentUserLanguageCode(userId);
            return Get(keys, languageCode);
        }
                
        public Dictionary<string, string> Get(IList<string> keys, int? languageCode)
        {

            var ret = new Dictionary<string, string>();

            if (!languageCode.HasValue)
            {

                _tracer.Trace("Language code could not be determined.");
                foreach (var key in keys)
                {
                    ret.Add(key, "");
                }
                return ret;
            }

            var inKeys = new StringBuilder();
            foreach (var key in keys)
            {
                inKeys.Append($"<value>{key}</value>");
            }

            var fetch =
                $@"<fetch>
                    <entity name='cmc_languagevalue'>
                        <attribute name='cmc_keyname'/>                        
                        <attribute name='cmc_value'/>
                    <filter>
                        <condition attribute='cmc_languagecode' operator='eq' value='{languageCode}'/>
                        <condition attribute='cmc_keyname' operator='in'>
                            {inKeys.ToString()}
                        </condition>
                    </filter>
                    </entity>
                </fetch>";

            ret = _orgService.RetrieveMultiple(new FetchExpression(fetch)).Entities
                .ToDictionary(key => key.GetAttributeValue<String>("cmc_keyname"), value => value.GetAttributeValue<String>("cmc_value"));

            //fill any missing keys
            foreach (var missingKey in keys.Except(ret.Keys))
            {
                ret.Add(missingKey, "");
            }

            return ret;
        }

        public int? GetCurrentUserLanguageCode(Guid? userId = null)
        {
            var userCondition =
                userId.HasValue ?
                    $"<condition attribute='systemuserid' operator='eq' value='{userId}'/>" :
                    "<condition attribute='systemuserid' operator='eq-userid'/>";

            var fetch =
                $@"<fetch>
                    <entity name='usersettings'>
                        <attribute name='uilanguageid'/>
                        <filter>
                            {userCondition}
                        </filter>
                    </entity>
                </fetch>";

            return _orgService.RetrieveMultiple(
                new FetchExpression(fetch))
                    .Entities.FirstOrDefault()?.GetAttributeValue<int>("uilanguageid");
        }

        public void TranslatePicklistValues<T>(IList<T> records, int? languageCode)
            where T : Entity
        {
            if (!languageCode.HasValue)
            {
                return;
            }

            var picklistAttributes = ((RetrieveEntityResponse)_orgService.Execute(new RetrieveEntityRequest()
            {
                EntityFilters = EntityFilters.Attributes,
                LogicalName = "cmc_todo"
            })).EntityMetadata.Attributes.Where(x => x is EnumAttributeMetadata);

            foreach (EnumAttributeMetadata picklistAttribute in picklistAttributes)
            {
                var recordsToTranslate = records.Where(x => x.Contains(picklistAttribute.LogicalName) && x[picklistAttribute.LogicalName] != null);
                foreach (var recordToTranslate in recordsToTranslate)
                {
                    var correctLabel = picklistAttribute.OptionSet.Options.FirstOrDefault(x => x.Value == recordToTranslate.GetAttributeValue<OptionSetValue>(picklistAttribute.LogicalName).Value)
                        ?.Label.LocalizedLabels.FirstOrDefault(x => x.LanguageCode == languageCode)?.Label;

                    if (String.IsNullOrWhiteSpace(correctLabel))
                    {
                        continue;
                    }

                    recordToTranslate.FormattedValues[picklistAttribute.LogicalName] = correctLabel;
                }
            }
        }

        public void RetrieveMultiLingualValues(Cmc.Core.Xrm.ServerExtension.Core.IExecutionContext context)
        {
            var serviceProvider = context.XrmServiceProvider;
            var pluginContext = serviceProvider.GetPluginExecutionContext();

            var keysString = pluginContext.InputParameters["Keys"] as string;
            _tracer.Trace($"LanguageService : RetrieveMultiLingualValues => Key is {keysString }");

            var keys = JsonConvert.DeserializeObject<IList<String>>(keysString);
            var result = Get(keys);
            pluginContext.OutputParameters["Output"] = JsonConvert.SerializeObject(result);
        }
    }
}
