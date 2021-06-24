<template>
  <v-card class="ec-card d-flex flex-column">
    <v-card-text class="py-2">
      <v-form class="ec-form" ref="form" lazy-validation>
        <v-select
          :items="jDealTypes"
          item-text="description"
          item-value="code"
          v-model="model.jDealType"
          :label="$t('JDealType')"
          outlined
        ></v-select>
        <v-select
          :items="dictionaries.transactionTypeEnum"
          item-text="description"
          item-value="code"
          v-model="model.transactionType"
          :label="$t('TransactionType')"
          @change="updateTransactionType()"
          outlined
        ></v-select>

        <installment-details
          ref="instDetails"
          :data="model.installmentDetails"
          v-if="isInstallmentTransaction"
          :total-amount="model.transactionAmount"
        ></installment-details>

        <deal-details
          ref="dealDetails"
          :data="model.dealDetails"
          :key="model.dealDetails ? model.dealDetails.consumerEmail : model.dealDetails"
        ></deal-details>

        <v-switch v-if="$integrationAvailable(terminalStore, appConstants.terminal.integrations.invoicing)" v-model="switchIssueDocument" :label="$t('IssueDocument')" class="pt-0 mt-0"></v-switch>
        <div v-if="switchIssueDocument">
          <invoice-details-fields :key="invoiceTypeUpd" ref="invoiceDetails" :data="model.invoiceDetails" :invoice-type="invoiceTypeUpd"></invoice-details-fields>
        </div>
      </v-form>
    </v-card-text>
    <v-card-actions class="px-2">
      <v-btn color="primary" bottom :x-large="true" block @click="ok()">{{$t('Confirm')}}</v-btn>
    </v-card-actions>
  </v-card>
</template>

<script>
import ValidationRules from "../../helpers/validation-rules";
import appConstants from "../../helpers/app-constants";
import { mapState } from "vuex";

export default {
  components: {
    InstallmentDetails: () => import("./InstallmentDetailsForm"),
    DealDetails: () => import("../transactions/DealDetailsFields"),
    InvoiceDetailsFields: () => import("../invoicing/InvoiceDetailsFields"),
    ReIcon: () => import("../../components/misc/ResponsiveIcon"),
  },
  props: {
    data: {
      type: Object,
      default: null,
      required: true
    },
    issueDocument: {
      type: Boolean,
      default: false
    },
    invoiceType: {
      type: String,
      default: null
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
      switchIssueDocument: this.issueDocument,
      messageDialog: false,
      invoiceTypeUpd: this.invoiceType,
      jDealTypes: [],
      appConstants: appConstants
    };
  },
  computed: {
    isInstallmentTransaction() {
      return (
        this.model.transactionType === "installments" ||
        this.model.transactionType === "credit"
      );
    },
    ...mapState({
      currencyStore: state => state.settings.currency,
      terminalStore: state => state.settings.terminal
    })
  },
  async mounted() {
    let dictionaries = await this.$api.dictionaries.getTransactionDictionaries();
    if (dictionaries) {
      this.dictionaries = dictionaries;
      this.model.transactionType = this.dictionaries.transactionTypeEnum[0].code;

      if (!this.model.currency) {
        this.model.currency =
          this.currencyStore.code || this.dictionaries.currencyEnum[0].code;
      }

      this.model.jDealType = this.dictionaries.jDealTypeEnum[0].code;
      let filteredJDealTypes = [];
      for (var jDeal of dictionaries.jDealTypeEnum){
        if(jDeal.code == "J4"){
          filteredJDealTypes.push(jDeal);
        }else if(jDeal.code == "J5"){
          if(this.terminalStore.settings.j5Allowed){
            filteredJDealTypes.push(jDeal);
          }
        }else if(jDeal.code == "J2"){
          if(this.terminalStore.settings.j2Allowed){
            filteredJDealTypes.push(jDeal);
          }
        }
      }
      this.jDealTypes = filteredJDealTypes;
      // this.model.cardPresence = this.dictionaries.cardPresenceEnum[1].code;
    }

    if(this.issueDocument){
      this.switchIssueDocument = this.$integrationAvailable(this.terminalStore, appConstants.terminal.integrations.invoicing);
    }
  },
  methods: {
    ok() {
      if (!this.$refs.form.validate()) return;

      let result = { ...this.model };
      if (this.$refs.instDetails) {
        result.installmentDetails = this.$refs.instDetails.getData();
      }else{
        result.installmentDetails = null;
      }

      if (this.switchIssueDocument) {
        result.invoiceDetails = this.$refs.invoiceDetails.getData();
      } else {
        result.invoiceDetails = null;
      }
      result.dealDetails = this.$refs.dealDetails.getData();
      this.$emit("ok", result);
    },
    updateTransactionType(){
      if(this.model.transactionType === "credit"){
        this.invoiceTypeUpd = this.terminal.invoiceSettings.defaultCreditInvoiceType || appConstants.invoicing.defaultCreditInvoiceType;
      }else{
        this.invoiceTypeUpd = this.invoiceType;
      }
    }
  }
};
</script>

<style lang="scss" scoped>
</style>