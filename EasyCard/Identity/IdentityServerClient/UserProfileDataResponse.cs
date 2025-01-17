﻿using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityServerClient
{
    public class UserProfileDataResponse
    {
        public Guid UserID { get; set; }

        public string DisplayName { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Email { get; set; }

        public string PhoneNumber { get; set; }

        public bool AccountLocked { get; set; }

        public bool TwoFactorEnabled { get; set; }

        public ICollection<string> Roles { get; set; }

        public ICollection<Guid> Terminals { get; set; }
    }
}
