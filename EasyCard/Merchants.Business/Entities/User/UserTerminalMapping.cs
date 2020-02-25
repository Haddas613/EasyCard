using System;
using System.Collections.Generic;
using System.Text;


namespace Merchants.Business.Entities.User
{
    public class UserTerminalMapping
    {
        public long UserTerminalMappingID { get; set; }

        public string UserID { get; set; }

        public long TerminalID { get; set; }

        public Terminal.Terminal Terminal { get; set; }

        public DateTime OperationDate { get; set; }

        public string OperationDoneBy { get; set; }

        public string OperationDoneByID { get; set; }
    }
}
