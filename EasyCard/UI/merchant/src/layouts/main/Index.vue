<template>
  <v-app id="inspire" v-bind:dir="$vuetify.rtl ? 'rtl' : 'ltr'">

    <main-app-bar :drawer.sync="drawer"/>

    <main-nav :items="items" :drawer.sync="drawer"/>

    <main-content />

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
          icon: 'mdi-cash-minus',
          'icon-alt': 'mdi-cash-plus',
          text: 'Transactions',
          expanded: false,
          children: [
            { icon: 'mdi-cash-multiple', text: 'TransactionsList', to: '/admin/transactions/list' },
          ],
        },
        { divider: true, dividerArea: 'userAuth' },
        { icon: 'mdi-account-cog', text: 'Profile', to: '/admin/profile' },
        { icon: 'mdi-logout', text: 'Logout', fn: () => {mainAuth.signOut();} },
      ],
    }),
  }
</script>
