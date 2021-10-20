<template>
  <v-card class="ec-card d-flex flex-column">
    <v-card-text class="py-2">
      <v-form class="ec-form" ref="form">
        <invoice-details-fields ref="invoiceDetails" :data="model.invoiceDetails" v-on:invoce-type-changed="invoiceTypeChanged($event)"></invoice-details-fields>

        <v-text-field
          v-model="model.dealDetails.consumerNationalID"
          :rules="[vr.special.israeliNationalId]"
          :label="$t('NationalID')"
          outlined
        ></v-text-field>

        <!-- <v-switch 
          v-model="isInstallmentTransaction" 
          :label="$t('InstallmentTransaction')" 
          class="pt-0 mt-0" 
          v-bind:class="{'pb-2': !isInstallmentTransaction}"
          hide-details="true"></v-switch> -->
        <v-select
          :items="dictionaries.transactionTypeEnum"
          item-text="description"
          item-value="code"
          v-model="model.transactionType"
          :label="$t('TransactionType')"
          outlined
        ></v-select>
        
        <installment-details
          ref="instDetails"
          :data="model.installmentDetails"
          v-if="isInstallmentTransaction"
          :total-amount="model.invoiceAmount"
          :key="model.transactionType"
          :transaction-type="model.transactionType"
          :hide-title="true"
        ></installment-details>

        <deal-details
          ref="dealDetails"
          :data="model"
          :key="model.dealDetails ? model.dealDetails.consumerEmail : model.dealDetails"
        ></deal-details>

        <template v-if="paymentInfoAvailable">
          <payment-type v-model="model.paymentType"></payment-type>
          <template v-if="model.paymentType == appConstants.transaction.paymentTypes.card">
            <invoice-credit-card-details-fields ref="ccDetails"></invoice-credit-card-details-fields>
          </template>
          <template v-else-if="model.paymentType == appConstants.transaction.paymentTypes.cheque">
            <cheque-details-fields ref="chequeDetails"></cheque-details-fields>
          </template>
          <template v-else-if="model.paymentType == appConstants.transaction.paymentTypes.bank">
            <bank-transfer-details-fields ref="bankDetails"></bank-transfer-details-fields>
          </template>
        </template>
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
import appConstants from "../../helpers/app-constants";

export default {
  components: {
    InstallmentDetails: () => import("../transactions/InstallmentDetailsForm"),
    DealDetails: () => import("../transactions/DealDetailsFields"),
    InvoiceDetailsFields: () => import("./InvoiceDetailsFields"),
    ChequeDetailsFields: () => import("./ChequeDetailsFields"),
    BankTransferDetailsFields: () => import("./BankTransferDetailsFields"),
    PaymentType: () => import("../transactions/PaymentType"),
    ReIcon: () => import("../../components/misc/ResponsiveIcon"),
    EcDialog: () => import("../../components/ec/EcDialog"),
    EcDialogInvoker: () => import("../../components/ec/EcDialogInvoker"),
    EcRadioGroup: () => import("../../components/inputs/EcRadioGroup"),
    InvoiceCreditCardDetailsFields: () => import("../../components/invoicing/InvoiceCreditCardDetailsFields"),
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
        invoiceDetails: this.data.invoiceDetails || {},
        transactionType: null
      },
      vr: ValidationRules,
      appConstants: appConstants,
      messageDialog: false,
      paymentInfoAvailable: true
    };
  },
  computed: {
    ...mapState({
      currencyStore: state => state.settings.currency
    }),
    isInstallmentTransaction() {
      return (
        this.model.transactionType === "installments" ||
        this.model.transactionType === "credit"
      );
    }
  },
  async mounted() {
    let dictionaries = await this.$api.dictionaries.getTransactionDictionaries();
    if (dictionaries) {
      this.dictionaries = dictionaries;
      if (!this.model.currency) {
        this.model.currency =
          this.currencyStore.code || this.dictionaries.currencyEnum[0].code;
      }
      // this.model.cardPresence = this.dictionaries.cardPresenceEnum[1].code;
      this.model.transactionType = this.dictionaries.transactionTypeEnum[0].code;
    }
  },
  methods: {
    ok() {
      let result = { ...this.model };
      if (this.$refs.instDetails) {
        result.installmentDetails = this.$refs.instDetails.getData();
      } else {
        result.installmentDetails = null;
      }

      result.invoiceDetails = this.$refs.invoiceDetails.getData();
      result.dealDetails = this.$refs.dealDetails.getData();

      if (!this.$refs.form.validate()) {
        const self = this;
        this.$nextTick(function(){
          self.$refs.form.validate();
        });
        return;
      }

      if (this.paymentInfoAvailable){
        result.paymentDetails = [];
        let data = null;
        switch(this.model.paymentType){
          case this.appConstants.transaction.paymentTypes.card:
            data = this.$refs.ccDetails.getData();
            if(data){
              result.paymentDetails.push({
                ...data,
                paymentType: this.appConstants.transaction.paymentTypes.card
              });
            }
            break;
          case this.appConstants.transaction.paymentTypes.cash:
            result.paymentDetails.push({
              paymentType: this.appConstants.transaction.paymentTypes.cash
            });
            break;
          case this.appConstants.transaction.paymentTypes.cheque:
            data = this.$refs.chequeDetails.getData();
            if(data){
              result.paymentDetails.push({
                ...data,
                paymentType: this.appConstants.transaction.paymentTypes.cheque
              });
            }
            break;
          case this.appConstants.transaction.paymentTypes.bank:
            data = this.$refs.bankDetails.getData();
            if(data){
              result.paymentDetails.push({
                ...data,
                paymentType: this.appConstants.transaction.paymentTypes.bank
              });
            }
            break;
        }
      }
      if (result.invoiceDetails) this.$emit("ok", result);
    },
    invoiceTypeChanged(val){
      this.paymentInfoAvailable = (val.code == appConstants.invoicing.types.invoiceWithPaymentInfo
        || val.code == appConstants.invoicing.types.paymentInfo
        || val.code == appConstants.invoicing.types.refundInvoice);
    }
  }
};
</script>

<style lang="scss" scoped>
</style>