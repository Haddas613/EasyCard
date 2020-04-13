using Merchants.Shared.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Merchants.Business.Models.Integration
{
    public class ExternalSystem
    {
#pragma warning disable IDE0051 // Remove unused private members
        /// <summary>
        /// Initializes a new instance of the <see cref="ExternalSystem"/> class.
        /// Used by Newtonsoft.Json to construct readonly instance
        /// </summary>
        [JsonConstructor]
        private ExternalSystem(long externalSystemID, ExternalSystemTypeEnum type, string instanceTypeFullName,
            string settingsTypeFullName, string name, JObject settings, bool active)
        {
            ExternalSystemID = externalSystemID;
            Type = type;
            InstanceTypeFullName = instanceTypeFullName;
            SettingsTypeFullName = settingsTypeFullName;
            Name = name;
            Settings = settings;
            Active = active;
        }
#pragma warning restore IDE0051 // Remove unused private members

        public long ExternalSystemID { get; }

        public ExternalSystemTypeEnum Type { get; }

        /// <summary>
        /// Fully qualified Type name (used to create instances of class)
        /// </summary>
        public string InstanceTypeFullName { get; }

        /// <summary>
        /// Fully qualified Type name of settings class (used to deserialize settings)
        /// </summary>
        public string SettingsTypeFullName { get; }

        public string Name { get; }

        public JObject Settings { get; }

        public bool Active { get; }
    }
}
