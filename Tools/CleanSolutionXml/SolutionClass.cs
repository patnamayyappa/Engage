using System;
using System.Xml.Serialization;
using System.Collections.Generic;
namespace CleanSolutionXml
{
    [XmlRoot(ElementName = "LocalizedName")]
    public class LocalizedName
    {
        [XmlAttribute(AttributeName = "description")]
        public string Description { get; set; }
        [XmlAttribute(AttributeName = "languagecode")]
        public string Languagecode { get; set; }
    }

    [XmlRoot(ElementName = "LocalizedNames")]
    public class LocalizedNames
    {
        [XmlElement(ElementName = "LocalizedName")]
        public LocalizedName LocalizedName { get; set; }
    }

    [XmlRoot(ElementName = "Description")]
    public class Description
    {
        [XmlAttribute(AttributeName = "description")]
        public string Description1 { get; set; }
        [XmlAttribute(AttributeName = "languagecode")]
        public string Languagecode { get; set; }
    }

    [XmlRoot(ElementName = "Descriptions")]
    public class Descriptions
    {
        [XmlElement(ElementName = "Description")]
        public Description Description { get; set; }
    }

    [XmlRoot(ElementName = "EMailAddress")]
    public class EMailAddress
    {
        [XmlAttribute(AttributeName = "nil", Namespace = "http://www.w3.org/2001/XMLSchema-instance")]
        public string Nil { get; set; }
    }

    [XmlRoot(ElementName = "SupportingWebsiteUrl")]
    public class SupportingWebsiteUrl
    {
        [XmlAttribute(AttributeName = "nil", Namespace = "http://www.w3.org/2001/XMLSchema-instance")]
        public string Nil { get; set; }
    }

    [XmlRoot(ElementName = "City")]
    public class City
    {
        [XmlAttribute(AttributeName = "nil", Namespace = "http://www.w3.org/2001/XMLSchema-instance")]
        public string Nil { get; set; }
    }

    [XmlRoot(ElementName = "County")]
    public class County
    {
        [XmlAttribute(AttributeName = "nil", Namespace = "http://www.w3.org/2001/XMLSchema-instance")]
        public string Nil { get; set; }
    }

    [XmlRoot(ElementName = "Country")]
    public class Country
    {
        [XmlAttribute(AttributeName = "nil", Namespace = "http://www.w3.org/2001/XMLSchema-instance")]
        public string Nil { get; set; }
    }

    [XmlRoot(ElementName = "Fax")]
    public class Fax
    {
        [XmlAttribute(AttributeName = "nil", Namespace = "http://www.w3.org/2001/XMLSchema-instance")]
        public string Nil { get; set; }
    }

    [XmlRoot(ElementName = "FreightTermsCode")]
    public class FreightTermsCode
    {
        [XmlAttribute(AttributeName = "nil", Namespace = "http://www.w3.org/2001/XMLSchema-instance")]
        public string Nil { get; set; }
    }

    [XmlRoot(ElementName = "ImportSequenceNumber")]
    public class ImportSequenceNumber
    {
        [XmlAttribute(AttributeName = "nil", Namespace = "http://www.w3.org/2001/XMLSchema-instance")]
        public string Nil { get; set; }
    }

    [XmlRoot(ElementName = "Latitude")]
    public class Latitude
    {
        [XmlAttribute(AttributeName = "nil", Namespace = "http://www.w3.org/2001/XMLSchema-instance")]
        public string Nil { get; set; }
    }

    [XmlRoot(ElementName = "Line1")]
    public class Line1
    {
        [XmlAttribute(AttributeName = "nil", Namespace = "http://www.w3.org/2001/XMLSchema-instance")]
        public string Nil { get; set; }
    }

    [XmlRoot(ElementName = "Line2")]
    public class Line2
    {
        [XmlAttribute(AttributeName = "nil", Namespace = "http://www.w3.org/2001/XMLSchema-instance")]
        public string Nil { get; set; }
    }

    [XmlRoot(ElementName = "Line3")]
    public class Line3
    {
        [XmlAttribute(AttributeName = "nil", Namespace = "http://www.w3.org/2001/XMLSchema-instance")]
        public string Nil { get; set; }
    }

