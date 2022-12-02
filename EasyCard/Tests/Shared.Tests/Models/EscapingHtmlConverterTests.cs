using Xunit;
using Shared.Api.Models.Binding;
using System;
using System.Collections.Generic;
using System.Text;
using Bit.Models;
using Newtonsoft.Json;
using Shared.Helpers;
using Newtonsoft.Json.Linq;

namespace Shared.Api.Models.Binding.Tests
{
    public class EscapingHtmlConverterTests
    {

        [Fact()]
        public void CanConvertTest()
        {
            var escapingHtmlConverter = new EscapingHtmlConverter();
            Assert.True(escapingHtmlConverter.CanConvert(typeof(string)));
        }
        
        
        [Fact()]
        public void ReadJsonTest()
        {
            string testValue = "Some string <with> [special] 'characters' & more";

            EscapingHtmlConverterItem model = new EscapingHtmlConverterItem()
            {
                Name = testValue,
            }; 

            var result = JsonConvert.SerializeObject(model, Formatting.None);

            var expectedResult = "";
            Assert.NotNull(result);
            Assert.Equal(expectedResult, result?.ToString());
        }        
    }

    public class EscapingHtmlConverterItem
    {
        [JsonConverter(typeof(EscapingHtmlConverter))]
        public string Name { get; set; }
    }
}