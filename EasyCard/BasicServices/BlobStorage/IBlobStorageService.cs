using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace BasicServices.BlobStorage
{
    public interface IBlobStorageService
    {
        Task<string> Upload(string filename, Stream stream);
    }
}
