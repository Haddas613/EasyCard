using Merchants.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Merchants.Business.Models.Merchant
{
    public class UpdateUserStatusData
    {
        public Guid UserID { get; set; }

        public string DisplayName { get; set; }

        public string Email { get; set; }

        public UserStatusEnum Status { get; set; }

        public UserActivityEnum UserActivity { get; set; }
    }
}
