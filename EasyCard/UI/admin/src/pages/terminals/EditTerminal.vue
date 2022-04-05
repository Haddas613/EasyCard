<template>
  <v-card flat color="ecbg">
    <v-tabs grow color="primary" v-model="tab">
      <v-tab key="settings">{{$t("TerminalSettings")}}</v-tab>
      <v-tab key="integrations">{{$t("Integrations")}}</v-tab>
      <v-tab key="features">{{$t("Features")}}</v-tab>
      <v-tab key="webhooks">{{$t("Webhooks")}}</v-tab>
    </v-tabs>
    <v-card-text :key="terminalUpdatedKey">
      <v-tabs-items v-model="tab" class="bg-ecbg">
        <v-tab-item key="settings" class="pt-2">
          <v-card class="mb-4" outlined v-if="terminal && terminal.merchant">
            <v-card-title class="pb-0 mb-0 subtitle-2 black--text">
              {{$t("MerchantInfo")}}
            </v-card-title>
            <v-card-text>
              <v-row class="body-2 black--text">
                <v-col cols="12">
                  <v-divider></v-divider>
                </v-col>
                <v-col cols="12" md="3">
                  <p class="caption ecgray--text text--darken-2">{{$t('BusinessName')}}</p>
                  <p>
                    <router-link v-if="terminal.merchant.merchantID" class="text-decoration-none" link :to="{name: 'Merchant', params: {id: terminal.merchant.merchantID}}">
                      {{terminal.merchant.businessName || terminal.merchant.merchantID}}
                    </router-link>
                  </p>
                </v-col>
                <v-col cols="12" md="3">
                  <p class="caption ecgray--text text--darken-2">{{$t('MarketingName')}}</p>
                  <p>{{terminal.merchant.marketingName}}</p>
                </v-col>
                <v-col cols="12" md="3">
                  <p class="caption ecgray--text text--darken-2">{{$t('BusinessID')}}</p>
                  <p>{{terminal.merchant.businessID}}</p>
                </v-col>
                <v-col cols="12" md="3">
                  <p class="caption ecgray--text text--darken-2">{{$t('ContactPerson')}}</p>
                  <p>{{terminal.merchant.contactPerson}}</p>
                </v-col>
              </v-row>
            </v-card-text>
          </v-card>
          <v-form ref="terminalSettingsForm" v-model="terminalSettingsFormValid">
            <terminal-settings-fields
              v-if="terminal"
              :key="terminal.updated"
              :data="terminal"
              class="pt-1"
              ref="terminalSettingsRef"
              @update="refreshTerminal()"
            ></terminal-settings-fields>
            <v-flex class="d-flex justify-end">
              <v-btn
                color="success"
                :disabled="!terminalSettingsFormValid"
                :block="$vuetify.breakpoint.smAndDown"
                @click="approveTerminal()"
                v-bind:class="{ 'mx-1': !$vuetify.breakpoint.smAndDown }"
              >{{$t('Approve')}}</v-btn>
              <v-btn
                :color="!terminalSettingsFormValid ? 'error' : 'primary'"
                :outlined="!terminalSettingsFormValid"
                :block="$vuetify.breakpoint.smAndDown"
                @click="saveTerminalSettings()"
              >{{$t('Save')}}</v-btn>
            </v-flex>
          </v-form>
        </v-tab-item>
        <v-tab-item key="integrations">
          <terminal-integrations-form :terminal="terminal" v-if="terminal"></terminal-integrations-form>
        </v-tab-item>
        <v-tab-item key="features">
          <terminal-features-form :terminal="terminal" v-if="terminal" @update="refreshTerminal()"></terminal-features-form>
        </v-tab-item>
        <v-tab-item key="webhooks">
          <terminal-webhooks-form :terminal="terminal" v-if="terminal" @save="saveWebhooks($event)"></terminal-webhooks-form>
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
      import("../../components/terminals/TerminalFeaturesForm"),
    TerminalWebhooksForm: () =>
      import("../../components/terminals/TerminalWebhooksForm")
  },
  data() {
    return {
      terminal: null,
      terminalSettingsFormValid: true,
      tab: "settings",
      terminalUpdatedKey: null,
    };
  },
  async mounted() {
    await this.refreshTerminal();

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
          },
          {
            text: this.$t("Transactions"),
            fn: () => {
              this.$router.push({
                name: "Transactions",
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
    window.addEventListener('beforeunload', this.confirmLeave);
  },
  methods: {
    async saveTerminalSettings(noRedirect) {
      this.$refs.terminalSettingsForm.validate();
      if (!this.terminalSettingsFormValid) {
        this.$toasted.show(this.$t("FormInvalidSaveNotAllowed"), { type: "error" });
        return false;
      }
      let data = this.$refs.terminalSettingsRef.getData();

      // if(!data.bankDetails){
      //   data.bankDetails = this.terminal.bankDetails;
      // }
      
      let operationResult = await this.$api.terminals.updateTerminal(data);
      this.$refs.terminalSettingsRef.watchModel();
      if (operationResult.status === "success") {
        return noRedirect ? true : this.$router.push({ name: "Merchant", params: {id: this.terminal.merchantID} });
      }
      else if (operationResult.errors && operationResult.errors.length > 0) {
        operationResult.errors.forEach(e => {
          this.$toasted.show(e.description, { type: "error" });
        })
      }
      return false;
    },
    async refreshTerminal() {
      let terminal = await this.$api.terminals.getTerminal(this.$route.params.id);
      if (!terminal) {
        return this.$router.push({ name: "Terminals" });
      }

      if (!terminal.bankDetails) {
        //terminal.bankDetails = {};
      }

      this.terminal = terminal;
      this.terminalUpdatedKey = new Date().toString();
    },
    confirmLeave($event){
      if(this.$refs.terminalSettingsRef && this.$refs.terminalSettingsRef.changed && !window.confirm(this.$t("UnsavedChangesWarningMessage"))){
          if($event){
            $event.preventDefault();
          }
          return false;
      }
      return true;
    },
    async approveTerminal(){
      let saveTerminal = await this.saveTerminalSettings(true);
      if(!saveTerminal){
        return;
      }

      let opResult = await this.$api.terminals.approveTerminal(this.$route.params.id);

      if (!this.$apiSuccess(opResult) && opResult.message){
        this.$toasted.show(opResult.message, { type: "error" })
      }
    },
    async saveWebhooks(data){
      this.terminal.webHooksConfiguration = data;
      let payload = this.$refs.terminalSettingsRef.getData();
      payload.webHooksConfiguration = this.terminal.webHooksConfiguration;
      let operationResult = await this.$api.terminals.updateTerminal(payload);
      
      if (operationResult.errors && operationResult.errors.length > 0) {
        operationResult.errors.forEach(e => {
          this.$toasted.show(e.description, { type: "error" });
        })
      }
      this.refreshTerminal();
    }
  },
  beforeRouteLeave (to, from, next) { 
    if(this.confirmLeave()){
      next();
    }
  },
  beforeDestroy() {
    window.removeEventListener('beforeunload', this.confirmLeave)
  },
};
</script>
<style lang="scss" scoped>
  .bg-ecbg{
    background-color: var(--v-ecbg-base) !important;
  }
</style>