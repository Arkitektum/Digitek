


using System;
using System.IO;
using System.Xml.Serialization;

namespace digitek.brannProsjektering
{
    public class SerializeUtil
    {
        public T DeserializeByggesakFromString<T>(string objectData)
        {
            var classObject = (T)DeserializeByggesakFromString(objectData, typeof(T));
           return classObject;
        }

        private object DeserializeByggesakFromString(string objectData, Type type)
        {
            var serializer = new XmlSerializer(type);
            serializer.UnknownElement += new XmlElementEventHandler(Serializer_UnknownElement);
            serializer.UnknownNode += new XmlNodeEventHandler(serializer_UnknownNode);
            serializer.UnknownAttribute += new XmlAttributeEventHandler(serializer_UnknownAttribute);
            serializer.UnreferencedObject += new UnreferencedObjectEventHandler(Serializer_UnreferencedObject);

            object result = null;

            TextReader reader = null;
            try
            {
                string standardizedXmlString = objectData;
                reader = new StringReader(standardizedXmlString);
                result = serializer.Deserialize(reader);
            }
            catch (Exception e)
            {
                throw;
            }
            finally
            {
                reader?.Close();
            }

            return result;
        }

        // To debug xml
        private static void Serializer_UnreferencedObject(object sender, UnreferencedObjectEventArgs e)
        {

            var objectBeingDeserialized = e.UnreferencedObject.ToString();
            var unreferencedId = e.UnreferencedId;
            var unreferencedObject = e.UnreferencedObject;
        }
        private static void Serializer_UnknownElement(object sender, XmlElementEventArgs e)
        {
            var objectBeingDeserialized = e.ObjectBeingDeserialized.ToString();
            var elementName = e.Element.Name;
            var elementInnerXml = e.Element.InnerXml;
            var lineNumber = e.LineNumber;
            var linePosition = e.LinePosition;
        }
        private static void serializer_UnknownNode(object sender, XmlNodeEventArgs e)
        {
            var name = e.Name;
            var objectBeingDeserialized = e.ObjectBeingDeserialized.ToString();
            var localName = e.LocalName;
            var namespaceURI = e.NamespaceURI;
            var text = e.Text;

        }
        private static void serializer_UnknownAttribute(object sender, XmlAttributeEventArgs e)
        {
            var attrName = e.Attr.Name;
            var attrInnerXml = e.Attr.InnerXml;
            var lineNumber = e.LineNumber;
            var linePosition = e.LinePosition;
            var objectBeingDeserialized = e.ExpectedAttributes;
        }
    }
}