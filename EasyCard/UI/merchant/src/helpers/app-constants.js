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
    }
};

export default appConstants;