    [XmlRoot(ElementName = "Longitude")]
    public class Longitude
    {
        [XmlAttribute(AttributeName = "nil", Namespace = "http://www.w3.org/2001/XMLSchema-instance")]
        public string Nil { get; set; }
    }

    [XmlRoot(ElementName = "Name")]
    public class Name
    {
        [XmlAttribute(AttributeName = "nil", Namespace = "http://www.w3.org/2001/XMLSchema-instance")]
        public string Nil { get; set; }
    }

    [XmlRoot(ElementName = "PostalCode")]
    public class PostalCode
    {
        [XmlAttribute(AttributeName = "nil", Namespace = "http://www.w3.org/2001/XMLSchema-instance")]
        public string Nil { get; set; }
    }

    [XmlRoot(ElementName = "PostOfficeBox")]
    public class PostOfficeBox
    {
        [XmlAttribute(AttributeName = "nil", Namespace = "http://www.w3.org/2001/XMLSchema-instance")]
        public string Nil { get; set; }
    }

    [XmlRoot(ElementName = "PrimaryContactName")]
    public class PrimaryContactName
    {
        [XmlAttribute(AttributeName = "nil", Namespace = "http://www.w3.org/2001/XMLSchema-instance")]
        public string Nil { get; set; }
    }

    [XmlRoot(ElementName = "StateOrProvince")]
    public class StateOrProvince
    {
        [XmlAttribute(AttributeName = "nil", Namespace = "http://www.w3.org/2001/XMLSchema-instance")]
        public string Nil { get; set; }
    }

    [XmlRoot(ElementName = "Telephone1")]
    public class Telephone1
    {
        [XmlAttribute(AttributeName = "nil", Namespace = "http://www.w3.org/2001/XMLSchema-instance")]
        public string Nil { get; set; }
    }

    [XmlRoot(ElementName = "Telephone2")]
    public class Telephone2
    {
        [XmlAttribute(AttributeName = "nil", Namespace = "http://www.w3.org/2001/XMLSchema-instance")]
        public string Nil { get; set; }
    }

    [XmlRoot(ElementName = "Telephone3")]
    public class Telephone3
    {
        [XmlAttribute(AttributeName = "nil", Namespace = "http://www.w3.org/2001/XMLSchema-instance")]
        public string Nil { get; set; }
    }

    [XmlRoot(ElementName = "TimeZoneRuleVersionNumber")]
    public class TimeZoneRuleVersionNumber
    {
        [XmlAttribute(AttributeName = "nil", Namespace = "http://www.w3.org/2001/XMLSchema-instance")]
        public string Nil { get; set; }
    }

    [XmlRoot(ElementName = "UPSZone")]
    public class UPSZone
    {
        [XmlAttribute(AttributeName = "nil", Namespace = "http://www.w3.org/2001/XMLSchema-instance")]
        public string Nil { get; set; }
    }

    [XmlRoot(ElementName = "UTCOffset")]
    public class UTCOffset
    {
        [XmlAttribute(AttributeName = "nil", Namespace = "http://www.w3.org/2001/XMLSchema-instance")]
        public string Nil { get; set; }
    }

    [XmlRoot(ElementName = "UTCConversionTimeZoneCode")]
    public class UTCConversionTimeZoneCode
    {
        [XmlAttribute(AttributeName = "nil", Namespace = "http://www.w3.org/2001/XMLSchema-instance")]
        public string Nil { get; set; }
    }

