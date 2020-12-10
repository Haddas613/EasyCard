<template>
  <v-app id="inspire" v-bind:dir="$vuetify.rtl ? 'rtl' : 'ltr'">

    <main-app-bar :drawer.sync="drawer"/>

    <ec-nav :drawer.sync="drawer"/>

    <main-content :drawer.sync="drawer" />

  </v-app>
</template>

<script>
  export default {
    name: 'MainIndex',

    components: {
      MainAppBar: () => import('./AppBar.vue'),
      EcNav: () => import('../shared/EcNav.vue'),
      MainContent: () => import('./Content.vue'),
      MainFooter: () => import('./Footer.vue'),
    },

    data: () => ({
      dialog: false,
      drawer: false
    }),
  }
</script>
