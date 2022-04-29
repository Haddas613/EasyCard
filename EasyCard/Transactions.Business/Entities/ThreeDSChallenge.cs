using Shared.Business;
using Shared.Helpers;
using System;
using System.Collections.Generic;
using System.Text;

namespace Transactions.Business.Entities
{
    public class ThreeDSChallenge : IEntityBase<Guid>
    {
        public ThreeDSChallenge()
        {
            MessageTimestamp = DateTime.UtcNow;
            MessageDate = TimeZoneInfo.ConvertTimeFromUtc(MessageTimestamp.Value, UserCultureInfo.TimeZone).Date;
            IntegrationMessageID = Guid.NewGuid().GetSequentialGuid(MessageTimestamp.Value);
        }

        public Guid IntegrationMessageID { get; set; }

        /// <summary>
        /// Legal transaction day
        /// </summary>
        public DateTime? MessageDate { get; set; }

        /// <summary>
        /// Date-time when transaction created initially in UTC
        /// </summary>
        public DateTime? MessageTimestamp { get; set; }

        public string Action { get; set; }

        public string ThreeDSServerTransID { get; set; }

        public Guid? TerminalID { get; set; }

        public Guid? MerchantID { get; set; }

        public string TransStatus { get; set; }

        public Guid GetID()
        {
            return IntegrationMessageID;
        }
    }
}
