using Newtonsoft.Json.Linq;
using Shared.Helpers.Configuration;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Helpers.Services
{
    public class AppInsightReaderService : IAppInsightReaderService
    {
        private readonly ApplicationInsightsSettings config;
        private readonly IWebApiClient webApiClient;
        private readonly string actionUrl;

        public AppInsightReaderService(ApplicationInsightsSettings config, IWebApiClient webApiClient)
        {
            this.config = config;
            this.webApiClient = webApiClient;
            actionUrl = $"{config.AppInsightsApiAction}{config.AiAppId}/query";
        }

        public async Task<IEnumerable<T>> GetData<T>(string query)
        {
            var httpResponse = await webApiClient.Get<JToken>(config.AppInsightsApi, actionUrl, new {
                query
            }, () => BuildHeaders());

            var columns = httpResponse.SelectToken("tables[0].columns")?.ToObject<JArray>();
            var rows = httpResponse.SelectToken("tables[0].rows")?.ToObject<JArray[]>();

            if (columns == null || rows == null)
            {
                return Enumerable.Empty<T>();
            }

            var intermediateResult = new JArray();
            var columnNames = new List<string>(columns.Count());

            foreach (var c in columns)
            {
                columnNames.Add(c.SelectToken("name").ToObject<string>());
            }

            for (int i = 0; i < rows.Length; i++)
            {
                var jo = new JObject();
                for (int j = 0; j < rows[i].Count && j < columnNames.Count; j++)
                {
                    jo[columnNames[j]] = rows[i][j];
                }

                intermediateResult.Add(jo);
            }

            return intermediateResult.ToObject<IEnumerable<T>>();
        }

        private async Task<NameValueCollection> BuildHeaders()
        {
            NameValueCollection headers = new NameValueCollection();

            headers.Add("x-api-key", config.AiAppKey);

            return await Task.FromResult(headers);
        }
    }
}
