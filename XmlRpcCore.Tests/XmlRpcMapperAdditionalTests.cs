using NUnit.Framework;
using System;
using System.Collections.Generic;
using XmlRpcCore;
using System.Linq;

namespace XmlRpcCore.Tests
{
    [TestFixture]
    public class XmlRpcMapperAdditionalTests
    {
        enum Color { Red, GreenBlue, Yellow }

        class ColorPoco
        {
            public Color Favorite { get; set; }
        }

        class DatePoco
        {
            public DateTime When { get; set; }
        }

        class LargePoco
        {
            public List<Dictionary<string, object>> Items { get; set; }
        }

        [Test]
        public void Enum_Mapping_From_Dictionary_To_Poco()
        {
            var mapper = new ObjectMapper();
            var dict = new Dictionary<string, object>
            {
                ["Favorite"] = "GreenBlue"
            };

            var result = mapper.MapTo<ColorPoco>(dict);
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Favorite, Is.EqualTo(Color.GreenBlue));
        }

        [Test]
        public void DateTime_Roundtrip_With_Json_Normalization()
        {
            var mapper = new ObjectMapper();
            var pojo = new DatePoco { When = new DateTime(2020, 12, 31, 23, 59, 59, DateTimeKind.Utc) };

            var dict = (Dictionary<string, object>)mapper.MapFrom(pojo);
            Assert.That(dict.ContainsKey("When"));
            // The MapFrom will convert DateTime into ISO string via Json and then ToPlainObject returns DateTime where possible
            var whenObj = dict["When"];
            Assert.That(whenObj, Is.TypeOf<DateTime>());
            var dt = (DateTime)whenObj;
            Assert.That(dt.ToUniversalTime(), Is.EqualTo(pojo.When.ToUniversalTime()));

            // Map back to POCO
            var round = mapper.MapTo<DatePoco>(dict);
            Assert.That(round.When.ToUniversalTime(), Is.EqualTo(pojo.When.ToUniversalTime()));
        }

        [Test]
        public void Large_Nested_Structures_MapFrom_Preserves_Shape_And_Content()
        {
            var mapper = new ObjectMapper();
            var large = new LargePoco
            {
                Items = new List<Dictionary<string, object>>()
            };

            const int itemCount = 100;
            const int depth = 5;

            for (int i = 0; i < itemCount; i++)
            {
                var dict = new Dictionary<string, object>();
                var current = dict;
                for (int d = 0; d < depth; d++)
                {
                    var child = new Dictionary<string, object>();
                    current[$"level{d}"] = child;
                    current = child;
                    current["index"] = i;
                }
                large.Items.Add(dict);
            }

            var obj = mapper.MapFrom(large);
            Assert.That(obj, Is.InstanceOf<Dictionary<string, object>>());
            var root = (Dictionary<string, object>)obj;
            Assert.That(root.ContainsKey("Items"));
            var items = root["Items"] as List<object>;
            Assert.That(items, Is.Not.Null);
            Assert.That(items.Count, Is.EqualTo(itemCount));

            // Verify nested content for a few samples
            for (int sample = 0; sample < 5; sample++)
            {
                var dictObj = items[sample] as Dictionary<string, object>;
                Assert.That(dictObj, Is.Not.Null);
                var lvl0 = dictObj["level0"] as Dictionary<string, object>;
                Assert.That(lvl0, Is.Not.Null);
                var lvl1 = lvl0["level1"] as Dictionary<string, object>;
                Assert.That(lvl1, Is.Not.Null);
                // Drill down to last level
                var last = lvl1;
                for (int d = 2; d < depth; d++)
                {
                    last = last[$"level{d}"] as Dictionary<string, object>;
                    Assert.That(last, Is.Not.Null);
                }
                // index should equal sample
                Assert.That(ConvertToInt(last["index"]), Is.EqualTo(sample));
            }
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