    [XmlRoot(ElementName = "Address")]
    public class Address
    {
        [XmlElement(ElementName = "AddressNumber")]
        public string AddressNumber { get; set; }
        [XmlElement(ElementName = "AddressTypeCode")]
        public string AddressTypeCode { get; set; }
        [XmlElement(ElementName = "City")]
        public City City { get; set; }
        [XmlElement(ElementName = "County")]
        public County County { get; set; }
        [XmlElement(ElementName = "Country")]
        public Country Country { get; set; }
        [XmlElement(ElementName = "Fax")]
        public Fax Fax { get; set; }
        [XmlElement(ElementName = "FreightTermsCode")]
        public FreightTermsCode FreightTermsCode { get; set; }
        [XmlElement(ElementName = "ImportSequenceNumber")]
        public ImportSequenceNumber ImportSequenceNumber { get; set; }
        [XmlElement(ElementName = "Latitude")]
        public Latitude Latitude { get; set; }
        [XmlElement(ElementName = "Line1")]
        public Line1 Line1 { get; set; }
        [XmlElement(ElementName = "Line2")]
        public Line2 Line2 { get; set; }
        [XmlElement(ElementName = "Line3")]
        public Line3 Line3 { get; set; }
        [XmlElement(ElementName = "Longitude")]
        public Longitude Longitude { get; set; }
        [XmlElement(ElementName = "Name")]
        public Name Name { get; set; }
        [XmlElement(ElementName = "PostalCode")]
        public PostalCode PostalCode { get; set; }
        [XmlElement(ElementName = "PostOfficeBox")]
        public PostOfficeBox PostOfficeBox { get; set; }
        [XmlElement(ElementName = "PrimaryContactName")]
        public PrimaryContactName PrimaryContactName { get; set; }
        [XmlElement(ElementName = "ShippingMethodCode")]
        public string ShippingMethodCode { get; set; }
        [XmlElement(ElementName = "StateOrProvince")]
        public StateOrProvince StateOrProvince { get; set; }
        [XmlElement(ElementName = "Telephone1")]
        public Telephone1 Telephone1 { get; set; }
        [XmlElement(ElementName = "Telephone2")]
        public Telephone2 Telephone2 { get; set; }
        [XmlElement(ElementName = "Telephone3")]
        public Telephone3 Telephone3 { get; set; }
        [XmlElement(ElementName = "TimeZoneRuleVersionNumber")]
        public TimeZoneRuleVersionNumber TimeZoneRuleVersionNumber { get; set; }
        [XmlElement(ElementName = "UPSZone")]
        public UPSZone UPSZone { get; set; }
        [XmlElement(ElementName = "UTCOffset")]
        public UTCOffset UTCOffset { get; set; }
        [XmlElement(ElementName = "UTCConversionTimeZoneCode")]
        public UTCConversionTimeZoneCode UTCConversionTimeZoneCode { get; set; }
    }

    [XmlRoot(ElementName = "Addresses")]
    public class Addresses
    {
        [XmlElement(ElementName = "Address")]
        public List<Address> Address { get; set; }
    }

    [XmlRoot(ElementName = "Publisher")]
    public class Publisher
    {
        [XmlElement(ElementName = "UniqueName")]
        public string UniqueName { get; set; }
        [XmlElement(ElementName = "LocalizedNames")]
        public LocalizedNames LocalizedNames { get; set; }
        [XmlElement(ElementName = "Descriptions")]
        public string Descriptions { get; set; }
        [XmlElement(ElementName = "EMailAddress")]
        public EMailAddress EMailAddress { get; set; }
        [XmlElement(ElementName = "SupportingWebsiteUrl")]
        public SupportingWebsiteUrl SupportingWebsiteUrl { get; set; }
        [XmlElement(ElementName = "CustomizationPrefix")]
        public string CustomizationPrefix { get; set; }
        [XmlElement(ElementName = "CustomizationOptionValuePrefix")]
        public string CustomizationOptionValuePrefix { get; set; }
        [XmlElement(ElementName = "Addresses")]
        public Addresses Addresses { get; set; }
    }

    [XmlRoot(ElementName = "RootComponent")]
    public class RootComponent
    {
        [XmlAttribute(AttributeName = "type")]
        public int Type { get; set; }
        [XmlAttribute(AttributeName = "schemaName")]
        public string SchemaName { get; set; }
        [XmlAttribute(AttributeName = "behavior")]
        public string Behavior { get; set; }
        [XmlAttribute(AttributeName = "id")]
        public string Id { get; set; }
        [XmlAttribute(AttributeName = "parentId")]
        public string ParentId { get; set; }
    }

