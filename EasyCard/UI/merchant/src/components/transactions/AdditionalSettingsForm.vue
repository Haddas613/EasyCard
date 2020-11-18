<template>
  <v-card class="ec-card d-flex flex-column">
    <v-card-text class="py-2">
      <v-form class="ec-form" ref="form" lazy-validation>
        <v-select
          :items="dictionaries.jDealTypeEnum"
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
          outlined
        ></v-select>

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

        <v-switch v-model="switchIssueDocument" :label="$t('IssueDocument')" class="pt-0 mt-0"></v-switch>
        <div v-if="switchIssueDocument">
          <invoice-details-form ref="invoiceDetails" :data="model.invoiceDetails"></invoice-details-form>
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
import { mapState } from "vuex";

export default {
  components: {
    InstallmentDetails: () => import("./InstallmentDetailsForm"),
    DealDetails: () => import("../transactions/DealDetailsForm"),
    InvoiceDetailsForm: () => import("../invoicing/InvoiceDetailsForm"),
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
      messageDialog: false
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
      currencyStore: state => state.settings.currency
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

      if (this.switchIssueDocument) {
        result.invoiceDetails = this.$refs.invoiceDetails.getData();
      } else {
        result.invoiceDetails = null;
      }
      result.dealDetails = this.$refs.dealDetails.getData();
      this.$emit("ok", result);
    }
  }
};
</script>

<style lang="scss" scoped>
</style>