<template>
  <div>
    <wizard-content />
  </div>
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
