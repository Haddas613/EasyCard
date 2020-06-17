using System;
using System.Collections.Generic;
using System.Text;

namespace BasicServices
{
    public class EmailQueueMessage
    {
        public EmailQueueMessage()
        {

        }

        public EmailQueueMessage(DateTime messageDate, Guid messageID)
        {
            MessageDate = messageDate;
            MessageID = messageID;
        }

        public DateTime MessageDate { get; set; }

        public Guid MessageID { get; set; }
    }
}
