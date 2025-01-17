<template>
  <v-card flat color="ecbg">
    <v-tabs grow color="primary" v-model="tab">
      <v-tab key="settings">{{$t("TerminalSettings")}}</v-tab>
      <v-tab key="integrations">{{$t("Integrations")}}</v-tab>
      <v-tab key="features">{{$t("Features")}}</v-tab>
    </v-tabs>
    <v-card-text>
      <v-tabs-items v-model="tab" class="bg-ecbg">
        <v-tab-item key="settings" class="pt-2">
          <v-form ref="terminalSettingsForm" v-model="terminalSettingsFormValid" lazy-validation>
            <terminal-settings-fields
              v-if="terminalTemplate"
              :key="terminalTemplate.updated"
              :data="terminalTemplate"
              class="pt-1"
              ref="terminalSettingsRef"
              @update="refreshTerminal()"
            ></terminal-settings-fields>
            <v-alert class="pt-4 text-center" border="top" colored-border type="info" elevation="2" v-if="terminalTemplate">
              {{$t("TerminalTemplateSaveInfoMessage")}}
              <v-flex class="d-flex justify-end">
              <v-btn
                color="primary"
                :disabled="!terminalSettingsFormValid"
                :block="$vuetify.breakpoint.smAndDown"
                @click="saveTerminalSettings()"
              >{{$t('Save')}}</v-btn>
            </v-flex>
            </v-alert>
          </v-form>
        </v-tab-item>
        <v-tab-item key="integrations">
          <terminal-integrations-form :terminal="terminalTemplate" v-if="terminalTemplate" is-template></terminal-integrations-form>
        </v-tab-item>
        <v-tab-item key="features">
          <terminal-features-form :terminal="terminalTemplate" v-if="terminalTemplate" is-template @update="getTerminalTemplate()"></terminal-features-form>
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
      import("../../components/terminals/TerminalIntegrationsForm"),
    TerminalFeaturesForm: () =>
      import("../../components/terminals/TerminalFeaturesForm")
  },
  data() {
    return {
      terminalTemplate: null,
      terminalSettingsFormValid: true,
      tab: "settings"
    };
  },
  async mounted() {
    await this.getTerminalTemplate();
  },
  methods: {
    async saveTerminalSettings() {
      if (!this.terminalSettingsFormValid) {
        return;
      }
      let data = this.$refs.terminalSettingsRef.getData();
      let operaionResult = await this.$api.terminalTemplates.updateTerminalTemplate(data);
      if (operaionResult.status === "success") {
        return this.$router.push({ name: "EditTerminalTemplate", params: {id: this.$route.params.id} });
      }
    },
    async getTerminalTemplate(){
      let terminalTemplate = await this.$api.terminalTemplates.getTerminalTemplate(this.$route.params.id);
      if (!terminalTemplate) {
        return this.$router.push({ name: "TerminalTemplates" });
      }

      this.terminalTemplate = terminalTemplate;
    }
  }
};
</script>
<style lang="scss" scoped>
  .bg-ecbg{
    background-color: var(--v-ecbg-base) !important;
  }
</style>