using Newtonsoft.Json.Linq;
using Shared.Api.Models.Enums;
using Shared.Helpers;
using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Integration.Models
{
    public class ProcessorCreateTransactionRequest
    {
        public ProcessorCreateTransactionRequest()
        {
            CreditCardToken = new CreditCardSecureDetails();
        }

        public string PaymentTransactionID { get; set; }

        /// <summary>
        /// Shva terminal settings
        /// </summary>
        public object ProcessorSettings { get; set; }

        /// <summary>
        /// Nayax terminal settings
        /// </summary>
        public object PinPadProcessorSettings { get; set; }

        /// <summary>
        /// Nayax TerminalID optional, in case of terminal supports multiple  devices
        /// </summary>
        public string PinpadDeviceID { get; set; }

        /// <summary>
        /// Unique transaction ID
        /// </summary>
        public string PinPadTransactionID { get; set; }

        /// <summary>
        /// Request ID
        /// </summary>
        public string CorrelationId { get; set; }

        /// <summary>
        /// Original terminal
        /// </summary>
        public string EasyCardTerminalID { get; set; }

        public string SapakMutavNo { get; set; }

        /// <summary>
        /// Legal transaction day
        /// </summary>
        public DateTime? TransactionDate { get; set; }

        /// <summary>
        /// Transaction Type
        /// </summary>
        public TransactionTypeEnum TransactionType { get; set; }

        /// <summary>
        /// Special transaction type
        /// </summary>
        public SpecialTransactionTypeEnum SpecialTransactionType { get; set; }

        /// <summary>
        /// J3, J4, J5
        /// </summary>
        public JDealTypeEnum JDealType { get; set; }

        /// <summary>
        /// Currency
        /// </summary>
        public CurrencyEnum Currency { get; set; }

        /// <summary>
        /// 50 telephone deal
        /// 00 regular (megnetic)
        /// </summary>
        public CardPresenceEnum CardPresence { get; set; }

        /// <summary>
        /// Current deal (billing)
        /// </summary>
        public int? CurrentDeal { get; set; }

        /// <summary>
        /// This transaction amount
        /// </summary>
        public decimal TransactionAmount { get; set; }

        /// <summary>
        /// Initial installment payment
        /// </summary>
        public decimal InitialPaymentAmount { get; set; }

        /// <summary>
        /// TotalAmount = InitialPaymentAmount + (NumberOfInstallments - 1) * InstallmentPaymentAmount
        /// </summary>
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// Amount of one instalment payment
        /// </summary>
        public decimal InstallmentPaymentAmount { get; set; }

        /// <summary>
        /// To be used for credit or installments
        /// </summary>
        public int NumberOfPayments { get; set; }

        // TODO: why it is called CreditCardToken ?

        /// <summary>
        /// Real credit card number
        /// </summary>
        public CreditCardSecureDetails CreditCardToken { get; set; }

        /// <summary>
        /// For billing deal
        /// </summary>
        public object InitialDeal { get; set; }

        public Processor.ShvaTransactionDetails LastDealShvaDetails { get; set; }

        public string OKNumber { get; set; }

        /// <summary>
        /// For Bit Processor only
        /// </summary>
        public string BitPaymentInitiationId { get; set; }

        /// <summary>
        /// For Bit Processor only
        /// </summary>
        public string BitTransactionSerialId { get; set; }

        /// <summary>
        /// For Bit Processor only
        /// </summary>
        public string RedirectURL { get; set; }
    }
}
