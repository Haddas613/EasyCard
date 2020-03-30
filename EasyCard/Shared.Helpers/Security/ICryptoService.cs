using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Shared.Helpers.Security
{
    public interface ICryptoService
    {
        string Encrypt(string textToEncrypt);

        string Decrypt(string encryptedText);

        string EncryptWithExpiration(string textToEncrypt, TimeSpan expiredIn);

        string DecryptWithExpiration(string encryptedText);

        string EncryptCompact(string textToEncrypt);

        string DecryptCompact(string encryptedText);
    }
}
