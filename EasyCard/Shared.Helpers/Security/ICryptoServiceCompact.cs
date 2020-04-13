using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Helpers.Security
{
    public interface ICryptoServiceCompact
    {
        string EncryptCompact(string textToEncrypt);

        string DecryptCompact(string encryptedText);
    }
}
