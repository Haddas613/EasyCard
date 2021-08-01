const appConstants = {
    invoicing: {
        defaultInvoiceType: 'invoiceWithPaymentInfo',
        defaultRefundInvoiceType: 'refundInvoice',
        defaultCreditInvoiceType: 'creditNote',
        types: {
            creditNote: 'creditNote',
            invoice: 'invoice',
            invoiceWithPaymentInfo: 'invoiceWithPaymentInfo',
            paymentInfo: 'paymentInfo',
            refundInvoice: 'refundInvoice'
        }
    },
    filtering: {
        defaultDashboardQuickDateType: 'today',
        defaultDashboardAltQuickDateType: 'lastMonth',
        defaultDashboardAltGranularity: 'week',
        defaultDashboardAltQuickDateFilterAltEnum: 'noComparison',
    },
    misc: {
        uiDefaultVersion: '#{versionNumber}#'
    },
    users: {
        roles: {
            merchant: 'Merchant',
            manager: 'Manager',
            billingAdmin: 'BillingAdministrator',
            businessAdmin: 'BusinessAdministrator',
        }
    },
    terminal: {
        features: {
            PreventDoubleTansactions: 'PreventDoubleTansactions',
            RecurrentPayments: 'RecurrentPayments',
            SmsNotification: 'SmsNotification',
            Checkout: 'Checkout',
            Api: 'Api',
            Billing: 'Billing'
        },
        integrations: {
            processor: "processor",
            aggregator: "aggregator",
            invoicing: "invoicing",
            marketer: "marketer",
            pinpadProcessor: "pinpadProcessor",
        }
    },
    ui: {
        charge: {
            steps: () => ({
                1: {
                    title: "Amount",
                    canChangeTerminal: true,
                    showItemsCount: true,
                },
                2: {
                    title: "Basket",
                },
                3: {
                    title: "ChooseCustomer",
                    skippable: true,
                },
                4: {
                    title: "PaymentInfo",
                },
                5: {
                    title: "AdditionalSettings",
                },
                //Last step may be dynamically altered to represent error if transaction creation has failed.
                6: {
                    title: "Success",
                    completed: true,
                }
            })
        }
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
    }
};

export default appConstants;