<template>
  <v-content class="ecbg">
    <v-container fluid ma-0 pa-0 style="height: 100%">
      <router-view />
    </v-container>
  </v-content>
</template>
<script>
export default {
  name: "WizardContent"
};
</script>