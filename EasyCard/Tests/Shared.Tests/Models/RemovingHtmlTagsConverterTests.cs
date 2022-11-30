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
            string testValue = "Some      <strong> Description</strong> with <p>html</p> tags <img src=\"https://www.google.com/images/branding/googlelogo/1x/googlelogo_color_272x92dp.png\" alt=\"Google\" width=\"272\" height=\"92\"> test <script>script code</script> <style>style code</style> <xml>xml code</xml>";
            string expectedResult = "Some Description with html tags test";
            
            DealDetails model = new DealDetails()
            {
                DealDescription = testValue,
            }; 

            JsonReader reader = new JTokenReader(JToken.Parse(JsonConvert.SerializeObject(model.DealDescription)));

            var converter = new RemovingHtmlTagsConverter();
            
            var result = converter.ReadJson(reader, model.DealDescription.GetType(), testValue, null);
            Assert.NotNull(result);
            Assert.Equal(expectedResult, result?.ToString());
        }        
    }
}