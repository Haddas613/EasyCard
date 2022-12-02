using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace Shared.Tests.General
{
    public class SerializationTests
    {
        [Fact]
        public void SpecialSymbolsWithNewtonsoftSettings()
        {
            string str = "[{\"name\":\":)\"}]";

            var items = JsonConvert.DeserializeObject<IEnumerable<Item>>(str);

            Assert.Equal(":)", items.First().Name);
        }
    }

    public class Item
    {
        public string Name { get; set; }
    }
}
