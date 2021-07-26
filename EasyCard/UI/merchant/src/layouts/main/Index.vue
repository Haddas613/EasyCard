<template>
  <div>

    <intercom></intercom>

    <app-version-overlay></app-version-overlay>

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
      AppVersionOverlay: () => import("../../components/misc/AppVersionOverlay"),
      Intercom: () => import("../../components/misc/Intercom")
    },

    data: () => ({
      dialog: false,
      drawer: false
    }),
  }
</script>
