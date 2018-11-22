using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Cmc.Engage.Contracts;

namespace Cmc.Engage.Retention.FetchBuilderSupport
{
    public interface IEntity : IFilter
    {
        string EntityName { get; set; }

        void AddAttribute(string attributeName);
        void AddAttributes(IEnumerable<string> attributeNames);

        ILinkEntity AddLinkEntity(string entityName, string from, string to);
        ILinkEntity AddLinkEntity(string entityName, string from, string to, string linkType);

        IEntity FindOrAddEntity(IEnumerable<RelatedEntity> relatedEntities, string linkType = null);

        void AddRelatedConditions(IEnumerable<IConditionEntity> conditions);
        void AddRelatedCondition(IConditionEntity condition);

    }
    public class Entity : Filter, IEntity
    {
        protected Entity() : this(new XElement("entity"))
        {
        }

        protected Entity(XElement entityElement) : base()
        {
            EntityElement = entityElement;
            FilterElement = entityElement.Element("filter");
        }

        protected XElement EntityElement { get; set; }

        protected override void OnFilterElementUpdated(XElement newValue)
        {
            base.OnFilterElementUpdated(newValue);
            EntityElement.Element("filter")?.Remove();
            if (newValue != null)
            {
                EntityElement.Add(newValue);
            }
        }

        public string EntityName
        {
            get { return EntityElement.Attribute("name").Value; }
            set { EntityElement.SetAttributeValue("name", value); }
        }
        public void AddAttribute(string attributeName)
        {
            if (!EntityElement.Elements("attribute").Any(a => a.Attribute("name")?.Value == attributeName))
            {
                EntityElement.Add(new XElement("attribute", new XAttribute("name", attributeName)));
            }
        }

        public void AddAttributes(IEnumerable<string> attributeNames)
        {
            foreach (var attributeName in attributeNames)
            {
                AddAttribute(attributeName);
            }
        }

        public ILinkEntity AddLinkEntity(string entityName, string from, string to)
        {
            var linkEntity = new LinkEntity();
            linkEntity.EntityName = entityName;
            linkEntity.From = from;
            linkEntity.To = to;
            EntityElement.Add(linkEntity.EntityElement);
            return linkEntity;
        }

        public ILinkEntity AddLinkEntity(string entityName, string from, string to, string linkType)
        {
            var link = AddLinkEntity(entityName, from, to);
            link.LinkType = linkType;
            return link;
        }

        public IEnumerable<ILinkEntity> LinkEntities()
        {
            return EntityElement.Elements("link-entity").Select(elem => new LinkEntity(elem));
        }

        public IEntity FindOrAddEntity(IEnumerable<RelatedEntity> entityChain, string linkType = null)
        {
            var top = entityChain.FirstOrDefault();
            if (top == null)
            {
                return this;
            }

            linkType = linkType ?? "inner";

            var match = LinkEntities().Where(e =>
                e.EntityName == top.EntityName &&
                e.From == top.FromAttribute &&
                e.LinkType == linkType &&
                e.To == top.ToAttribute).FirstOrDefault();

            if (match == null)
            {
                match = AddLinkEntity(top.EntityName, top.FromAttribute, top.ToAttribute, linkType);
            }

            return match.FindOrAddEntity(entityChain.Skip(1));
        }

        public void AddRelatedConditions(IEnumerable<IConditionEntity> conditions)
        {
            foreach (var condition in conditions)
            {
                AddRelatedCondition(condition);
            }
        }

        public void AddRelatedCondition(IConditionEntity condition)
        {
            var path = condition.ParseAttributeName();
            var entity = FindOrAddEntity(path.RelatedEntities);
            entity.AddCondition(condition);
        }

    }
}
