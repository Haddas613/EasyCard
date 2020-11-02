<template>
  <v-card class="ec-card d-flex flex-column">
    <v-card-text class="py-2">
      <v-form class="ec-form" ref="form" lazy-validation>

        <installment-details
          ref="instDetails"
          :data="model.installmentDetails"
          v-if="isInstallmentTransaction"
        ></installment-details>

        <deal-details
          ref="dealDetails"
          :data="model.dealDetails"
          :key="model.dealDetails ? model.dealDetails.consumerEmail : model.dealDetails"
        ></deal-details>

        <invoice-details-form ref="invoiceDetails" :data="model.invoiceDetails"></invoice-details-form>
      </v-form>
    </v-card-text>
    <v-card-actions class="px-2">
      <v-btn color="primary" bottom :x-large="true" block @click="ok()">{{$t('Confirm')}}</v-btn>
    </v-card-actions>
  </v-card>
</template>

<script>
import ValidationRules from "../../helpers/validation-rules";
import { mapState } from "vuex";

export default {
  components: {
    InstallmentDetails: () => import("../transactions/InstallmentDetailsForm"),
    DealDetails: () => import("../transactions/DealDetailsForm"),
    InvoiceDetailsForm: () => import("./InvoiceDetailsForm"),
    ReIcon: () => import("../../components/misc/ResponsiveIcon"),
    EcDialog: () => import("../../components/ec/EcDialog"),
    EcDialogInvoker: () => import("../../components/ec/EcDialogInvoker"),
    EcRadioGroup: () => import("../../components/inputs/EcRadioGroup")
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
      messageDialog: false
    };
  },
  computed: {
    isInstallmentTransaction() {
      return false;//TODO
      return (
        this.model.invoiceDetails.invoiceType === "installments" ||
        this.model.invoiceDetails.invoiceType === "credit"
      );
    },
    ...mapState({
      currencyStore: state => state.settings.currency
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
      if (!this.model.invoiceDetails.invoiceType) {
        this.$set(this.model.invoiceDetails, 'invoiceType', this.dictionaries.invoiceTypeEnum[0]);
      }
      // this.model.cardPresence = this.dictionaries.cardPresenceEnum[1].code;
    }
  },
  methods: {
    ok() {
      if (!this.$refs.form.validate()) return;

      let result = { ...this.model };
      if (this.$refs.instDetails) {
        result.installmentDetails = this.$refs.instDetails.model;
      }else{
        result.installmentDetails = null;
      }

      result.invoiceDetails = this.$refs.invoiceDetails.getData();
      result.dealDetails = this.$refs.dealDetails.getData();
      if(result.invoiceDetails) this.$emit("ok", result);
    }
  }
};
</script>

<style lang="scss" scoped>
</style>