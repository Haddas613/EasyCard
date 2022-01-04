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
        <v-tab-item key="features">
          <terminal-features-form :terminal="terminal" v-if="terminal" @update="refreshTerminal()"></terminal-features-form>
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
      terminal: null,
      terminalSettingsFormValid: true,
      tab: "settings"
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
    async saveTerminalSettings() {
      if (!this.terminalSettingsFormValid) {
        return;
      }
      let data = this.$refs.terminalSettingsRef.getData();

      // if(!data.bankDetails){
      //   data.bankDetails = this.terminal.bankDetails;
      // }
      
      let operaionResult = await this.$api.terminals.updateTerminal(data);
      this.$refs.terminalSettingsRef.watchModel();
      if (operaionResult.status === "success") {
        return this.$router.push({ name: "Merchant", params: {id: this.terminal.merchantID} });
      }
      else if (operaionResult.errors && operaionResult.errors.length > 0) {
        operaionResult.errors.forEach(e => {
          this.$toasted.show(e.description, { type: "error" });
        })
      }
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
    },
    confirmLeave($event){
      if(this.$refs.terminalSettingsRef && this.$refs.terminalSettingsRef.changed && !window.confirm(this.$t("UnsavedChangesWarningMessage"))){
          if($event){
            $event.preventDefault();
          }
          return false;
      }
      return true;
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