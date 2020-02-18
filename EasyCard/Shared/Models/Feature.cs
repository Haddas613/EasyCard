using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Models
{
    public class Feature
    {
        //Enabe REST API(will be payed) - and regenerate key

        //Credit card tokens for regular payments(we are charging per each cc token)

        //“Enable billing system“ - enable recurent payments, including card expiration, invoicing(optional), products list etc.

        //Enable J5 transactions

        //Disable double transactions(time to check double transaction)

        //Send recite by sms to consumer(payable feature)

        public decimal Price { get; set; }
    }
}
