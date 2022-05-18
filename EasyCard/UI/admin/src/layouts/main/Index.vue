<template>
  <v-app id="inspire" v-bind:dir="$vuetify.rtl ? 'rtl' : 'ltr'">

    <app-version-overlay></app-version-overlay>

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
      AppVersionOverlay: () => import("../../components/misc/AppVersionOverlay"),
    },

    data: () => ({
      dialog: false,
      drawer: false,
    }),
    mounted () {
      this.drawer = this.$vuetify.breakpoint.mdAndUp;
    },
  }
</script>
