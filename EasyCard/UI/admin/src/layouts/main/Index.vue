<template>
  <v-app id="inspire" v-bind:dir="$vuetify.rtl ? 'rtl' : 'ltr'">

    <main-app-bar :drawer.sync="drawer"/>

    <main-nav :items="items" :drawer.sync="drawer"/>

    <main-content />

    <main-footer />
  </v-app>
</template>

<script>
  import mainAuth from '../../auth';

  export default {
    name: 'MainIndex',

    components: {
      MainAppBar: () => import('./AppBar.vue'),
      MainNav: () => import('./Nav.vue'),
      MainContent: () => import('./Content.vue'),
      MainFooter: () => import('./Footer.vue'),
    },

    data: () => ({
      dialog: false,
      drawer: false,
      items: [
        { icon: 'mdi-view-dashboard', text: 'Dashboard', to: '/admin/dashboard' },
        {
          icon: 'mdi-account-arrow-left',
          'icon-alt': 'mdi-account',
          text: 'Merchants',
          expanded: false,
          children: [
            { icon: 'mdi-account-plus', text: 'Create Merchant', to: '/admin/merchants/create' },
            { icon: 'mdi-account-group', text: 'Merchants List', to: '/admin/merchants/list' },
          ],
        },
        {
          icon: 'mdi-tab-minus',
          'icon-alt': 'mdi-tab',
          text: 'Terminals',
          expanded: false,
          children: [
            { icon: 'mdi-tab-plus', text: 'Create Terminal', to: '/admin/terminals/create' },
            { icon: 'mdi-tab', text: 'Terminals List', to: '/admin/terminals/list' },
          ],
        },
        {
          icon: 'mdi-cash-minus',
          'icon-alt': 'mdi-cash-plus',
          text: 'Transactions',
          expanded: false,
          children: [
            { icon: 'mdi-cash-multiple', text: 'Transactions List', to: '/admin/transactions/list' },
          ],
        },
        { divider: true, dividerArea: 'userAuth' },
        { icon: 'mdi-account-cog', text: 'Profile', to: '/admin/profile' },
        { icon: 'mdi-logout', text: 'Logout', fn: () => {mainAuth.signOut();} },
      ],
    }),
  }
</script>
