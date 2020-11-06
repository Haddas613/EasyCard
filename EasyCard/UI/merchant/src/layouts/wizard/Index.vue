<template>
  <v-app id="inspire" v-bind:dir="$vuetify.rtl ? 'rtl' : 'ltr'">
    <wizard-content />
  </v-app>
</template>

<script>

export default {
  name: "WizardIndex",

  components: {
    WizardContent: () => import("./Content.vue"),
  },

  data: () => ({
    dialog: false,
    drawer: false
  })
};
</script>
