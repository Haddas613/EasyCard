﻿using Ecwid.Models;
using MerchantProfileApi.Models.Billing;
using Newtonsoft.Json.Linq;
using Shared.Integration.Models;
using System.Collections.Generic;
using System.Linq;
using Transactions.Api.Models.PaymentRequests;
using Transactions.Shared.Enums;

namespace CheckoutPortal.Helpers
{
    // TODO: Please move it to ecwid dll
    public static class EcwidConversionsHelper
    {
        /// <summary>
        /// Converts ecwid payload to create payment request
        /// </summary>
        /// <returns></returns>
        public static PaymentRequestCreate GetCreatePaymentRequest(this EcwidPayload payload)
        {
            var ecwidOrder = payload.Cart.Order;

            var transaction = new PaymentRequestCreate
            {
                Currency = payload.Cart.Currency,

                PaymentRequestAmount = ecwidOrder.Total,
                VATTotal = ecwidOrder.Tax,
                NetTotal = ecwidOrder.Total - ecwidOrder.Tax,
                VATRate = (ecwidOrder.CustomerTaxExempt || ecwidOrder.Tax == 0) ? 0m : (ecwidOrder.Total / ecwidOrder.Tax) - 1,

                CardOwnerNationalID = ecwidOrder.CustomerTaxId,

                DealDetails = new DealDetails
                {
                    ConsumerEmail = ecwidOrder.Email,
                    Items = MapEcwidItemsToECNGItems(ecwidOrder.Items),
                },

                RedirectUrl = payload.ReturnUrl,
                Extension = JObject.FromObject(new EcwidTransactionExtension
                {
                    ReferenceTransactionID = ecwidOrder.ReferenceTransactionId,
                    StoreID = payload.StoreId,
                    Token = payload.Token,
                })
            };

            if (ecwidOrder.BillingPerson != null)
            {
                transaction.DealDetails.ConsumerAddress = new Address
                {
                    City = ecwidOrder.BillingPerson.City,
                    CountryCode = ecwidOrder.BillingPerson.CountryCode,
                    Street = ecwidOrder.BillingPerson.Street,
                    Zip = ecwidOrder.BillingPerson.PostalCode
                };
                transaction.DealDetails.ConsumerPhone = ecwidOrder.BillingPerson.Phone;
            }

            return transaction;
        }

        /// <summary>
        /// Maps ecwid request to create consumer request. May be used in case when customer does not exist in ECNG.
        /// </summary>
        /// <param name="ecwidOrder">Ecwid data</param>
        /// <returns>Consumer request</returns>
        public static ConsumerRequest GetConsumerRequest(this EcwidOrder ecwidOrder)
        {
            if (ecwidOrder.BillingPerson == null)
            {
                return null;
            }

            var request = new ConsumerRequest
            {
                ConsumerPhone = ecwidOrder.BillingPerson.Phone,
                ConsumerEmail = ecwidOrder.Email,
                ConsumerName = ecwidOrder.BillingPerson.Name,
                ConsumerNationalID = ecwidOrder.CustomerTaxId ?? ecwidOrder.BillingPerson.CompanyName,
                //ExternalReference = ecwidOrder.CustomerId,
                //Origin = DocumentOriginEnum.Ecwid.ToString(),
                Active = true,
            };

            request.ConsumerAddress = new Address
            {
                City = ecwidOrder.BillingPerson.City,
                CountryCode = ecwidOrder.BillingPerson.CountryCode,
                Street = ecwidOrder.BillingPerson.Street,
                Zip = ecwidOrder.BillingPerson.PostalCode
            };

            return request;
        }

        private static IEnumerable<Item> MapEcwidItemsToECNGItems(IEnumerable<EcwidOrderItem> source)
        {
            return source.Select(e => new Item
            {
                EcwidItemId = e.ProductId.ToString(),
                Price = e.Price,
                Quantity = e.Quantity,
                ItemName = e.Name
            });
        }
    }
}