    [XmlRoot(ElementName = "RootComponents")]
    public class RootComponents
    {
        [XmlElement(ElementName = "RootComponent")]
        public List<RootComponent> RootComponent { get; set; }
    }

    [XmlRoot(ElementName = "Required")]
    public class Required
    {
        [XmlAttribute(AttributeName = "key")]
        public int Key { get; set; }
        [XmlAttribute(AttributeName = "type")]
        public int Type { get; set; }
        [XmlAttribute(AttributeName = "schemaName")]
        public string SchemaName { get; set; }
        [XmlAttribute(AttributeName = "displayName")]
        public string DisplayName { get; set; }
        [XmlAttribute(AttributeName = "solution")]
        public string Solution { get; set; }
        [XmlAttribute(AttributeName = "parentSchemaName")]
        public string ParentSchemaName { get; set; }
        [XmlAttribute(AttributeName = "parentDisplayName")]
        public string ParentDisplayName { get; set; }
        [XmlAttribute(AttributeName = "id")]
        public string Id { get; set; }
        [XmlAttribute(AttributeName = "parentId")]
        public string ParentId { get; set; }
    }

    [XmlRoot(ElementName = "Dependent")]
    public class Dependent
    {
        [XmlAttribute(AttributeName = "key")]
        public int Key { get; set; }
        [XmlAttribute(AttributeName = "type")]
        public int Type { get; set; }
        [XmlAttribute(AttributeName = "displayName")]
        public string DisplayName { get; set; }
        [XmlAttribute(AttributeName = "parentDisplayName")]
        public string ParentDisplayName { get; set; }
        [XmlAttribute(AttributeName = "id")]
        public string Id { get; set; }
        [XmlAttribute(AttributeName = "schemaName")]
        public string SchemaName { get; set; }
        [XmlAttribute(AttributeName = "parentSchemaName")]
        public string ParentSchemaName { get; set; }
    }

    [XmlRoot(ElementName = "MissingDependency")]
    public class MissingDependency
    {
        [XmlElement(ElementName = "Required")]
        public Required Required { get; set; }
        [XmlElement(ElementName = "Dependent")]
        public Dependent Dependent { get; set; }
    }

    [XmlRoot(ElementName = "MissingDependencies")]
    public class MissingDependencies
    {
        [XmlElement(ElementName = "MissingDependency")]
        public List<MissingDependency> MissingDependency { get; set; }
    }

    [XmlRoot(ElementName = "SolutionManifest")]
    public class SolutionManifest
    {
        [XmlElement(ElementName = "UniqueName")]
        public string UniqueName { get; set; }
        [XmlElement(ElementName = "LocalizedNames")]
        public LocalizedNames LocalizedNames { get; set; }
        [XmlElement(ElementName = "Descriptions")]
        public Descriptions Descriptions { get; set; }
        [XmlElement(ElementName = "Version")]
        public string Version { get; set; }
        [XmlElement(ElementName = "Managed")]
        public string Managed { get; set; }
        [XmlElement(ElementName = "Publisher")]
        public Publisher Publisher { get; set; }
        [XmlElement(ElementName = "RootComponents")]
        public RootComponents RootComponents { get; set; }
        [XmlElement(ElementName = "MissingDependencies")]
        public MissingDependencies MissingDependencies { get; set; }
    }

    [XmlRoot(ElementName = "ImportExportXml")]
    public class ImportExportXml
    {
        [XmlElement(ElementName = "SolutionManifest")]
        public SolutionManifest SolutionManifest { get; set; }
        [XmlAttribute(AttributeName = "version")]
        public string Version { get; set; }
        [XmlAttribute(AttributeName = "SolutionPackageVersion")]
        public string SolutionPackageVersion { get; set; }
        [XmlAttribute(AttributeName = "languagecode")]
        public string Languagecode { get; set; }
        [XmlAttribute(AttributeName = "generatedBy")]
        public string GeneratedBy { get; set; }
        //[XmlAttribute(AttributeName = "xsi", Namespace = "http://www.w3.org/2000/xmlns/")]
        //public string Xsi { get; set; }
    }

}
