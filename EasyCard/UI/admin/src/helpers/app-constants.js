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
    },
    terminal:{
        features: {
            PreventDoubleTansactions: 'PreventDoubleTansactions',
            RecurrentPayments: 'RecurrentPayments',
            SmsNotification: 'SmsNotification',
            Checkout: 'Checkout',
            Api: 'Api'
        }
    }
};

export default appConstants;