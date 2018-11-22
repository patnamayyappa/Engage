using System.Xml.Linq;

namespace Cmc.Engage.Retention
{
    public class FetchBuilder : FetchBuilderSupport.Entity
    {
        public FetchBuilder() : base()
        {
            FetchElement = new XElement("fetch");
            FetchElement.Add(EntityElement);
        }

        protected XElement FetchElement { get; set; }

        public string PagingCookie
        {
            get { return FetchElement.Attribute("paging-cookie")?.Value; }
            set { FetchElement.SetAttributeValue("paging-cookie", value); }
        }

        public int Page
        {
            get { return (int?)FetchElement.Attribute("page") ?? 1; }
            set { FetchElement.SetAttributeValue("page", value); }
        }

        public int Count
        {
            get { return (int?)FetchElement.Attribute("count") ?? 5000; }
            set { FetchElement.SetAttributeValue("count", value); }
        }

        public override string ToString()
        {
            return FetchElement.ToString(SaveOptions.DisableFormatting);
        }
    }
}
