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
            
            BitCreateRequest model = new BitCreateRequest()
            {
                RequestAmount = 100,
                CurrencyTypeCode = 1,
                DebitMethodCode = 2,
                ExternalSystemReference = null,
                RequestSubjectDescription = testValue,
                FranchisingId = 0,
                ProviderNbr = 0,
                UrlReturnAddress = null
            }; 

            var expectedResult = JsonConvert.SerializeObject(model.RequestSubjectDescription, Formatting.None, new JsonSerializerSettings
            {
                StringEscapeHandling = StringEscapeHandling.EscapeHtml
            });
                        
            JsonReader reader = new JTokenReader(JToken.Parse(JsonConvert.SerializeObject(model.RequestSubjectDescription)));

            var converter = new EscapingHtmlConverter();
            
            var result = converter.ReadJson(reader, model.RequestSubjectDescription.GetType(), testValue, null);
            Assert.NotNull(result);
            Assert.Equal(expectedResult, result?.ToString());
        }        
    }
}