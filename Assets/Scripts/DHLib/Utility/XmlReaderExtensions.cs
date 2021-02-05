using System.Xml;

namespace DHLib
{
    public static class XmlReaderExtensions
    {
        public static bool GetLocalElement(this XmlReader reader, string value)
        {
            return reader.NodeType == XmlNodeType.Element &&
                   reader.LocalName == value;
        }
    }
}