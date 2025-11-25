using NUnit.Framework;
using System.Collections.Generic;
using XmlRpcCore;
using System.Linq;

namespace XmlRpcCore.Tests
{
    [TestFixture]
    public class XmlRpcObjectMapperTests
    {
        class Inner
        {
            public int X { get; set; }
            public string Y { get; set; }
        }

        class Outer
        {
            public string Name { get; set; }
            public Inner Inner { get; set; }
            public List<int> Numbers { get; set; }
        }

        [Test]
        public void MapTo_Poco_With_Nested_Object_And_Array()
        {
            var mapper = new ObjectMapper();

            var dict = new Dictionary<string, object>
            {
                ["Name"] = "Test",
                ["Inner"] = new Dictionary<string, object>
                {
                    ["X"] = 42,
                    ["Y"] = "Hello"
                },
                ["Numbers"] = new List<object> { 1, 2, 3 }
            };

            var result = mapper.MapTo<Outer>(dict);
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Name, Is.EqualTo("Test"));
            Assert.That(result.Inner, Is.Not.Null);
            Assert.That(result.Inner.X, Is.EqualTo(42));
            Assert.That(result.Inner.Y, Is.EqualTo("Hello"));
            Assert.That(result.Numbers, Is.Not.Null);
            Assert.That(result.Numbers, Is.EqualTo(new List<int> { 1, 2, 3 }));
        }

        [Test]
        public void MapFrom_Poco_To_Dictionary_Preserves_Nested_Structure()
        {
            var mapper = new ObjectMapper();

            var pojo = new Outer
            {
                Name = "T",
                Inner = new Inner { X = 7, Y = "Z" },
                Numbers = new List<int> { 9, 8 }
            };

            var obj = mapper.MapFrom(pojo);
            Assert.That(obj, Is.InstanceOf<Dictionary<string, object>>());
            var dict = (Dictionary<string, object>)obj;
            Assert.That(dict["Name"].ToString(), Is.EqualTo("T"));
            Assert.That(dict["Inner"], Is.InstanceOf<Dictionary<string, object>>());
            var inner = (Dictionary<string, object>)dict["Inner"];
            // numeric types may be Int64 depending on serialization; compare as long
            Assert.That(ConvertToInt(inner["X"]), Is.EqualTo(7));
            Assert.That(inner["Y"].ToString(), Is.EqualTo("Z"));

            Assert.That(dict["Numbers"], Is.InstanceOf<System.Collections.Generic.List<object>>());
            var nums = ((List<object>)dict["Numbers"]).Select(o => ConvertToInt(o)).ToList();
            Assert.That(nums, Is.EqualTo(new List<int> { 9, 8 }));
        }

        private int ConvertToInt(object o)
        {
            if (o is int i) return i;
            if (o is long l) return (int)l;
            if (o is System.Text.Json.JsonElement je && je.ValueKind == System.Text.Json.JsonValueKind.Number)
            {
                return je.GetInt32();
            }
            return (int)System.Convert.ChangeType(o, typeof(int));
        }
    }
}
