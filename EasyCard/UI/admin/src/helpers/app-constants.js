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
            Api: 'Api',
            Billing: 'Billing'
        },
        api: {
            terminals: 'terminals',
            terminalTemplates: 'terminalTemplates',
        },
    },
    config:{
        ui: {
            typeaheadTimeout: 500,
            defaultTake: process.env.VUE_APP_UI_LIST_DEFAULT_TAKE_COUNT,
        },
    },
    transaction: {
        types: {
            credit: "credit",
            immediate: "immediate",
            installments: "installments",
            regularDeal: "regularDeal"
        },
        paymentTypes: {
            bank: "bank",
            card: "card",
            cash: "cash",
            cheque: "cheque",
        }
    },
    filtering: {
        defaultDashboardQuickDateType: 'today',
        defaultDashboardAltQuickDateType: 'lastMonth',
        defaultDashboardAltGranularity: 'week',
        defaultDashboardAltQuickDateFilterAltEnum: 'noComparison',
    },
};

export default appConstants;