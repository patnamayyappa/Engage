using System.Collections.Generic;

namespace Cmc.Engage.Retention
{
    public class AttributePath
    {
        public string AttributeName { get; set; }
        public IList<RelatedEntity> RelatedEntities { get; } = new List<RelatedEntity>();
        public override string ToString()
        {
            return ToString(false);
        }

        public string ToString(bool omitAttributeName)
        {
            string relationships = string.Join(".", RelatedEntities);
            if (omitAttributeName)
            {
                return relationships;
            }
            else
            {
                if (string.IsNullOrEmpty(relationships))
                {
                    return AttributeName;
                }
                else
                {
                    return string.Concat(relationships, ".", AttributeName);
                }
            }
        }
    }
}
