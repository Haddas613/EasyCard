using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Integration
{
    /// <summary>
    /// Every integration settings class must implement this interface.
    /// The Integration validity will be based on Valid() method
    /// </summary>
    public interface IExternalSystemSettings
    {
        Task<bool> Valid();
    }
}
