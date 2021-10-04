using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using RapidOne.Models.Enums;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace RapidOne.Models
{
    public class CreateFinancialDocumentModelDto
    {
        public string Company { get; set; }

        public string CustomerCode { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public InvoiceTypeEnum InvoiceTypeId { get; set; }

        public DateTime? InvoiceDate { get; set; }

        public DateTime? DueDate { get; set; }

        public DateTime? TaxDate { get; set; }

        public int BranchId { get; set; }

        public int DepartmentId { get; set; }

        public IEnumerable<FinDocItemDto> Items { get; set; }

        public FinDocPaymentMethodDto PaymentMethods { get; set; }

        public decimal Subtotal { get; set; }

        public decimal Discount { get; set; }

        public decimal Vat { get; set; }

        public decimal Total { get; set; }

        public decimal ToPay { get; set; }

        public string Currency { get; set; }

        public string CustomerName { get; set; }

        public string CustomerEmail { get; set; }

        public string CustomerCell { get; set; }

        [JsonExtensionData]
        public Newtonsoft.Json.Linq.JObject Extension { get; set; }
    }

    public class FinDocItemDto
    {
        public int? LineNum { get; set; }
        public string LineStatus { get; set; }
        public string DocStatus { get; set; }
        public string Code { get; set; }
        public string TaxCode { get; set; }
        public string Description { get; set; }
        public decimal Quantity { get; set; }
        public FinDocUnitPriceDto UnitPrice { get; set; }
        public decimal Subtotal { get; set; }
        public decimal Discount { get; set; }
        public decimal Vat { get; set; }
        public decimal Total { get; set; }
        public decimal ToPay { get; set; }
        public decimal Paid { get; set; }
        public decimal VatPercent { get; set; }
        public string VatGroup { get; set; }
        public decimal Rate { get; set; }
        public string Notes { get; set; }

        public bool IsDraft { get; set; }
        public bool IsIssuer { get; set; }
        public int PriceListId { get; set; }
        public bool? Charge { get; set; }
        public int? ApptSrvId { get; set; }
        public string SalesStaff { get; set; }

        public bool? IsVoucherItem { get; set; }

        public DateTime? VoucherExpiration { get; set; }
        public int? VoucherMonetary { get; set; }
        public int? VoucherType { get; set; }

        public bool? IsVoucherFullfill { get; set; }
        public string VoucherNumber { get; set; }

        // there is no null-check in R1 so this empty array required
        public string[] Users { get; set; }

        [JsonExtensionData]
        public Newtonsoft.Json.Linq.JObject Extension { get; set; }
    }

    public class FinDocUnitPriceDto
    {
        public decimal Value { get; set; }
        public string Currency { get; set; } = "₪";
    }

    public class FinDocPaymentMethodDto
    {
        public string CashAccount { get; set; }
        public string BankAccount { get; set; }
        public string Currency { get; set; }
        public IEnumerable<FinDocCashDto> Cash { get; set; }
        public IEnumerable<FinDocCheckDto> Check { get; set; }
        public IEnumerable<FinDocCreditCardDto> CreditCard { get; set; }
        public IEnumerable<FinDocMoneyTransferDto> MoneyTransfer { get; set; }
    }

    public class FinDocCashDto
    {
        public decimal Value { get; set; }
    }

    public class FinDocCheckDto
    {
        public DateTime DueDate { get; set; }
        public BankDto Bank { get; set; }
        public string Branch { get; set; }
        public string AccountNumber { get; set; }
        public int CheckNumber { get; set; }
        public decimal Value { get; set; }
    }

    public class FinDocCreditCardDto
    {
        public int? Type { get; set; }
        public string Number { get; set; }
        public FinDocCreditCardExpDto Expiration { get; set; }
        public decimal Value { get; set; }
        public int DealType { get; set; }
        public int? Payments { get; set; }
        public decimal? FirstPayment { get; set; }
        public decimal? Remaining { get; set; }
        public string VoucherNum { get; set; }
        public DateTime? FirstDue { get; set; }
    }

    public class FinDocMoneyTransferDto
    {
        public DateTime DueDate { get; set; }
        public BankDto Bank { get; set; }
        public string Branch { get; set; }
        public string AccountNumber { get; set; }
        public string Account { get; set; }
        public string ReferenceNumber { get; set; }
        public decimal Value { get; set; }
    }

    public class FinDocCreditCardExpDto
    {
        public int Month { get; set; }
        public int Year { get; set; }
    }

    public class BankDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public string CountryCode { get; set; } = "IL";
    }
}
