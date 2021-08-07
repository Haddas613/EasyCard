using IdentityServer4.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServer.Models.Events
{
#pragma warning disable SA1402, SA1649 // File may only contain a single type
    public class TwoFactorInfoEvent : Event
    {
        public TwoFactorInfoEvent(string username, string name, string message)
            : base(
                  EventCategories.Authentication,
                  name,
                  EventTypes.Information,
                  0,
                  message)
        {
            Username = username;
        }

        public string Username { get; set; }
    }

    public class TwoFactorErrorEvent : Event
    {
        public TwoFactorErrorEvent(string username, string name, string error)
            : base(
                  EventCategories.Authentication,
                  name,
                  EventTypes.Error,
                  0,
                  error)
        {
            Username = username;
        }

        public string Username { get; set; }
    }

    public class ResetPasswordEvent : Event
    {
        public ResetPasswordEvent(string username, string name, string message)
            : base(
                  EventCategories.Authentication,
                  name,
                  EventTypes.Information,
                  0,
                  message)
        {
            Username = username;
        }

        public string Username { get; set; }
    }
#pragma warning restore SA1402, SA1649 // File may only contain a single type
}
