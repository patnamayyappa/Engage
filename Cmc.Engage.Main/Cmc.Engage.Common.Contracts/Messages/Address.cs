
using System.Collections.Generic;

using System.Xml.Serialization;

namespace Cmc.Engage.Common.Messages
{
    [XmlRoot(ElementName = "Address", Namespace = "http://schemas.microsoft.com/search/local/2010/5/geocode")]
    public class Address
    {
        [XmlAttribute(AttributeName = "AddressLine")]
        public string AddressLine { get; set; }

        [XmlAttribute(AttributeName = "AdminDistrict")]
        public string AdminDistrict { get; set; }

        [XmlAttribute(AttributeName = "Locality")]
        public string Locality { get; set; }

        [XmlAttribute(AttributeName = "PostalCode")]
        public string PostalCode { get; set; }

        [XmlAttribute(AttributeName = "CountryRegion")]
        public string CountryRegion { get; set; }

        [XmlAttribute(AttributeName = "AdminDistrict2")]
        public string AdminDistrict2 { get; set; }

        [XmlAttribute(AttributeName = "FormattedAddress")]
        public string FormattedAddress { get; set; }

        [XmlAttribute(AttributeName = "Neighborhood")]
        public string Neighborhood { get; set; }
    }

    [XmlRoot(ElementName = "GeocodeRequest", Namespace = "http://schemas.microsoft.com/search/local/2010/5/geocode")]
    public class GeocodeRequest
    {
        [XmlElement(ElementName = "Address", Namespace = "http://schemas.microsoft.com/search/local/2010/5/geocode")]
        public Address Address { get; set; }

        [XmlAttribute(AttributeName = "Culture")]
        public string Culture { get; set; }

        [XmlAttribute(AttributeName = "IncludeNeighborhood")]
        public string IncludeNeighborhood { get; set; }

        [XmlElement(ElementName = "ConfidenceFilter",
            Namespace = "http://schemas.microsoft.com/search/local/2010/5/geocode")]
        public ConfidenceFilter ConfidenceFilter { get; set; }

        [XmlAttribute(AttributeName = "MaxResults")]
        public string MaxResults { get; set; }

        [XmlAttribute(AttributeName = "Query")]
        public string Query { get; set; }
    }

    [XmlRoot(ElementName = "GeocodePoint", Namespace = "http://schemas.microsoft.com/search/local/2010/5/geocode")]
    public class GeocodePoint
    {
        [XmlAttribute(AttributeName = "CalculationMethod")]
        public string CalculationMethod { get; set; }

        [XmlAttribute(AttributeName = "Latitude")]
        public string Latitude { get; set; }

        [XmlAttribute(AttributeName = "Longitude")]
        public string Longitude { get; set; }

        [XmlAttribute(AttributeName = "Type")] public string Type { get; set; }

        [XmlAttribute(AttributeName = "UsageTypes")]
        public string UsageTypes { get; set; }
    }

    [XmlRoot(ElementName = "BoundingBox", Namespace = "http://schemas.microsoft.com/search/local/2010/5/geocode")]
    public class BoundingBox
    {
        [XmlAttribute(AttributeName = "SouthLatitude")]
        public string SouthLatitude { get; set; }

        [XmlAttribute(AttributeName = "WestLongitude")]
        public string WestLongitude { get; set; }

        [XmlAttribute(AttributeName = "NorthLatitude")]
        public string NorthLatitude { get; set; }

        [XmlAttribute(AttributeName = "EastLongitude")]
        public string EastLongitude { get; set; }
    }

    [XmlRoot(ElementName = "Point", Namespace = "http://schemas.microsoft.com/search/local/2010/5/geocode")]
    public class Point
    {
        [XmlAttribute(AttributeName = "Latitude")]
        public string Latitude { get; set; }

        [XmlAttribute(AttributeName = "Longitude")]
        public string Longitude { get; set; }
    }

    [XmlRoot(ElementName = "GeocodeResponse", Namespace = "http://schemas.microsoft.com/search/local/2010/5/geocode")]
    public class GeocodeResponse
    {
        [XmlElement(ElementName = "Address", Namespace = "http://schemas.microsoft.com/search/local/2010/5/geocode")]
        public Address Address { get; set; }

        [XmlElement(ElementName = "GeocodePoint",
            Namespace = "http://schemas.microsoft.com/search/local/2010/5/geocode")]
        public List<GeocodePoint> GeocodePoint { get; set; }

        [XmlElement(ElementName = "BoundingBox",
            Namespace = "http://schemas.microsoft.com/search/local/2010/5/geocode")]
        public BoundingBox BoundingBox { get; set; }

        [XmlElement(ElementName = "Point", Namespace = "http://schemas.microsoft.com/search/local/2010/5/geocode")]
        public Point Point { get; set; }

        [XmlAttribute(AttributeName = "Name")] public string Name { get; set; }

        [XmlAttribute(AttributeName = "EntityType")]
        public string EntityType { get; set; }

        [XmlAttribute(AttributeName = "Confidence")]
        public string Confidence { get; set; }

        [XmlAttribute(AttributeName = "MatchCodes")]
        public string MatchCodes { get; set; }
    }

    [XmlRoot(ElementName = "GeocodeEntity", Namespace = "http://schemas.microsoft.com/search/local/2010/5/geocode")]
    public class GeocodeEntity
    {
        [XmlElement(ElementName = "GeocodeRequest",
            Namespace = "http://schemas.microsoft.com/search/local/2010/5/geocode")]
        public GeocodeRequest GeocodeRequest { get; set; }

        [XmlElement(ElementName = "GeocodeResponse",
            Namespace = "http://schemas.microsoft.com/search/local/2010/5/geocode")]
        public GeocodeResponse GeocodeResponse { get; set; }

        [XmlElement(ElementName = "StatusCode", Namespace = "http://schemas.microsoft.com/search/local/2010/5/geocode")]
        public string StatusCode { get; set; }

        [XmlElement(ElementName = "TraceId", Namespace = "http://schemas.microsoft.com/search/local/2010/5/geocode")]
        public string TraceId { get; set; }

        [XmlAttribute(AttributeName = "Id")] public string Id { get; set; }

        [XmlAttribute(AttributeName = "xmlns")]
        public string Xmlns { get; set; }
    }

    [XmlRoot(ElementName = "ConfidenceFilter", Namespace = "http://schemas.microsoft.com/search/local/2010/5/geocode")]
    public class ConfidenceFilter
    {
        [XmlAttribute(AttributeName = "MinimumConfidence")]
        public string MinimumConfidence { get; set; }
    }

    [XmlRoot(ElementName = "GeocodeFeed", Namespace = "http://schemas.microsoft.com/search/local/2010/5/geocode")]
    public class GeocodeFeed
    {
        [XmlElement(ElementName = "GeocodeEntity",
            Namespace = "http://schemas.microsoft.com/search/local/2010/5/geocode")]
        public List<GeocodeEntity> GeocodeEntity { get; set; }

        [XmlAttribute(AttributeName = "Version")]
        public string Version { get; set; }

        [XmlAttribute(AttributeName = "xmlns")]
        public string Xmlns { get; set; }
    }
}
