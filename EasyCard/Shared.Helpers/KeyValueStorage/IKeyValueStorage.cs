using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Helpers.KeyValueStorage
{
    public interface IKeyValueStorage<T>
        where T : class
    {
        Task Save(string key, string value);

        Task<T> Get(string key);

        Task Delete(string key);
    }
}
