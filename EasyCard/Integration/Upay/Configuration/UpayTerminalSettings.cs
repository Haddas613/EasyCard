using Shared.Integration;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Upay
{
    public class UpayTerminalSettings : IExternalSystemSettings
    {
        /// <summary>
        /// Gets or Sets  Email
        /// </summary>
        [DataMember(Name = "email")]
        public string Email { get; set; }

        /// <summary>
        /// Gets or Sets Password
        /// </summary>
        [DataMember(Name = "password")]
        public string Password { get; set; }

        /// <summary>
        /// Gets or Sets AuthenticateKey
        /// </summary>
        [DataMember(Name = "authenticateKey")]
        public string AuthenticateKey { get; set; }

        public Task<bool> Valid()
        {
            bool valid = true;

            if (string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(Password))
            {
                valid = false;
            }

            return Task.FromResult(valid);
        }
    }
}
