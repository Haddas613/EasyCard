using Merchants.Business.Models.Integration;
using Merchants.Shared.Enums;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Merchants.Business.Services
{
    public class ExternalSystemService : IExternalSystemsService
    {
        public ExternalSystemService(string filename = "external-systems.json")
        {
            ExternalSystems = JsonConvert.DeserializeObject<ICollection<ExternalSystem>>(File.ReadAllText(filename)).Where(es => es.Active).ToList().AsReadOnly();
        }

        public IReadOnlyList<ExternalSystem> ExternalSystems { get; }

        public ExternalSystem GetExternalSystem(long externalSystemID) =>
            ExternalSystems.FirstOrDefault(es => es.ExternalSystemID == externalSystemID);

        public IEnumerable<ExternalSystem> GetExternalSystems()
        {
            return ExternalSystems;
        }
    }
}
