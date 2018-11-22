using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cmc.Core.Xrm.ServerExtension.Core;
using Cmc.Core.Xrm.ServerExtension.Logging;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Cmc.Engage.Models;
using Microsoft.Xrm.Sdk.Workflow;
using Autofac;


namespace Cmc.Engage.Common
{
    public class ConfigurationService : IConfigurationService
    {
        private readonly IOrganizationService _orgService;

        private readonly ILogger _trace;
        public ConfigurationService(ILogger trace, IOrganizationService orgService)
        {
            _trace = trace ?? throw new ArgumentNullException(nameof(trace));        
            _orgService = orgService;
        }
                
        /// <summary>
        /// Get All Active Configuration 
        /// </summary>
        /// <returns></returns>
        public cmc_configuration GetActiveConfiguration()
        {
            _trace.Info($"ConfigurationService: Get Active Configuration");
            
            var result = _orgService.RetrieveMultiple(new FetchExpression($@"
                <fetch mapping='logical' output-format='xml-platform' version='1.0' >						
                   <entity name='cmc_configuration'>                      
                        <all-attributes />                       
                        <filter type='and'>
                        <condition attribute='statecode' operator='eq' value='0' />
                       </filter>
                        </entity>
                </fetch>"));
            if (result.Entities.Count <= 0)
            {
                return new cmc_configuration();
            }
            _trace.Info($"ConfigurationService: Got {result.Entities.Count} Active Configuration");
            return result.Entities[0].ToEntity<cmc_configuration>();            
        }
        
        public Dictionary<string, string> GetConfigurationDetails(List<string> keyNamesList)
        {
            var inKeys = new StringBuilder();
            foreach (var key in keyNamesList)
            {
                inKeys.Append($"<value>{key}</value>");
            }


            var fetchXml = $@"
                <fetch>                                       
                   <entity name='cmc_configuration'>  
                        <attribute name='cmc_configurationname'/>
                        <attribute name='cmc_value' />                       
                        <filter type='and'>                      
                        <condition attribute='cmc_configurationname' operator='in'>
                            {inKeys}
                        </condition>
                       </filter>
                        </entity>
                </fetch>";

            var data = _orgService.RetrieveMultiple(new FetchExpression(fetchXml));
            if (data?.Entities == null || data.Entities.Count <= 0) return null;
            return data.Entities.Cast<cmc_configuration>().ToList().ToDictionary(a => a.cmc_configurationname, a => a.cmc_Value);
        }
    }
}
