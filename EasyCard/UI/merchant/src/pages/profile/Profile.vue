<template>
  <div>
    <v-card flat color="ecbg">
      <v-card-text>
        <v-row no-gutters>
          <v-col cols="12" md="4" class="text-start d-flex align-center">
            <span>{{$t("@AppVersion").replace("@version", appVersion)}}</span>
          </v-col>
          <v-col cols="12" md="8" class="text-end mb-4">
            <v-btn
              v-bind:class="{'mt-1': $vuetify.breakpoint.smAndDown}"
              class="mx-1"
              color="primary"
              target="_blank"
              link
              :href="'https://github.com/HaddasEC/ECNG'"
              :block="$vuetify.breakpoint.smAndDown"
            >
              <v-icon left>mdi-file-document</v-icon>
              {{$t("Docs")}}
            </v-btn>
            <v-btn
              v-bind:class="{'mt-1': $vuetify.breakpoint.smAndDown}"
              class="mx-1"
              color="secondary"
              target="_blank"
              link
              :href="$cfg.VUE_APP_AUTHORITY + '/Home/ManageAccount'"
              :block="$vuetify.breakpoint.smAndDown"
            >
              <v-icon left>mdi-account</v-icon>
              {{$t("AccountSettings")}}
            </v-btn>
            <v-btn
              v-bind:class="{'mt-2': $vuetify.breakpoint.smAndDown}"
              class="mx-1"
              @click="$oidc.signOut()"
              :block="$vuetify.breakpoint.smAndDown"
            >
              <v-icon left>mdi-logout</v-icon>
              {{$t("SignOut")}}
            </v-btn>
          </v-col>
        </v-row>
        <v-row no-gutters>
          <v-col cols="12" md="4">
            <lang-switcher class></lang-switcher>
          </v-col>
          <v-col cols="12" md="4">
            <v-select
              :items="terminals"
              item-text="label"
              item-value="terminalID"
              return-object
              v-model="terminal"
              :label="$t('Terminal')"
              outlined
              v-bind:class="{'px-1' : $vuetify.breakpoint.mdAndUp}"
            ></v-select>
          </v-col>
          <v-col cols="12" md="4">
            <v-select
              :items="currencies"
              item-text="description"
              return-object
              v-model="currency"
              :label="$t('Currency')"
              outlined
            ></v-select>
          </v-col>
        </v-row>
      </v-card-text>
    </v-card>
    <v-card class="mt-2" flat color="ecbg">
      <v-card-title class="subtitle-1">{{$t("TerminalSettings")}}</v-card-title>
      <v-divider></v-divider>
      <v-card-text>
        <v-form ref="terminalSettingsForm" v-model="terminalSettingsFormValid" lazy-validation>
          <terminal-settings-fields
            v-if="terminalRefreshed"
            :key="terminalStore ? terminalStore.terminalID : false"
            :data="terminalStore"
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
      </v-card-text>
    </v-card>
  </div>
</template>

<script>
import { mapState } from "vuex";
export default {
  components: {
    LangSwitcher: () => import("../../components/LanguageSwitcher"),
    TerminalSettingsFields: () =>
      import("../../components/settings/TerminalSettingsFields"),
    ChargeWizardSettings: () =>
      import("../../components/settings/ChargeWizardSettings")
  },
  data() {
    return {
      terminals: [],
      currencies: [],
      terminalSettingsFormValid: true,
      terminalRefreshed: false,
      appVersion: ""
    };
  },
  async mounted() {
    if (this.$cfg.VUE_APP_VERSION != this.$appConstants.misc.uiDefaultVersion) {
      this.appVersion = this.$cfg.VUE_APP_VERSION;
    }

    let terminals = await this.$api.terminals.getTerminals();
    this.terminals = terminals ? terminals.data : [];
    let dictionaries = await this.$api.dictionaries.getTransactionDictionaries();
    this.currencies = dictionaries ? dictionaries.currencyEnum : [];
    await this.refreshTerminal();
    this.terminalRefreshed = true;
    window.addEventListener("beforeunload", this.confirmLeave);
  },
  computed: {
    ...mapState({
      terminalStore: state => state.settings.terminal,
      currencyStore: state => state.settings.currency
    }),
    terminal: {
      get: function() {
        return this.terminalStore;
      },
      set: function(nv) {
        this.$store.dispatch("settings/changeTerminal", {
          api: this.$api,
          newTerminal: nv
        });
      }
    },
    currency: {
      get: function() {
        return this.currencyStore;
      },
      set: function(nv) {
        this.$store.dispatch("settings/changeCurrency", {
          api: this.$api,
          newCurrency: nv
        });
      }
    }
  },
  methods: {
    async saveTerminalSettings() {
      this.$refs.terminalSettingsForm.validate();
      if (!this.terminalSettingsFormValid) {
        this.$toasted.show(this.$t("FormInvalidSaveNotAllowed"), { type: "error" });
        return false;
      }
      
      let data = this.$refs.terminalSettingsRef.getData();
      let operaionResult = await this.$api.terminals.updateTerminal(data);
      if (operaionResult.status === "success") {
        this.terminalRefreshed = false;
        await this.refreshTerminal();
        let terminals = await this.$api.terminals.getTerminals(null, {
          refreshCache: true
        });
        this.terminals = terminals ? terminals.data : [];
        this.terminalRefreshed = true;
      }
    },
    async refreshTerminal() {
      await this.$store.dispatch("settings/refreshTerminal", {
        api: this.$api
      });
    },
    confirmLeave($event) {
      if ( this.$refs.terminalSettingsRef.changed && !window.confirm(this.$t("UnsavedChangesWarningMessage"))) {
        if ($event) {
          $event.preventDefault();
        }
        return false;
      }
      return true;
    }
  },
  beforeRouteLeave(to, from, next) {
    if (this.confirmLeave()) {
      next();
    }
  },
  beforeDestroy() {
    window.removeEventListener("beforeunload", this.confirmLeave);
  }
};
</script>