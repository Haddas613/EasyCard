using System;
using System.Collections.Generic;
using System.Text;

namespace EasyInvoice.Models
{
    public class UpdateUserDetailsRequest
    {
        public EasyInvoiceTerminalSettings Terminal { get; set; }
        public string email { get; set; }

        public string password { get; set; }

        public string name { get; set; }

        public string taxId { get; set; }

        public string street { get; set; }

        public string streetNumber { get; set; }

        public string city { get; set; }

        public string postalCode { get; set; }

        public string country { get; set; }

        public string countryCode { get; set; }

        public string phoneNumber { get; set; }

        public List<ECInvoiceHashConfigurationItem> hashExportConfiguration { get; set; }

        public string generalClientCode { get; set; }

        public string incomeCode { get; set; }

        //  public string TaxAccount { get; set; }
        public UpdateUserDetailsRequest()
        {
            hashExportConfiguration = new List<ECInvoiceHashConfigurationItem>();
            ECInvoiceHashConfigurationItem itemCash = new ECInvoiceHashConfigurationItem();
            itemCash.key = "CASH";
            hashExportConfiguration.Add(itemCash);

            ECInvoiceHashConfigurationItem itemCHEQUE = new ECInvoiceHashConfigurationItem();
            itemCash.key = "CHEQUE";
            hashExportConfiguration.Add(itemCHEQUE);

            ECInvoiceHashConfigurationItem itemCREDIT_CARD_VISA = new ECInvoiceHashConfigurationItem();
            itemCash.key = "CREDIT_CARD_VISA";
            hashExportConfiguration.Add(itemCREDIT_CARD_VISA);

            ECInvoiceHashConfigurationItem itemCREDIT_CARD_MASTER_CARD = new ECInvoiceHashConfigurationItem();
            itemCash.key = "CREDIT_CARD_MASTER_CARD";
            hashExportConfiguration.Add(itemCREDIT_CARD_MASTER_CARD);

            ECInvoiceHashConfigurationItem itemCREDIT_CARD_AMEX = new ECInvoiceHashConfigurationItem();
            itemCash.key = "CREDIT_CARD_AMEX";
            hashExportConfiguration.Add(itemCREDIT_CARD_AMEX);

            ECInvoiceHashConfigurationItem itemCREDIT_CARD_DISCOVER = new ECInvoiceHashConfigurationItem();
            itemCash.key = "CREDIT_CARD_DISCOVER";
            hashExportConfiguration.Add(itemCREDIT_CARD_DISCOVER);

            ECInvoiceHashConfigurationItem itemCREDIT_CARD_JBC = new ECInvoiceHashConfigurationItem();
            itemCash.key = "CREDIT_CARD_JBC";
            hashExportConfiguration.Add(itemCREDIT_CARD_JBC);

            ECInvoiceHashConfigurationItem itemCREDIT_CARD_DINERS_CLUB = new ECInvoiceHashConfigurationItem();
            itemCash.key = "CREDIT_CARD_DINERS_CLUB";
            hashExportConfiguration.Add(itemCREDIT_CARD_DINERS_CLUB);

            ECInvoiceHashConfigurationItem itemCREDIT_CARD_ISRA_CARD = new ECInvoiceHashConfigurationItem();
            itemCash.key = "CREDIT_CARD_ISRA_CARD";
            hashExportConfiguration.Add(itemCREDIT_CARD_ISRA_CARD);

            ECInvoiceHashConfigurationItem itemCREDIT_CARD_OTHER = new ECInvoiceHashConfigurationItem();
            itemCash.key = "CREDIT_CARD_OTHER";
            hashExportConfiguration.Add(itemCREDIT_CARD_OTHER);

            ECInvoiceHashConfigurationItem itemBANK_TRANSFER = new ECInvoiceHashConfigurationItem();
            itemCash.key = "BANK_TRANSFER";
            hashExportConfiguration.Add(itemBANK_TRANSFER);

            ECInvoiceHashConfigurationItem itemNONE = new ECInvoiceHashConfigurationItem();
            itemCash.key = "NONE";
            hashExportConfiguration.Add(itemNONE);
        }
    }
}
