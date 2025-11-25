using NUnit.Framework;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace XmlRpcCore.Tests
{
    [TestFixture]
    public class XmlRpcXxeProtectionTests
    {
        [Test]
        public void Dtd_Is_Prohibited_By_Default()
        {
            var xxe = "<?xml version=\"1.0\"?>\n<!DOCTYPE data [ <!ENTITY xxe SYSTEM \"file:///etc/passwd\"> ]>\n<methodResponse><params><param><value><string>&xxe;</string></value></param></params></methodResponse>";
            var des = new XmlRpcCore.XmlRpcResponseDeserializer();
            // should not throw external entity expansion, instead parse as literal or error
            Assert.Throws<XmlRpcCore.XmlRpcProtocolException>(() => des.Deserialize(new StringReader(xxe)));
        }

        [Test]
        public void Depth_Limit_Is_Enforced()
        {
            // build deeply nested arrays beyond the default depth
            var sb = new StringBuilder();
            sb.Append("<?xml version=\"1.0\"?><methodResponse><params><param><value>");
            for (int i = 0; i < 200; i++) sb.Append("<array><data><value>");
            sb.Append("1");
            for (int i = 0; i < 200; i++) sb.Append("</value></data></array>");
            sb.Append("</value></param></params></methodResponse>");

            var des = new XmlRpcCore.XmlRpcResponseDeserializer();

            try
            {
                des.Deserialize(new StringReader(sb.ToString()));
                Assert.Fail("Expected exception due to excessive depth");
            }
            catch (System.Exception ex)
            {
                Assert.That(ex is XmlRpcCore.XmlRpcProtocolException || ex is XmlRpcCore.XmlRpcTransportException,
                    Is.True, "Expected XmlRpcProtocolException or XmlRpcTransportException, got: " + ex.GetType());
            }
        }
    }
}
