<template>
  <div>

    <intercom></intercom>

    <main-app-bar :drawer.sync="drawer"/>

    <ec-nav :drawer.sync="drawer"/>

    <main-content :drawer.sync="drawer" />

  </div>
</template>

<script>
  export default {
    name: 'MainIndex',

    components: {
      MainAppBar: () => import('./AppBar.vue'),
      EcNav: () => import('../shared/EcNav.vue'),
      MainContent: () => import('./Content.vue'),
      MainFooter: () => import('./Footer.vue'),
      Intercom: () => import("../../components/misc/Intercom"),
    },

    data: () => ({
      dialog: false,
      drawer: false
    }),

    mounted () {
      this.drawer = this.$vuetify.breakpoint.mdAndUp;
    },
  }
</script>
