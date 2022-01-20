using System;
using System.Collections.Generic;
using System.Text;

namespace Ecwid.Models
{
    public class EcwidOrderItem
    {
        public int Id { get; set; }
        public int ProductId { get; set; }

        /// <summary>
        /// ID of category this product was added to cart from. If the product was added to cart from API or Search page, categoryID will return -1
        /// </summary>
        public int CategoryId { get; set; }

        /// <summary>
        /// Price of ordered item in the cart including product options and variations. Excludes discounts, taxes
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// Product price as set by merchant in Ecwid Control Panel including product variation pricing. Excludes product options markups, wholesale discounts etc.
        /// </summary>
        public decimal ProductPrice { get; set; }
        public string Sku { get; set; }
        public decimal Quantity { get; set; }
        public string ShortDescription { get; set; }
        public decimal Tax { get; set; }
        public string Name { get; set; }

        /// <summary>
        /// true/false: shows whether a discount coupon is applied for this item
        /// </summary>
        public bool CouponApplied { get; set; }
        /// <summary>
        /// Coupon discount amount applied to item. Provided if discount applied to order. Is not recalculated if order is updated later manually
        /// </summary>
        public decimal CouponAmount { get; set; }

        /// <summary>
        /// Discounts applied to order item 'as is'. Provided if discounts are applied to order (not including discount coupons)
        /// and are not recalculated if order is updated later manually
        /// </summary>
        public IEnumerable<EcwidItemDiscount> Discounts { get; set; }
    }
}
