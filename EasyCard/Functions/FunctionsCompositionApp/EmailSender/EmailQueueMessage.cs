using System;
using System.Collections.Generic;
using System.Text;

namespace EmailSender
{
    public class EmailQueueMessage
    {
        public EmailQueueMessage()
        {
        }

        public DateTime MessageDate { get; set; }

        public Guid MessageID { get; set; }
    }
}
