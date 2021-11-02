import Vue from 'vue'
import VueRouter from 'vue-router'
import MainLayout from '../layouts/main/Index.vue'
import WizardLayout from '../layouts/wizard/Index.vue'
import store from '../store/index';
import cfg from "../app.config";
import appConstants from "../helpers/app-constants";

Vue.use(VueRouter)

/**
 * @param {roles} roles array: string | string 
 */
const AllowedForGuard = function(roles, defaultRoute = { name: "Transactions" }){
    this.defaultRoute = defaultRoute;
    this.challenge = async function(oidc){
        if(await oidc.isInRole(roles)){
            return true;
        }
        
        return false;
    }
};

const allowedForManagerOrAdminGuard = new AllowedForGuard([appConstants.users.roles.manager, appConstants.users.roles.billingAdmin]);

/**
 * MainLayout components may contain next meta:
 * 1. altDesign  - do not display title and switch color to desktop version
 * 
 * 2. backBtn:  string:(route-name) | boolean, - when present with positive value, the back button will appear on mobile 
 * layout instead of burger menu. Either route or goBack(-1) will be used if route name is supplied or not respectively.
 * 
 * 3. closeBtn: string:(route-name) | boolean - when present with positive value, the close button will appear on mobile 
 * layout instead of burger menu. Either route or goBack(-1) will be used if route name is supplied or not respectively.
 */
