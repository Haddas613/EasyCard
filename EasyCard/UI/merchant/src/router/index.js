import Vue from 'vue'
import VueRouter from 'vue-router'
import MainLayout from '../layouts/main/Index.vue'
import WizardLayout from '../layouts/wizard/Index.vue'
import mainAuth from '../auth';
import store from '../store/index';

Vue.use(VueRouter)

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
    path: '/',
    name: 'Entry',
    meta: {
      authName: mainAuth.authName
    },
    redirect: '/admin'
  },
  {
    path: '/guest',
    name: 'Guest',
    component: () => import('../pages/Guest.vue')
  },
  {
    path: '/admin',
    component: MainLayout,
    meta: {
      authName: mainAuth.authName
    },
    children: [
      {
        name: 'Dashboard',
        path: 'dashboard',
        meta:{
          altDisplay: true, 
        },
        alias: '',
        component: () => import('../pages/Dashboard.vue'),
      },
      {
        name: 'Transactions',
        path: 'transactions/list',
        component: () => import('../pages/transactions/TransactionsList.vue'),
      },
      {
        name: 'TransactionsDate',
        path: 'transactions/date/:date',
        meta:{
          backBtn: true
        },
        component: () => import('../pages/transactions/TransactionsDate.vue'),
      },
      {
        name: 'Items',
        path: 'items/list',
        component: () => import('../pages/items/ItemsList.vue'),
      },
      {
        name: 'CreateItem',
        path: 'items/create',
        meta:{
          closeBtn: 'Items'
        },
        component: () => import('../pages/items/CreateItem.vue'),
      },
      {
        name: 'EditItem',
        path: 'items/edit/:id',
        meta:{
          closeBtn: true
        },
        component: () => import('../pages/items/EditItem.vue'),
      },
      {
        name: 'Item',
        path: 'items/view/:id',
        meta:{
          backBtn: true
        },
        component: () => import('../pages/items/ItemInfo.vue'),
      },
      {
        name: 'Customers',
        path: 'customers/list',
        component: () => import('../pages/customers/CustomersList.vue'),
      },
      {
        name: 'CreateCustomer',
        path: 'customers/create',
        meta:{
          closeBtn: 'Customers'
        },
        component: () => import('../pages/customers/CreateCustomer.vue'),
      },
      {
        name: 'EditCustomer',
        path: 'customers/edit/:id',
        meta:{
          backBtn: true
        },
        component: () => import('../pages/customers/EditCustomer.vue'),
      },
      {
        name: 'Customer',
        path: 'customers/view/:id',
        meta:{
          backBtn: true
        },
        component: () => import('../pages/customers/CustomerInfo.vue'),
      },
      {
        name: '404',
        path: '*',
        component: () => import('../views/NotFound.vue'),
      }
    ]
  },
  {
    path: '/wizard',
    component: WizardLayout,
    meta: {
      authName: mainAuth.authName
    },
    children: [
      {
        name: 'Wizard/Transactions/Charge',
        path: 'transactions/charge',
        component: () => import('../wizards/transactions/CreateCharge.vue'),
      }
    ]
  },
  {
    path: '*',
    component: () => import('../views/NotFound.vue'),
  },
]

const router = new VueRouter({
  mode: 'history',
  base: process.env.BASE_URL,
  routes
})

router.beforeEach((to, from, next) => {
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
  next();
});

export default router
