import Vue from 'vue'
import VueRouter from 'vue-router'
import MainLayout from '../layouts/main/Index.vue'
import store from '../store/index';
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
        name: 'Terminals',
        path: 'terminals/list',
        component: () => import('../pages/terminals/TerminalsList.vue'),
      },
      {
        name: 'EditTerminal',
        path: 'terminals/edit/:id',
        component: () => import('../pages/terminals/EditTerminal.vue'),
      },
      {
        name: 'TerminalTemplates',
        path: 'terminal-templates/list',
        component: () => import('../pages/terminal-templates/TerminalTemplatesList.vue'),
      },
      {
        name: 'EditTerminalTemplate',
        path: 'terminal-templates/edit/:id',
        component: () => import('../pages/terminal-templates/EditTerminalTemplate.vue'),
      },
      {
        name: 'CreateMerchant',
        path: 'merchants/create',
        component: () => import('../pages/merchants/CreateMerchant.vue'),
      },
      {
        name: 'EditMerchant',
        path: 'merchants/edit/:id',
        component: () => import('../pages/merchants/EditMerchant.vue'),
      },
      {
        name: 'Merchants',
        path: 'merchants/list',
        component: () => import('../pages/merchants/MerchantsList.vue'),
      },
      {
        name: 'Users',
        path: 'users/list',
        component: () => import('../pages/users/UsersList.vue'),
      },
      {
        name: 'Merchant',
        path: 'merchants/view/:id',
        component: () => import('../pages/merchants/MerchantInfo.vue'),
      },
      {
        name: 'Transaction',
        path: 'transactions/view/:id',
        component: () => import('../pages/transactions/TransactionInfo.vue'),
      },
      {
        name: 'Transactions',
        path: 'transactions/list',
        component: () => import('../pages/transactions/TransactionsList.vue'),
      },
      {
        name: 'SystemLogs',
        path: 'system-logs/list',
        component: () => import('../pages/system-logs/SystemLogsList.vue'),
      },
      {
        name: 'Audits',
        path: 'audit/list',
        props: true,
        component: () => import('../pages/audit/AuditList.vue'),
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
      if(await oidc.isAdmin()){
        next();
      }else{
        oidc.signOut();
      }
  } else {
      oidc.signinRedirect(to)
  }
});


export default router
