<template>
  <ec-dialog :dialog.sync="visible">
    <template v-slot:title>{{$t('TriggerBillingDealsByTerminal')}}</template>
    <template>
      <v-form class="mt-4" ref="form" lazy-validation>
        <merchant-terminal-filter v-model="model" :clearable="false"></merchant-terminal-filter>
      </v-form>
      <div class="mt-4 d-flex justify-end">
        <v-btn :disabled="!model.terminalID" color="success" @click="triggerBillingDeals()" :loading="loading">{{$t("OK")}}</v-btn>
      </div>
    </template>
  </ec-dialog>
</template>

<script>
import ValidationRules from "../../helpers/validation-rules";

export default {
  props: {
    show: {
      type: Boolean,
      default: false,
      required: true
    }
  },
  components: {
    EcDialog: () => import("../../components/ec/EcDialog"),
    MerchantTerminalFilter: () => import("../filtering/MerchantTerminalFilter"),
  },
  data() {
    return {
      model: {
        merchantID: null,
        terminalID: null
      },
      vr: ValidationRules,
      loading: false
    };
  },
  async mounted() {
    let terminals = await this.$api.terminals.getTerminals();
    this.terminals = terminals ? terminals.data : [];
  },
  computed: {
    visible: {
      get: function() {
        return this.show;
      },
      set: function(val) {
        this.$emit("update:show", val);
      }
    }
  },
  methods: {
    async triggerBillingDeals() {
      if(!this.model.terminalID){
        return;
      }

      let opResult = await this.$api.billingDeals.triggerBillingDealsByTerminal(this.model.terminalID);
    
      this.visible = false;
      this.$emit("ok");
    }
  }
};
</script>