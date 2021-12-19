using Merchants.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Merchants.Business.Entities.User
{
    public class UserTerminalMapping
    {
        public long UserTerminalMappingID { get; set; }

        public Guid UserID { get; set; }

        public Guid? TerminalID { get; set; }

        public Guid MerchantID { get; set; }

        public Terminal.Terminal Terminal { get; set; }

        public DateTime OperationDate { get; set; }

        public string OperationDoneBy { get; set; }

        public Guid? OperationDoneByID { get; set; }

        public string DisplayName { get; set; }

        public string Email { get; set; }

        public UserStatusEnum Status { get; set; }

        public IEnumerable<string> Roles { get; set; }

        public IEnumerable<Guid?> Terminals { get; set; }
    }
}
