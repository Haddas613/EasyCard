import Vue from 'vue'
import VueRouter from 'vue-router'
import MainLayout from '../layouts/main/Index.vue'
Vue.use(VueRouter)

const routes = [
  {
    path: '/',
    name: 'Entry',
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
    children: [
      {
        name: 'Dashboard',
        path: 'dashboard',
        alias: '',
        component: () => import('../pages/Dashboard.vue'),
      },
      {
        name: 'Terminals/Create',
        path: 'terminals/create',
        component: () => import('../pages/terminals/CreateTerminal.vue'),
      },
      {
        name: 'Terminals/List',
        path: 'terminals/list',
        component: () => import('../pages/terminals/TerminalsList.vue'),
      },
      {
        name: 'Merchants/Create',
        path: 'merchants/create',
        component: () => import('../pages/merchants/CreateMerchant.vue'),
      },
      {
        name: 'Merchants/List',
        path: 'merchants/list',
        component: () => import('../pages/merchants/MerchantsList.vue'),
      },
      {
        name: 'Transactions/List',
        path: 'transactions/list',
        component: () => import('../pages/transactions/TransactionsList.vue'),
      },
      {
        name: 'MyProfile',
        path: 'profile',
        component: () =>
            import ('../pages/profile/Profile.vue'),
      },
      {
        name: '404',
        path: '*',
        component: () => import('../views/NotFound.vue'),
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


// auth guard added to any route
router.beforeEach(async(to, from, next) => {
  const oidc = Vue.prototype.$oidc;
  if (await oidc.isAuthenticated()) {
      next()
  } else {
      oidc.signinRedirect(to)
  }
});


export default router
