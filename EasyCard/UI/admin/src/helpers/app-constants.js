const appConstants = {
    invoicing:{
        defaultInvoiceType: 'invoiceWithPaymentInfo',
        defaultRefundInvoiceType: 'refundInvoice',
        defaultCreditInvoiceType: 'creditNote',
    },
    misc: {
        uiDefaultVersion: "#{versionNumber}#"
    },
    users: {
        roles: {
            merchant: "Merchant",
            manager: "Manager"
        }
    }
};

export default appConstants;