import Vue from 'vue'
import VueRouter from 'vue-router'
import MainLayout from '../layouts/main/Index.vue'

Vue.use(VueRouter)

  const routes = [
  {
    path: '/',
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
  }
]

const router = new VueRouter({
  mode: 'history',
  base: process.env.BASE_URL,
  routes
})

export default router
