using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml.Serialization;

namespace CleanSolutionXml
{
    class Program
    {
        static void Main(string[] args)
        {
            //string filePath = @"C:\Data\Work\Engage\Dynamics 365 Solutions\CampusManagement\Other\Solution.xml";
            string filePath = args[0];
            XmlSerializer serializer = new XmlSerializer(typeof(ImportExportXml));
            ImportExportXml importExportXml;
            using (FileStream fileStream = new FileStream(filePath, FileMode.Open))
            {
                importExportXml = (ImportExportXml)serializer.Deserialize(fileStream);
                importExportXml = SortXml(importExportXml);
            }

            TextWriter txtWriter = new StreamWriter(filePath);
            serializer.Serialize(txtWriter, importExportXml);
            txtWriter.Close();
        }

        private static ImportExportXml SortXml(ImportExportXml importExportXml)
        {
            importExportXml.SolutionManifest.RootComponents.RootComponent.Sort(new RootComponentCompare());
            importExportXml.SolutionManifest.MissingDependencies.MissingDependency.Sort(new MissingDependencyComparer());
            return importExportXml;
        }
    }
}
