<template>
  <v-row class="px-3">
    <v-col cols="12" md="6">
      <v-autocomplete
        :items="merchants"
        item-text="businessName"
        item-value="merchantID"
        v-model="selectedMerchant"
        :search-input.sync="searchMerchants"
        return-object
        :label="$t('Merchant')"
        clearable
      ></v-autocomplete>
    </v-col>
    <v-col cols="12" md="6">
      <v-autocomplete
        :items="terminals"
        item-text="label"
        item-value="terminalID"
        v-model="selectedTerminal"
        :search-input.sync="searchTerminals"
        return-object
        :label="$t('Terminal')"
        clearable
      ></v-autocomplete>
    </v-col>
  </v-row>
</template>

<script>
import ValidationRules from "../../helpers/validation-rules";

export default {
  model: {
    prop: "data",
    event: "change"
  },
  props: {
    data: {
      type: Object,
      default: null,
      required: true
    }
  },
  data() {
    return {
      vr: ValidationRules,
      searchTerminals: null,
      searchTerminalsTimeout: null,
      searchMerchantsTimeout: null,
      searchMerchants: null,
      selectedMerchant: null,
      selectedTerminal: null,
      terminals: [],
      merchants: []
    };
  },
  watch: {
    async searchTerminals(val) {
      //Vuetify search-input event is triggered when terminal is selected producing redundant event, it should be ignored
      if (this.selectedTerminal && val == this.selectedTerminal.label) {
        return;
      }
      if (this.searchTerminalsTimeout) {
        clearTimeout(this.searchTerminalsTimeout);
      }

      this.searchTerminalsTimeout = setTimeout(
        () => this.getTerminals(!val || val.length < 3 ? null : val),
        1500
      );
    },
    async searchMerchants(val) {
      //Vuetify search-input event is triggered when terminal is selected producing redundant event, it should be ignored
      if (this.selectedMerchant && val == this.selectedMerchant.businessName) {
        return;
      }
      if (this.searchMerchantsTimeout) {
        clearTimeout(this.searchMerchantsTimeout);
      }

      this.searchMerchantsTimeout = setTimeout(
        () => this.getMerchants(!val || val.length < 3 ? null : val),
        1500
      );
    },
    selectedMerchant: async function(val) {
      if (!val) {
        this.data.terminalID = null;
        return;
      }

      if (this.selectedTerminal) {
        if(this.selectedTerminal.merchantID != val.merchantID){
          this.selectedTerminal = null;
        }
      } else {
        var terminal = this.lodash.find(
          this.terminals,
          t => t.merchantID == val.merchantID
        );
        if (!terminal) {
          this.data.terminalID = null;
        }
        this.getTerminals();
      }

      this.data.merchantID = val.merchantID;
      this.$emit("change", this.data);
    },
    selectedTerminal: async function(val) {
      if (!val) {
        return;
      }
      this.data.terminalID = val.terminalID;
      this.$emit("change", this.data);
    }
  },
  methods: {
    async getTerminals(search, terminalID = null) {
      let response = await this.$api.terminals.getTerminalsRaw({
        label: search,
        take: 10,
        merchantID: this.data.merchantID,
        terminalID: terminalID
      });
      this.terminals = response.data;
      if (this.terminals.length === 1) {
        this.selectedTerminal = this.terminals[0];
      }
    },
    async getMerchants(search, merchantID = null) {
      let response = await this.$api.merchants.getMerchantsRaw({
        search: search,
        merchantID: merchantID,
        take: 10
      });
      this.merchants = response.data;
      if (this.merchants.length === 1) {
        this.selectedMerchant = this.merchants[0];
      }
    }
  },
  async mounted() {
    this.getTerminals(null, this.data.terminalID);
    this.getMerchants(null, this.data.merchantID);
  }
};
</script>