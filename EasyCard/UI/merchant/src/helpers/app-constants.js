const appConstants = {
    invoicing:{
        defaultInvoiceType: 'invoiceWithPaymentInfo',
        defaultRefundInvoiceType: 'refundInvoice',
        defaultCreditInvoiceType: 'creditNote',
    },
    filtering:{
        defaultDashboardQuickDateType: 'today',
        defaultDashboardAltQuickDateType: 'lastMonth',
        defaultDashboardAltGranularity: 'week',
        defaultDashboardAltQuickDateFilterAltEnum: 'noComparison',
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