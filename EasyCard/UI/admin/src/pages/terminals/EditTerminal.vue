<template>
  <v-card flat color="ecbg">
    <v-tabs grow color="primary" v-model="tab">
      <v-tab key="settings">{{$t("TerminalSettings")}}</v-tab>
      <v-tab key="integrations">{{$t("Integrations")}}</v-tab>
    </v-tabs>
    <v-card-text>
      <v-tabs-items v-model="tab" class="bg-ecbg">
        <v-tab-item key="settings" class="pt-2">
          <v-form ref="terminalSettingsForm" v-model="terminalSettingsFormValid" lazy-validation>
            <terminal-settings-fields
              v-if="terminal"
              :data="terminal"
              class="pt-1"
              ref="terminalSettingsRef"
              @update="refreshTerminal()"
            ></terminal-settings-fields>
            <v-flex class="d-flex justify-end">
              <v-btn
                color="primary"
                :disabled="!terminalSettingsFormValid"
                :block="$vuetify.breakpoint.smAndDown"
                @click="saveTerminalSettings()"
              >{{$t('Save')}}</v-btn>
            </v-flex>
          </v-form>
        </v-tab-item>
        <v-tab-item key="integrations">
          <terminal-integrations-form :terminal="terminal" v-if="terminal"></terminal-integrations-form>
        </v-tab-item>
      </v-tabs-items>
    </v-card-text>
  </v-card>
</template>

<script>
export default {
  components: {
    TerminalSettingsFields: () =>
      import("../../components/terminals/TerminalSettingsFields"),
    TerminalIntegrationsForm: () =>
      import("../../components/terminals/TerminalIntegrationsForm")
  },
  data() {
    return {
      terminal: null,
      terminalSettingsFormValid: true,
      tab: "settings"
    };
  },
  async mounted() {
    let terminal = await this.$api.terminals.getTerminal(this.$route.params.id);
    if (!terminal) {
      return this.$router.push({ name: "Terminals" });
    }

    this.terminal = terminal;

    this.$store.commit("ui/changeHeader", {
      value: {
        threeDotMenu: [
          {
            text: this.$t("SeeHistory"),
            fn: () => {
              this.$router.push({
                name: "Audits",
                params: {
                  filters: {
                    terminalID: this.$route.params.id
                  }
                }
              });
            }
          }
        ]
      }
    });
  },
  methods: {
    async saveTerminalSettings() {
      if (!this.terminalSettingsFormValid) {
        return;
      }
      let data = this.$refs.terminalSettingsRef.getData();
      let operaionResult = await this.$api.terminals.updateTerminal(data);
      if (operaionResult.status === "success") {
        return this.$router.push({ name: "Terminals" });
      }
    },
    async refreshTerminal() {
      await this.$store.dispatch("settings/refreshTerminal", {
        api: this.$api
      });
    }
  }
};
</script>
<style lang="scss" scoped>
  .bg-ecbg{
    background-color: var(--v-ecbg-base) !important;
  }
</style>