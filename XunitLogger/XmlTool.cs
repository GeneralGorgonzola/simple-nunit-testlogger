namespace GeneralGorgonzola.TestLogger
{
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Xml;
    using System.Xml.Serialization;

    internal class XmlTool
    {
        private readonly XmlSerializer xmlSerializer;
        public XmlTool()
        {
            var additionalTypes = typeof(Zool.ResultType).Assembly.GetTypes()
                .Where(t => t.Namespace == nameof(Zool) && t != typeof(Zool.ResultType))
                .ToArray();
            this.xmlSerializer = new XmlSerializer(typeof(Zool.ResultType), additionalTypes);
        }

        public void SaveXML(string path, Zool.ResultType rootObject)
        {
            using (var stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                using (var exportWriter = new XmlTextWriter(stream, Encoding.UTF8))
                {
                    exportWriter.Formatting = Formatting.Indented;
                    this.xmlSerializer.Serialize(exportWriter, rootObject);
                }
            }
        }
    }
}