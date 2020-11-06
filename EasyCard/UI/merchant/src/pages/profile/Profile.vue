<template>
  <v-card flat>
    <v-card-text>
      <v-row no-gutters>
        <v-col cols="4" md="2">
          <v-switch class="px-2" :color="'accent'" label="RTL" v-model="$vuetify.rtl"></v-switch>
        </v-col>
        <v-col cols="8" md="10" class="text-end">
          <v-btn @click="mainAuth.signOut()">
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
</template>

<script>
import LangSwitcher from "../../components/LanguageSwitcher";
import { mapState } from "vuex";

export default {
  components: { LangSwitcher },
  data() {
    return {
      terminals: [],
      currencies: []
    };
  },
  async mounted() {
    let terminals = await this.$api.terminals.getTerminals();
    this.terminals = terminals ? terminals.data : [];
    let dictionaries = await this.$api.dictionaries.getTransactionDictionaries();
    this.currencies = dictionaries ? dictionaries.currencyEnum : [];
  },
  computed: {
    isRtl: {
      cache: false,
      get: function() {
        return this.$vuetify.rtl === true;
      }
    },
    ...mapState({
      terminalStore: state => state.settings.terminal,
      currencyStore: state => state.settings.currency
    }),
    terminal: {
      get: function() {
        return this.terminalStore;
      },
      set: function(nv) {
        this.$store.commit("settings/changeTerminal", {
          vm: this,
          newTerminal: nv
        });
      }
    },
    currency: {
      get: function() {
        return this.currencyStore;
      },
      set: function(nv) {
        this.$store.commit("settings/changeCurrency", {
          vm: this,
          newCurrency: nv
        });
      }
    }
  }
};
</script>