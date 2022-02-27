using System;
using System.Collections.Generic;
using System.Text;

namespace Ecwid.Security
{
    public interface IEcwidCryptoService
    {
        string Decrypt(string encryptedText);
    }
}
