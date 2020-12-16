<template>
  <div v-if="integrations">
    <v-card v-for="int in integrations" :key="int.externalSystemID" no-gutters class="mb-4">
      <v-card-title class="subtitle-2">{{int.name}}</v-card-title>
      <v-divider></v-divider>
      <v-card-text>to be done</v-card-text>
    </v-card>
  </div>
</template>

<script>
export default {
  props: {
    terminal: {
      type: Object,
      required: true
    }
  },
  data() {
    return {
      integrations: null
    };
  },
  async mounted() {
    this.integrations = (
      await this.$api.terminals.getAvailableIntegrations()
    ).data;
  }
};
</script>

<style lang="scss" scoped>
</style>