const routes = [
    {
        path: '/wizard',
        component: WizardLayout,
        meta: {
            //authName: mainAuth.authName
        },
        children: [{
                name: 'Charge',
                path: 'transactions/charge',
                component: () =>
                    import ('../wizards/transactions/CreateCharge.vue'),
                props: true
            },
            {
                name: 'QuickCharge',
                path: 'transactions/quickcharge',
                component: () =>
                    import ('../wizards/transactions/QuickCharge.vue'),
                props: true
            },
            {
                name: 'Refund',
                path: 'transactions/refund',
                component: () =>
                    import ('../wizards/transactions/CreateRefund.vue'),
                props: true
            },
            {
                name: 'CreateInvoice',
                path: 'invoicing/create',
                component: () =>
                    import ('../wizards/invoicing/CreateInvoice.vue'),
                props: true
            },
            {
                name: 'CreatePaymentRequest',
                path: 'payment-requests/create',
                component: () =>
                    import ('../wizards/payment-requests/CreatePaymentRequest.vue'),
                props: true
            }
        ]
    },
    {
        path: '/',
        component: MainLayout,
        meta: {
            //authName: mainAuth.authName
        },
        children: [{
                name: 'Dashboard',
                path: 'dashboard',
                meta: {
                    altDisplay: true,
                    guard: allowedForManagerOrAdminGuard
                },
                alias: '',
                component: () =>
                    import ('../pages/Dashboard.vue'),
            },
            {
                name: 'Transactions',
                path: 'transactions/list',
                component: () =>
                    import ('../pages/transactions/TransactionsList.vue'),
            },
            {
                name: 'Transmissions',
                path: 'transmissions/list',
                component: () =>
                    import ('../pages/transmissions/TransmissionsList.vue'),
            },
            {
                name: 'Transaction',
                path: 'transactions/view/:id',
                meta: {
                    backBtn: true
                },
                component: () =>
                    import ('../pages/transactions/TransactionInfo.vue'),
            },
            {
                name: 'TransactionsFiltered',
                path: 'transactions/filter/',
                meta: {
                    backBtn: 'Transactions'
                },
                props: true,
                component: () =>
                    import ('../pages/transactions/TransactionsFiltered.vue'),
            },
            {
                name: 'Items',
                path: 'items/list',
                component: () =>
                    import ('../pages/items/ItemsList.vue'),
            },
            {
                name: 'CreateItem',
                path: 'items/create',
                meta: {
                    closeBtn: 'Items'
                },
                component: () =>
                    import ('../pages/items/CreateItem.vue'),
            },
            {
                name: 'EditItem',
                path: 'items/edit/:id',
                meta: {
                    closeBtn: true
                },
                component: () =>
                    import ('../pages/items/EditItem.vue'),
            },
            {
                name: 'Item',
                path: 'items/view/:id',
                meta: {
                    backBtn: true
                },
                component: () =>
                    import ('../pages/items/ItemInfo.vue'),
            },
            {
                name: 'Customers',
                path: 'customers/list',
                component: () =>
                    import ('../pages/customers/CustomersList.vue'),
            },
            {
                name: 'CreateCustomer',
                path: 'customers/create',
                meta: {
                    closeBtn: 'Customers'
                },
                component: () =>
                    import ('../pages/customers/CreateCustomer.vue'),
            },
            {
                name: 'EditCustomer',
                path: 'customers/edit/:id',
                meta: {
                    backBtn: true
                },
                component: () =>
                    import ('../pages/customers/EditCustomer.vue'),
            },
            {
                name: 'Customer',
                path: 'customers/view/:id',
                meta: {
                    backBtn: true
                },
                component: () =>
                    import ('../pages/customers/CustomerInfo.vue'),
            },
            {
                name: 'CreateCardToken',
                path: 'ctokens/:customerid/create',
                meta: {
                    closeBtn: true
                },
                component: () =>
                    import ('../pages/ctokens/CreateCardToken.vue'),
            },
            {
                name: 'EditCardToken',
                path: 'ctokens/:id/edit',
                meta: {
                    backBtn: true
                },
                component: () =>
                    import ('../pages/ctokens/EditCardToken.vue'),
            },
            {
                name: 'BillingDeals',
                path: 'billing-deals/list',
                props: true,
                meta: {
                    guard: allowedForManagerOrAdminGuard
                },
                component: () =>
                    import ('../pages/billing-deals/BillingDealsList.vue'),
            },
            {
                name: 'FutureBillingDeals',
                path: 'future-billing-deals/list',
                props: true,
                meta: {
                    guard: allowedForManagerOrAdminGuard
                },
                component: () =>
                    import ('../pages/future-billing-deals/FutureBillingDealsList.vue'),
            },
            {
                name: 'BillingDeal',
                meta: {
                    backBtn: true,
                    guard: allowedForManagerOrAdminGuard
                },
                path: 'billing-deals/view/:id',
                component: () =>
                    import ('../pages/billing-deals/BillingDealInfo.vue'),
            },
            {
                name: 'EditBillingDeal',
                meta: {
                    backBtn: 'BillingDeals',
                    guard: allowedForManagerOrAdminGuard
                },
                path: 'billing-deals/edit/:id',
                component: () =>
                    import ('../pages/billing-deals/EditBillingDeal.vue'),
            },
            {
                name: 'CreateBillingDeal',
                props: true,
                path: 'billing-deals/create',
                meta: {
                    guard: allowedForManagerOrAdminGuard
                },
                component: () =>
                    import ('../pages/billing-deals/CreateBillingDeal.vue'),
            },
            {
                name: 'Invoices',
                path: 'invoicing/list',
                props: true,
                component: () =>
                    import ('../pages/invoicing/InvoicesList.vue'),
            },
            {
                name: 'Invoice',
                meta: {
                    backBtn: 'Invoices'
                },
                path: 'invoicing/view/:id',
                component: () =>
                    import ('../pages/invoicing/InvoiceInfo.vue'),
            },
            {
                name: 'PaymentRequests',
                path: 'payment-requests/list',
                props: true,
                component: () =>
                    import ('../pages/payment-requests/PaymentRequestsList.vue'),
            },
            {
                name: 'PaymentRequest',
                meta: {
                    backBtn: 'PaymentRequests'
                },
                path: 'payment-requests/view/:id',
                component: () =>
                    import ('../pages/payment-requests/PaymentRequestInfo.vue'),
            },
            {
                name: 'MyProfile',
                meta: {
                    backBtn: 'Dashboard'
                },
                path: 'profile',
                component: () =>
                    import ('../pages/profile/Profile.vue'),
            },
            {
                name: 'MasavFiles',
                path: 'masav-files/list',
                props: true,
                meta: {
                    backBtn: 'Dashboard'
                },
                component: () =>
                    import ('../pages/masav/MasavFilesList.vue'),
            },
            {
                name: 'MasavFileRows',
                path: 'masav-file/:id(\\d+)/rows',
                meta: {
                    backBtn: 'MasavFiles'
                },
                props: true,
                component: () =>
                    import ('../pages/masav/MasavFileRowsList.vue'),
            },
            {
                name: '404',
                path: '*',
                component: () =>
                    import ('../views/NotFound.vue'),
            }
        ]
    },
    {
        path: '*',
        component: () =>
            import ('../views/NotFound.vue'),
    },
]

const router = new VueRouter({
    mode: 'history',
    base: cfg.BASE_URL,
    routes
})

router.afterEach((to, from) => {
    store.commit("ui/changeHeader", {
        value: {
            text: {
                translate: true,
                value: to.name,
            },
            threeDotMenu: null,
            altDisplay: to.meta.altDisplay,
            navBtn: null
        }
    });
});



// auth guard added to any route
router.beforeEach(async(to, from, next) => {
    const oidc = Vue.prototype.$oidc;
    if (await oidc.isAuthenticated()) {
        if(to.meta && to.meta.guard){
            let challenge = await to.meta.guard.challenge(oidc);
            if(challenge){
                next();
            }else{
                next(to.meta.guard.defaultRoute);
            }
        }else{
            // we can pass through merchants and admins
            next();
        }
          
    } else {
        oidc.signinRedirect(to)
    }
});

export default router