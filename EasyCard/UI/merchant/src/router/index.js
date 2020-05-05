import Vue from 'vue'
import VueRouter from 'vue-router'
import MainLayout from '../layouts/main/Index.vue'

Vue.use(VueRouter)

  const routes = [
  {
    path: '/',
    name: 'Home',
    component: MainLayout,
    children: [
      {
        name: 'Dashboard',
        path: '',
        component: () => import('../pages/Dashboard.vue'),
      },
      {
        name: 'Transactions/Create',
        path: 'transactions/create',
        component: () => import('../pages/transactions/CreateTransaction.vue'),
      },
      {
        name: 'Transactions/List',
        path: 'transactions/list',
        component: () => import('../pages/transactions/TransactionsList.vue'),
      }
    ]
  },
  {
    path: '/about',
    name: 'About',
    // route level code-splitting
    // this generates a separate chunk (about.[hash].js) for this route
    // which is lazy-loaded when the route is visited.
    component: function () {
      return import(/* webpackChunkName: "about" */ '../views/About.vue')
    }
  }
]

const router = new VueRouter({
  mode: 'history',
  base: process.env.BASE_URL,
  routes
})

export default router
