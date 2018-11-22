using System;
using System.Linq;
using Cmc.Core.Xrm.ServerExtension.Logging;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace Cmc.Engage.Common
{
    public class GetPortalUserLanguageCode
    {
        
        private IOrganizationService OrgService;
        private ILogger _traceService;
        public GetPortalUserLanguageCode(ILogger tracer, IOrganizationService organizationService)
        {
            _traceService = tracer ?? throw new ArgumentNullException(nameof(tracer));
            OrgService = organizationService ?? throw new ArgumentNullException(nameof(organizationService));
        }       
        public int? GetUserLanguageCode(Guid? contactId, Guid? websiteId)
        {
            int? code = null;

            if (contactId.HasValue)
            {
                code = GetContactPreferredLanguageCode(contactId.Value);
                if (code.HasValue)
                {
                    _traceService.Trace("Got contact language from preferred.");
                    return code;
                }
            }

            if (websiteId.HasValue)
            {
                code = GetPortalLanguageCode(websiteId.Value);
                if (code.HasValue)
                {
                    _traceService.Trace("Got contact language from website.");
                    return code;
                }
            }

            _traceService.Trace("Got contact language from CRM default.");
            return GetCrmLanguageCode();
        }

        private int? GetContactPreferredLanguageCode(Guid contactId)
        {
            var query = new QueryExpression("adx_portallanguage");
            query.ColumnSet = new ColumnSet("adx_lcid");
            query.TopCount = 1;
            query.Criteria.AddCondition("statecode", ConditionOperator.Equal, 0);
            query.AddLink("contact", "adx_portallanguageid", "adx_preferredlanguageid");
            query.LinkEntities[0].LinkCriteria.AddCondition("contactid", ConditionOperator.Equal, contactId);
            return OrgService.RetrieveMultiple(query).Entities.FirstOrDefault()?.GetAttributeValue<int?>("adx_lcid");
        }

        private int? GetPortalLanguageCode(Guid websiteId)
        {
            var query = new QueryExpression("adx_portallanguage");
            query.ColumnSet = new ColumnSet("adx_lcid");
            query.TopCount = 1;
            query.Criteria.AddCondition("statecode", ConditionOperator.Equal, 0);
            query.AddLink("adx_websitelanguage", "adx_portallanguageid", "adx_portallanguageid");
            query.LinkEntities[0].LinkCriteria.AddCondition("adx_websiteid", ConditionOperator.Equal, websiteId);
            return OrgService.RetrieveMultiple(query).Entities.FirstOrDefault()?.GetAttributeValue<int?>("adx_lcid");
        }

        private int? GetCrmLanguageCode()
        {
            var query = new QueryExpression("organization");
            query.ColumnSet = new ColumnSet("languagecode");
            query.TopCount = 1;
            return OrgService.RetrieveMultiple(query).Entities.FirstOrDefault()?.GetAttributeValue<int>("languagecode");
        }
    }
}
