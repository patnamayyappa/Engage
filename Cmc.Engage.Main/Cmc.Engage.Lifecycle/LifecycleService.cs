using Cmc.Core.Xrm.ServerExtension.Logging;
using Cmc.Engage.Models;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;


namespace Cmc.Engage.Lifecycle
{
    public class LifecycleService : ILifecycleService
    {
        private IOrganizationService _orgService;
        private readonly ILogger _tracer;

        public LifecycleService(IOrganizationService orgService, ILogger tracer)
        {      
            _tracer = tracer;          
            _orgService = orgService;
        }

        #region RetrieveContactOpenLifecycle

        public EntityReference RetrieveContactOpenLifecycle(RetrieveContactOpenLifecycleFilters filters)
        {
            var fetchFilters = BuildOpportunityFetchFilters(filters);

            return _orgService.RetrieveMultiple(new FetchExpression($@"
                <fetch top='1' mapping='logical' version='1.0'>
                  <entity name='opportunity'>
                    <attribute name='opportunityid' />
                    <filter>
                      <condition attribute='statecode' operator='eq' value='{OpportunityState.Open}' />
                      {string.Join("", fetchFilters)}
                    </filter>
                    <order attribute='createdon' />
                  </entity>
                </fetch>")).Entities.FirstOrDefault()?.ToEntityReference();
        }

        private List<string> BuildOpportunityFetchFilters(RetrieveContactOpenLifecycleFilters filters)
        {
            _tracer.Trace("Building Opportunity Fetch Filters");

            var opportunityFetchFilters = new List<string>();
            Type filtersType = typeof(RetrieveContactOpenLifecycleFilters);

            foreach (var property in filtersType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                _tracer.Trace($"Processing {property.Name}");
                var value = property.GetValue(filters);

                _tracer.Trace($"Raw Value: {value}");
                if (value == null)
                {
                    _tracer.Trace("Value is null. A condition will not be added.");
                    continue;
                }

                var opportunityAttribute = property.GetCustomAttribute<RetrieveContactOpenLifecycleFilters.OpportunityField>().LogicalName;
                var containsDataCondition = property.GetCustomAttribute<RetrieveContactOpenLifecycleFilters.ContainsDataCondition>();
                var optionSetValueCondition = property.GetCustomAttribute<RetrieveContactOpenLifecycleFilters.OptionSetField>();

                if (containsDataCondition != null)
                {
                    if ((value as bool?) != true)
                    {
                        _tracer.Trace($"Contains Data Condition is not being added for {opportunityAttribute}");
                        continue;
                    }

                    _tracer.Trace($"A Contains Data Condition is being added for {opportunityAttribute}");
                    opportunityFetchFilters.Add($"<condition attribute='{opportunityAttribute}' operator='not-null' />");
                    continue;
                }
                else if (optionSetValueCondition != null)
                {
                    _tracer.Trace($"{opportunityAttribute} is an Option Set Value");
                    var optionSetValue = GetOpportunityOptionSetValue(opportunityAttribute, value.ToString());

                    if (optionSetValue.HasValue == false)
                    {
                        _tracer.Trace($"Cannot resolve {value} as a valid Option Set Value for {opportunityAttribute}. A filter will not be added.");
                        continue;
                    }

                    value = optionSetValue.Value;
                    _tracer.Trace($"Option Set Value is {value}");
                }
                else if (value is EntityReference)
                {
                    _tracer.Trace($"{opportunityAttribute} is an EntityReference");
                    value = ((EntityReference)value).Id;
                    _tracer.Trace($"Entity Reference Id is {value}");
                }

                _tracer.Trace($"Adding a filter of {opportunityAttribute} equals {value}");
                opportunityFetchFilters.Add($"<condition attribute='{opportunityAttribute}' operator='eq' value='{value}' />");
            }

            return opportunityFetchFilters;
        }

        private int? GetOpportunityOptionSetValue(string attribute, string value)
        {
            if (int.TryParse(value, out int parsedValue))
            {
                return parsedValue;
            }

            var response = _orgService.Execute(new RetrieveAttributeRequest()
            {
                EntityLogicalName = Opportunity.EntityLogicalName,
                LogicalName = attribute
            }) as RetrieveAttributeResponse;


            OptionSetMetadata optionSetMetadata;
            if (response.AttributeMetadata is StatusAttributeMetadata)
            {
                optionSetMetadata = ((StatusAttributeMetadata)response.AttributeMetadata).OptionSet;
            }
            else if (response.AttributeMetadata is StateAttributeMetadata)
            {
                optionSetMetadata = ((StateAttributeMetadata)response.AttributeMetadata).OptionSet;
            }
            else
            {
                optionSetMetadata = ((PicklistAttributeMetadata)response.AttributeMetadata).OptionSet;
            }

            var lowerCaseValue = value.ToLower();
            return optionSetMetadata.Options.FirstOrDefault(option => option.Label.UserLocalizedLabel.Label.ToLower() == lowerCaseValue &&
                option.Label.LocalizedLabels.Any(label => label.Label.ToLower() == lowerCaseValue))?.Value;
        }

        #endregion
    }
}
