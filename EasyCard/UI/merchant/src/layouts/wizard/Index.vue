<template>
  <div>
    <app-version-overlay></app-version-overlay>
    <wizard-content />
  </div>
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
