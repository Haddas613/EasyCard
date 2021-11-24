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

    public class ItemDto
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string ForeignName { get; set; }
        public int CategoryCode { get; set; }
        public string CategoryName { get; set; }
        public int ManufacturerCode { get; set; }
        public int InventoryItem { get; set; }
        public int Active { get; set; }
        public bool VatLiable { get; set; }
        public string VatGroup { get; set; }
        public bool PreventDiscount { get; set; }
        public bool IsGeneric { get; set; }
        public bool IsPackage { get; set; }
        public bool IsSeries { get; set; }
        public int NoAuthorization { get; set; }
        public bool IsMultiplier { get; set; }
        public bool RequireSurface { get; set; }
        //public SeriesItemType? SeriesType { get; set; }
        public int? SeriesDays { get; set; }
        public string Currency { get; set; }
        public bool AutomaticRecall { get; set; }
        public int? AutomaticRecallValue { get; set; }
        public string AutomaticRecallPeriod { get; set; }
        public double Vat { get; set; }
        public string Barcode { get; set; }
        public bool RequireCreditCardInfo { get; set; }
        public string OriginalCode { get; set; }
        //public PackageItemDto[] PackageItems { get; set; }
        //public AlternativeItemDto[] AlternativeItems { get; set; }
        //public ItemDurationDto[] Durations { get; set; }
        public ItemPriceDto[] Prices { get; set; }

        public int? ExternalSectionId { get; set; }
        public bool ShowInCustomerPortal { get; set; }
        public bool CopyDescrToInvNotes { get; set; }
        //public List<ItemStaffDto> Staff { get; set; }
        public string ParentItemCode { get; set; }

        public string ColorHexCode { get; set; }
        public int? RequireFormLinking { get; set; }
        public bool RequireAdvancePayment { get; set; }
        public bool AssignOnSalesStaff { get; set; }
        public string AutomaticRecallOnTreatmentNumbers { get; set; }
        public bool AdditionalStockRemoval { get; set; }
        //public SubItemDto[] SubItems { get; set; }

        public double? MaterialCost { get; set; }
        public bool IncludesLabForm { get; set; }

        public int? ItemType { get; set; }
        public int? VoucherType { get; set; }
        public int? VoucherMonetary { get; set; }
        public int? VoucherExpType { get; set; }
        public int? VoucherExpInMonths { get; set; }
        public DateTime? VoucherExpDate { get; set; }

        //public VoucherItemDto[] VoucherItems { get; set; }



        public int? SubscriptionType { get; set; }
        public int? AppointmentType { get; set; }
        public int? ChargeType { get; set; }

        public int? MembershipDurationType { get; set; }
        public int? MembershipDuration { get; set; }
        public int? CancellationPolicy { get; set; }
        public bool? AllowReplacementMeeting { get; set; }
        public bool AllowUnlimitedReplacement { get; set; }
        public int? ReplacementMeetingDuration { get; set; }

        public decimal? LimitAmountDaily { get; set; }
        public bool LimitTypeDaily { get; set; }
        public decimal? LimitAmountWeekly { get; set; }
        public bool LimitTypeWeekly { get; set; }
        public decimal? LimitAmountMonthly { get; set; }
        public bool LimitTypeMonthly { get; set; }
        public decimal? LimitAmountYearly { get; set; }
        public bool LimitTypeYearly { get; set; }

    }
}
