<template>
  <v-app id="inspire" v-bind:dir="$vuetify.rtl ? 'rtl' : 'ltr'">
    <wizzard-app-bar :drawer.sync="drawer" />

    <ec-nav :drawer.sync="drawer" />
    <wizard-content />
  </v-app>
</template>

<script>
import mainAuth from "../../auth";

export default {
  name: "WizardIndex",

  components: {
    WizardContent: () => import("./Content.vue"),
    WizzardAppBar: () => import("./AppBar.vue"),
    EcNav: () => import("../shared/EcNav.vue")
  },

  data: () => ({
    dialog: false,
    drawer: false
  })
};
</script>
