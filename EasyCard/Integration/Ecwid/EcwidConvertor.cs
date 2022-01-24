using Ecwid.Configuration;
using Ecwid.Models;
using MerchantProfileApi.Models.Billing;
using Newtonsoft.Json;
using Shared.Integration.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Transactions.Api.Models.PaymentRequests;
using Transactions.Api.Models.Transactions;

namespace Ecwid
{
    public class EcwidConvertor
    {
        private readonly EcwidGlobalSettings config;
        private readonly EcwidDecryptor decryptor;

        private const string CUSTOMER_ORIGIN = "Ecwid";

        public EcwidConvertor(EcwidGlobalSettings config)
        {
            this.config = config;
            this.decryptor = new EcwidDecryptor(config);
        }

        public EcwidOrder DecryptEcwidOrder(string encryptedRaw)
        {
            var json = decryptor.Decrypt(encryptedRaw);

            EcwidOrder ecwidModel = null;

            try
            {
                ecwidModel = JsonConvert.DeserializeObject<EcwidOrder>(json);
            }
            catch (Exception ex)
            {
                //TODO: log
                throw;
            }
            
            return ecwidModel;
        }

        /// <summary>
        /// Converts 
        /// </summary>
        /// <param name="encryptedRaw"></param>
        /// <returns></returns>
        public PaymentRequestCreate GetCreatePaymentRequest(EcwidOrder ecwidOrder)
        {

            var transaction = new PaymentRequestCreate
            {
                Currency = ecwidOrder.Cart.Currency,

                PaymentRequestAmount = ecwidOrder.Total,
                VATTotal = ecwidOrder.Tax,
                NetTotal = ecwidOrder.Total - ecwidOrder.Tax,
                VATRate = ecwidOrder.CustomerTaxExempt ? 0m : (ecwidOrder.Total / ecwidOrder.Tax) - 1,

                CardOwnerNationalID = ecwidOrder.CustomerTaxId,

                DealDetails = new DealDetails
                {
                    ConsumerAddress = new Address
                    {
                        City = ecwidOrder.BillingPerson.City,
                        CountryCode = ecwidOrder.BillingPerson.CountryCode,
                        Street = ecwidOrder.BillingPerson.Street,
                        Zip = ecwidOrder.BillingPerson.PostalCode
                    },
                    ConsumerPhone = ecwidOrder.BillingPerson.Phone,
                    ConsumerEmail = ecwidOrder.Email,
                    Items = MapEcwidItemsToECNGItems(ecwidOrder.Items),
                }
            };

            return transaction;
        }

        /// <summary>
        /// Maps ecwid request to create consumer request. May be used in case when customer does not exist in ECNG.
        /// </summary>
        /// <param name="ecwidOrder">Ecwid data</param>
        /// <returns>Consumer request</returns>
        public ConsumerRequest GetConsumerRequest(EcwidOrder ecwidOrder)
        {
            var request = new ConsumerRequest
            {
                ConsumerPhone = ecwidOrder.BillingPerson.Phone,
                ConsumerEmail = ecwidOrder.Email,
                ConsumerName = ecwidOrder.BillingPerson.Name,
                ConsumerNationalID = ecwidOrder.CustomerTaxId,
                ExternalReference = ecwidOrder.CustomerId,
                Origin = CUSTOMER_ORIGIN,
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

        private IEnumerable<Item> MapEcwidItemsToECNGItems(IEnumerable<EcwidOrderItem> source)
        {
            return source.Select(e => new Item 
            {
                ExternalReference = e.ProductId.ToString(),
                Price = e.Price,
                Quantity = e.Quantity,
                ItemName = e.Name
            });
        }
    }
}
