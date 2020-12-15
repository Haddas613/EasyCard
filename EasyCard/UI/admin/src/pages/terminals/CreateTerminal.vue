<template>
  <v-card class="mt-2" flat color="ecbg">
    <!-- <v-card-title class="subtitle-1">{{$t("EditTerminal")}}</v-card-title>
    <v-divider></v-divider>-->
    <v-card-text>
      TODO: Implement or remove
      <v-form ref="terminalSettingsForm" v-model="terminalSettingsFormValid" lazy-validation>
        <terminal-settings-form
          v-if="terminal"
          :data="terminal"
          class="pt-1"
          ref="terminalSettingsRef"
          @update="refreshTerminal()"
        ></terminal-settings-form>
        <v-flex class="d-flex justify-end">
          <v-btn
            color="primary"
            :disabled="!terminalSettingsFormValid"
            :block="$vuetify.breakpoint.smAndDown"
            @click="saveTerminalSettings()"
          >{{$t('Save')}}</v-btn>
        </v-flex>
      </v-form>
    </v-card-text>
  </v-card>
</template>

<script>
export default {
  components: {
    TerminalSettingsForm: () =>
      import("../../components/terminals/TerminalSettingsForm")
  },
  data() {
    return {
      terminal: null,
      merchantID: null,
      terminalSettingsFormValid: true
    };
  },
  async mounted() {
    // let terminal = await this.$api.terminals.getTerminal(this.$route.params.id);
    // if (!terminal) {
    //   return this.$router.push({ name: "Terminals" });
    // }

    // this.terminal = terminal;
  },
  methods: {
    async saveTerminalSettings() {
      if (!this.terminalSettingsFormValid) {
        return;
      }
      let data = this.$refs.terminalSettingsRef.getData();
      let operaionResult = await this.$api.terminals.updateTerminal(data);
      if (operaionResult.status === "success") {
        if(this.$route.params.merchantID){
            return this.$router.push({ name: "Merchant", params: { id: result.entityReference }});
        }else{
            return this.$router.push({ name: "Terminals" });
        }
      }
    }
  }
};
</script>