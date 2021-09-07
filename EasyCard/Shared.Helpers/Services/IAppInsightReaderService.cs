using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Helpers.Services
{
    public interface IAppInsightReaderService
    {
        /// <summary>
        /// Retrieves data from ApplicationInsights api
        /// </summary>
        /// <param name="query">Kusto QL query</param>
        /// <returns></returns>
        Task<IEnumerable<T>> GetData<T>(string query);
    }
}
