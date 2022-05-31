<template>
  <v-card class="ec-card d-flex flex-column">
    <v-card-text class="py-2">
      <v-form class="ec-form" ref="form" lazy-validation>

        <v-row no-gutters>
          <v-col cols="12" md="6" class="pb-2">
            <v-switch 
              v-model="model.isRefund" 
              :label="$t('Refund')" 
              class="pt-0 mt-0"
              color="error"
              hide-details="true"></v-switch>
          </v-col>
          <v-col cols="12" md="6">
            <v-switch 
              v-model="model.allowPinPad" 
              :label="$t('AllowPinPad')" 
              class="pt-0 mt-0"
              hide-details="true"></v-switch>
          </v-col>
          <v-col cols="12" md="6">
            <v-switch 
              v-model="isInstallmentTransaction" 
              :label="$t('InstallmentTransaction')" 
              class="pt-0 mt-0"
              hide-details="true"></v-switch>
          </v-col>
          <v-col cols="12" md="6">
            <v-switch 
              v-model="model.userAmount"
              v-if="!model.paymentRequestAmount"
              :label="$t('AllowUserAmount')" 
              class="pt-0 mt-0"
              hide-details="true"></v-switch>
          </v-col>
        </v-row>

        <installment-details
          class="mt-2"
          ref="instDetails"
          :data="model.installmentDetails"
          v-if="isInstallmentTransaction"
          :total-amount="model.paymentRequestAmount"
          :key="model.transactionType"
          :transaction-type="model.transactionType"
          hide-title
        ></installment-details>

        <deal-details
          ref="dealDetails"
          :data="model"
          :key="model.dealDetails ? model.dealDetails.consumerEmail : model.dealDetails"
          :isPaymentRequest = true
        ></deal-details>

        <ec-date-input v-model="model.dueDate" :min="minDate" :label="$t('DueDate')" />

        <invoice-details-fields v-if="invoiceAvailable" ref="invoiceDetails" :data="model.invoiceDetails"></invoice-details-fields>
      </v-form>
    </v-card-text>
    <v-card-actions class="px-2">
      <v-row no-gutters>
        <v-col cols="12" md="6" v-bind:class="{'px-1': $vuetify.breakpoint.mdAndUp}">
          <v-btn color="primary" bottom :x-large="true" block @click="ok()">{{$t('PaymentRequest')}}</v-btn>
        </v-col>
        <v-col cols="12" md="6" v-bind:class="{'px-1': $vuetify.breakpoint.mdAndUp, 'pt-1': $vuetify.breakpoint.smAndDown}">
          <v-btn color="secondary" bottom :x-large="true" block @click="ok(true)">{{$t('CreatePaymentLink')}}</v-btn>
        </v-col>
      </v-row>
    </v-card-actions>
  </v-card>
</template>

<script>
import ValidationRules from "../../helpers/validation-rules";
import { mapState } from "vuex";

export default {
  components: {
    InstallmentDetails: () => import("../transactions/InstallmentDetailsForm"),
    DealDetails: () => import("../transactions/DealDetailsFields"),
    InvoiceDetailsFields: () => import("../invoicing/InvoiceDetailsFields"),
    ReIcon: () => import("../../components/misc/ResponsiveIcon"),
    EcDialog: () => import("../../components/ec/EcDialog"),
    EcDialogInvoker: () => import("../../components/ec/EcDialogInvoker"),
    EcRadioGroup: () => import("../../components/inputs/EcRadioGroup"),
    EcDateInput: () => import("../../components/inputs/EcDateInput"),
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
      dictionaries: {},
      model: { 
        ...this.data, 
        invoiceDetails: this.data.invoiceDetails || {} 
      },
      vr: ValidationRules,
      messageDialog: false,
      minDate: new Date().toISOString(),
      isInstallmentTransaction: false,
      invoiceAvailable: false
    };
  },
  computed: {
    ...mapState({
      currencyStore: state => state.settings.currency,
      terminalStore: state => state.settings.terminal
    })
  },
  async mounted() {
    let dictionaries = await this.$api.dictionaries.getTransactionDictionaries();
    if (dictionaries) {
      this.dictionaries = dictionaries;

      if (!this.model.currency) {
        this.model.currency =
          this.currencyStore.code || this.dictionaries.currencyEnum[0].code;
      }

      this.invoiceAvailable = this.model.currency == 'ILS' 
        && this.$integrationAvailable(this.terminalStore, this.$appConstants.terminal.integrations.invoicing);

      // this.model.cardPresence = this.dictionaries.cardPresenceEnum[1].code;
    }
  },
  methods: {
    ok(paymentIntent = false) {
      if (!this.$refs.form.validate()) return;
      
      if(this.model.userAmount && this.model.paymentRequestAmount){
        this.model.userAmount = false;
      }

      let result = { ...this.model };
      if (this.isInstallmentTransaction) {
        result.installmentDetails = this.$refs.instDetails.getData();
      }else{
        result.installmentDetails = null;
      }
      result.invoiceDetails = this.invoiceAvailable
          ? this.$refs.invoiceDetails.getData() : null;
      result.dealDetails = this.$refs.dealDetails.getData();
      result.paymentIntent = paymentIntent;
      this.$emit("ok", result);
    }
  }
};
</script>

<style lang="scss" scoped>
</style>