using Xunit;
using Shared.Api.Models.Binding;
using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Shared.Helpers;
using Newtonsoft.Json.Linq;
using Transactions.Business.Entities;

namespace Shared.Api.Models.Binding.Tests
{
    public class RemovingHtmlTagsConverterTests
    {

        [Fact()]
        public void CanConvertTest()
        {
            var removingHtmlTagsConverter = new RemovingHtmlTagsConverter();
            Assert.True(removingHtmlTagsConverter.CanConvert(typeof(string)));
        }


        [Fact()]
        public void ReadJsonTest_Ok()
        {
            string testValue = "{\"name\":\"Some<strong> Description</ strong > with < p > html </ p > tags < img src =\\\"https://www.google.com/images/branding/googlelogo/1x/googlelogo_color_272x92dp.png\\\" alt=\\\"Google\\\" width=\\\"272\\\" height=\\\"92\\\"> test\"}";
            string expectedResult = "Some Description with  html  tags  test";

            var result = JsonConvert.DeserializeObject<RemovingHtmlTagsConverterItem>(testValue);
            Assert.NotNull(result);
            Assert.Equal(expectedResult, result.Name);
        }        
    }

    public class RemovingHtmlTagsConverterItem
    {
        [JsonConverter(typeof(RemovingHtmlTagsConverter))]
        public string Name { get; set; }
    }
}