using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BasicServices.Services
{
    public interface IExcelService
    {
        Task<string> GenerateFile<T>(string fileName, string worksheetName, List<T> rows, Dictionary<string, string> header, TimeSpan? downloadUrlExpiration = null, string dateFormat = "yyyy-mm-dd");
        Task<string> GenerateFileWithSummaryRow<T, TSummary>(string fileName, string worksheetName, List<T> rows, Dictionary<string, string> header, TimeSpan? downloadUrlExpiration = null, string dateFormat = "yyyy-mm-dd", List<TSummary> summary = null);
    }
}
