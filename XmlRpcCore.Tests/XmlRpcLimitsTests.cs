using System;
using System.IO;
using System.Text;
using System.Xml;
using NUnit.Framework;
using XmlRpcCore;

namespace XmlRpcCore.Tests
{
    [TestFixture]
    public class XmlRpcLimitsTests
    {
        [Test]
        public void Exceed_MaxNodeCount_Throws()
        {
            var original = XmlRpcDeserializer.MaxNodeCount;
            try
            {
                XmlRpcDeserializer.MaxNodeCount = 5;
                // craft simple xml with many elements
                var sb = new StringBuilder();
                sb.AppendLine("<methodCall>");
                sb.AppendLine("<methodName>m</methodName>");
                sb.AppendLine("<params>");
                for (int i = 0; i < 10; i++)
                {
                    sb.AppendLine("<param><value>1</value></param>");
                }
                sb.AppendLine("</params>");
                sb.AppendLine("</methodCall>");

                var deser = new XmlRpcRequestDeserializer();
                Assert.That(() => deser.Deserialize(new StringReader(sb.ToString())), Throws.InstanceOf<XmlException>());
            }
            finally
            {
                XmlRpcDeserializer.MaxNodeCount = original;
            }
        }

        [Test]
        public void Exceed_MaxElementDepth_Throws()
        {
            var original = XmlRpcDeserializer.MaxElementDepth;
            try
            {
                XmlRpcDeserializer.MaxElementDepth = 3;
                var sb = new StringBuilder();
                sb.AppendLine("<methodCall>");
                sb.AppendLine("<methodName>m</methodName>");
                sb.AppendLine("<params>");
                sb.AppendLine("<param>");
                // create deep nesting
                sb.AppendLine("<value>");
                for (int i = 0; i < 10; i++)
                {
                    sb.AppendLine("<array>");
                    sb.AppendLine("<data>");
                    sb.AppendLine("<value>");
                }
                for (int i = 0; i < 10; i++)
                {
                    sb.AppendLine("</value>");
                    sb.AppendLine("</data>");
                    sb.AppendLine("</array>");
                }
                sb.AppendLine("</param>");
                sb.AppendLine("</params>");
                sb.AppendLine("</methodCall>");

                var deser = new XmlRpcRequestDeserializer();
                Assert.That(() => deser.Deserialize(new StringReader(sb.ToString())), Throws.InstanceOf<XmlException>());
            }
            finally
            {
                XmlRpcDeserializer.MaxElementDepth = original;
            }
        }
    }
}
