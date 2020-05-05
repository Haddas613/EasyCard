<template>
  <v-app id="inspire" v-bind:dir="$vuetify.rtl ? 'rtl' : 'ltr'">

    <main-app-bar :drawer.sync="drawer"/>

    <main-nav :items="items" :drawer.sync="drawer"/>

    <main-content />

    <main-footer />
  </v-app>
</template>

<script>
  export default {
    name: 'MainIndex',

    components: {
      MainAppBar: () => import('./AppBar.vue'),
      MainNav: () => import('./Nav.vue'),
      MainContent: () => import('./Content.vue'),
      MainFooter: () => import('./Footer.vue'),
    },

    methods: {
      changeRTL (to) {
        this.$vuetify.rtl = to;
      },
    },

    data: () => ({
      dialog: false,
      drawer: false,
      items: [
        { icon: 'mdi-view-dashboard', text: 'Dashboard', to: '/' },
        {
          icon: 'mdi-cash-minus',
          'icon-alt': 'mdi-cash-plus',
          text: 'Transactions',
          children: [
            { icon: 'mdi-cash-register', text: 'Create Transaction', to: '/transactions/create' },
            { icon: 'mdi-cash-multiple', text: 'Transactions List', to: '/transactions' },
          ],
        }
      ],
    }),
  }
</script>
