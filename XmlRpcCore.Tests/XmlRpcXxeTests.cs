using System.IO;
using System.Xml;
using NUnit.Framework;
using XmlRpcCore;

namespace XmlRpcCore.Tests
{
    [TestFixture]
    public class XmlRpcXxeTests
    {
        [Test]
        public void RequestDeserializer_Rejects_Doctype_XXE()
        {
            var xml = "<?xml version=\"1.0\"?>\n" +
                      "<!DOCTYPE root [\n" +
                      "  <!ENTITY xxe SYSTEM \"file:///c:/windows/win.ini\">\n" +
                      "]>\n" +
                      "<methodCall>\n" +
                      "  <methodName>test</methodName>\n" +
                      "  <params>\n" +
                      "    <param>\n" +
                      "      <value>&xxe;</value>\n" +
                      "    </param>\n" +
                      "  </params>\n" +
                      "</methodCall>\n";

            var deserializer = new XmlRpcRequestDeserializer();

            Assert.That(() => deserializer.Deserialize(new StringReader(xml)), Throws.TypeOf<XmlException>());
        }

        [Test]
        public void ResponseDeserializer_Rejects_Doctype_XXE()
        {
            var xml = "<?xml version=\"1.0\"?>\n" +
                      "<!DOCTYPE root [\n" +
                      "  <!ENTITY xxe SYSTEM \"file:///c:/windows/win.ini\">\n" +
                      "]>\n" +
                      "<methodResponse>\n" +
                      "  <params>\n" +
                      "    <param>\n" +
                      "      <value>&xxe;</value>\n" +
                      "    </param>\n" +
                      "  </params>\n" +
                      "</methodResponse>\n";

            var deserializer = new XmlRpcResponseDeserializer();

            Assert.That(() => deserializer.Deserialize(new StringReader(xml)), Throws.TypeOf<XmlException>());
        }
    }
}
