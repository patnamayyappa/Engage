using System.Xml.Linq;

namespace Cmc.Engage.Retention.FetchBuilderSupport
{
    public interface ILinkEntity : IEntity
    {
        string To { get; set; }
        string From { get; set; }
        string LinkType { get; set; }
        string Alias { get; set; }
    }

    public class LinkEntity : Entity, ILinkEntity
    {
        public LinkEntity() : base(new XElement("link-entity"))
        {
        }

        public LinkEntity(XElement entityElement) : base(entityElement)
        {
        }

        public string From
        {
            get { return EntityElement.Attribute("from")?.Value; }
            set { EntityElement.SetAttributeValue("from", value); }
        }

        public string To
        {
            get { return EntityElement.Attribute("to")?.Value; }
            set { EntityElement.SetAttributeValue("to", value); }
        }

        public string LinkType
        {
            get { return EntityElement.Attribute("link-type")?.Value ?? "inner"; }
            set { EntityElement.SetAttributeValue("link-type", value); }
        }

        public string Alias
        {
            get { return EntityElement.Attribute("alias")?.Value; }
            set { EntityElement.SetAttributeValue("alias", value); }
        }
    }
}
