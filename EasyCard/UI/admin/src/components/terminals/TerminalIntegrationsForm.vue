<template>
  <div v-if="integrations">
    <v-card v-for="int in integrations" :key="int.externalSystemID" no-gutters class="mb-4">
      <v-card-title class="subtitle-2">{{int.name}}</v-card-title>
      <v-divider></v-divider>
      <v-card-text>
          <component v-bind:is="getIntegrationComponentName(int.key)" v-bind="{data: int, terminalId: terminal.terminalID}"></component>
      </v-card-text>
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
      integrations: null,
    };
  },
  async mounted() {
    let integrations = (
      await this.$api.terminals.getAvailableIntegrations()
    ).data;

    if(this.terminal.integrations && this.terminal.integrations.length > 0){
      for(var int of integrations){
        let terminalInt = this.lodash.find(this.terminal.integrations, i => i.externalSystem.externalSystemID == int.externalSystemID);
        int.settings = terminalInt ? terminalInt.settings : int.settings;
      }
    }
    this.integrations = integrations;
  },
  methods: {
    mergeIntegration() {
      
    },
    getIntegrationComponentName(key){
      return `${key}-settings-form`
    }
  },
};
</script>