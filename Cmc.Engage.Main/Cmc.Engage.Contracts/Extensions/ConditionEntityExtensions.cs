using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace Cmc.Engage.Contracts
{
    public static class ConditionEntityExtensions
    {
        public static AttributePath ParseAttributeName(this IConditionEntity conditionEntity)
        {
            // example path: account(accountid=parentcustomerid).contact(primarycontactid=contactid).fullname
            if (string.IsNullOrWhiteSpace(conditionEntity.cmc_attributename))
                throw new InvalidOperationException($"Blank attribute is not allowed."); ;

            string[] parts = conditionEntity.cmc_attributename.Split('.');
            AttributePath result = new AttributePath();
            result.AttributeName = parts.Last();

            Regex related = new Regex(@"\s*(?<entity>\S+)\s*\(\s*(?<from>\S+)\s*=\s*(?<to>\S+)\s*\)\s*");
            foreach (var part in parts.Take(parts.Length - 1))
            {
                var match = related.Match(part);
                if (match.Success)
                {
                    result.RelatedEntities.Add(new RelatedEntity
                    {
                        EntityName = match.Groups["entity"].Value,
                        FromAttribute = match.Groups["from"].Value,
                        ToAttribute = match.Groups["to"].Value
                    });
                }
                else
                {
                    throw new InvalidOperationException($"Malformed attribute path: {conditionEntity.cmc_attributename}");
                }
            }
            return result;
        }
    }
}
