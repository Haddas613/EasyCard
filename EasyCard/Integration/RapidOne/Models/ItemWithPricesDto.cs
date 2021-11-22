using System;
using System.Collections.Generic;
using System.Text;

namespace RapidOne.Models
{
    public class ItemWithPricesDto
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public decimal Vat { get; set; }
        public string VatGroup { get; set; }
        public bool VatLiable { get; set; }
        public ItemPriceDto[] Prices { get; set; }
        public int CategoryCode { get; set; }
        public string CategoryName { get; set; }
        public bool IsGeneric { get; set; }
        public bool IsPackage { get; set; }
        public bool IsSeries { get; set; }
        public bool IsMultiplier { get; set; }
        public bool RequireSurface { get; set; }
        public string Currency { get; set; }
        public bool PreventDiscount { get; set; }
        public bool InventoryItem { get; set; }
        public decimal Price { get; set; }
        public string PriceListName { get; set; }
        public bool IsAvailable { get; set; }
        public bool IsFrequencyItem { get; set; }


        public int? ItemType { get; set; }
        public int? VoucherType { get; set; }
        public int? VoucherMonetary { get; set; }
        public int? VoucherExpType { get; set; }
        public int? VoucherExpInMonths { get; set; }
        public DateTime? VoucherExpDate { get; set; }
    }

    public class ItemPriceDto
    {
        public int Code { get; set; }
        public string ItemCode { get; set; }
        public int PriceListCode { get; set; }
        public string PriceListName { get; set; }
        public decimal Price { get; set; }
        public decimal PriceWithVat { get; set; }
        public string Currency { get; set; }
        public decimal Deductible { get; set; }
        public decimal? Coverage { get; set; }
        public bool IsAvailable { get; set; }
        public string TreaterId { get; set; }
        public int DepartmentId { get; set; }
        public decimal? Quantity { get; set; }
        public decimal? FeeValue { get; set; }
        public decimal? FeePerc { get; set; }
        public decimal? InsFeeValue { get; set; }
        public decimal? InsFeePerc { get; set; }
    }
}
