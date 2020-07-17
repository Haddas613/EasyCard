import Vue from 'vue'
import VueRouter from 'vue-router'
import MainLayout from '../layouts/main/Index.vue'
import WizardLayout from '../layouts/wizard/Index.vue'
import mainAuth from '../auth';

Vue.use(VueRouter)

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
        alias: '',
        component: () => import('../pages/Dashboard.vue'),
      },
      {
        name: 'Transactions',
        path: 'transactions/list',
        component: () => import('../pages/transactions/TransactionsList.vue'),
      },
      {
        name: 'Items',
        path: 'items/list',
        component: () => import('../pages/items/ItemsList.vue'),
      },
      {
        name: 'Create Item',
        path: 'items/create',
        component: () => import('../pages/items/CreateItem.vue'),
      },
      {
        name: 'Edit Item',
        path: 'items/edit/:id',
        component: () => import('../pages/items/EditItem.vue'),
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

export default router
