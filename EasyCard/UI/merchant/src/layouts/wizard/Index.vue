<template>
  <v-app id="inspire" v-bind:dir="$vuetify.rtl ? 'rtl' : 'ltr'">
    <app-version-overlay></app-version-overlay>
    <wizard-content />
  </v-app>
</template>

<script>

export default {
  name: "WizardIndex",

  components: {
    WizardContent: () => import("./Content.vue"),
    AppVersionOverlay: () => import("../../components/misc/AppVersionOverlay"),
  },

  data: () => ({
    dialog: false,
    drawer: false
  })
};
</script>
