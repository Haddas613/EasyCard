<template>
  <v-flex>
    <v-dialog v-model="deleteDialog" width="500">
      <v-card>
        <v-card-title>{{$t("Confirmation")}}</v-card-title>
        <v-divider></v-divider>
        <v-card-text class="pt-2">{{$t("AreYouSureYouWantToDeleteBillingDeal")}}</v-card-text>
        <v-card-actions>
          <v-spacer></v-spacer>
          <v-btn @click="deleteDialog = false;">{{$t("Cancel")}}</v-btn>
          <v-btn color="primary" @click="deleteBillingDeal()">{{$t("OK")}}</v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>
    <v-tabs grow>
      <v-tab key="info">{{$t("GeneralInfo")}}</v-tab>
      <v-tab-item key="info">
        <div v-if="model">
          <v-card flat class="mb-2">
            <!-- <v-card-title class="py-3 ecdgray--text subtitle-2 text-uppercase">{{$t('GeneralInfo')}}</v-card-title>
            <v-divider></v-divider>-->
            <v-card-text class="body-1 black--text" v-if="model">
              <v-row class="info-container">
                <v-col cols="12" md="4" class="info-block">
                  <p class="caption ecgray--text text--darken-2">{{$t('BillingDealID')}}</p>
                  <v-chip color="primary" small>{{model.$billingDealID | guid}}</v-chip>
                </v-col>
                <v-col cols="12" md="4" class="info-block" v-if="model.invoiceOnly">
                  <p class="caption ecgray--text text--darken-2">{{$t('InvoiceOnly')}}</p>
                  <p>{{$t("Yes")}}</p>
                </v-col>
                <v-col cols="12" md="4" class="info-block">
                  <p class="caption ecgray--text text--darken-2">{{$t('Terminal')}}</p>
                  <p>
                    <router-link link :to="{name: 'EditTerminal', params: {id: model.$terminalID || model.terminalID}}">
                      {{model.terminalName}}
                    </router-link>
                  </p>
                </v-col>
                <v-col cols="12" md="4" class="info-block">
                  <p class="caption ecgray--text text--darken-2">{{$t('Created')}}</p>
                  <p>{{model.$billingDealTimestamp | ecdate('LLLL')}}</p>
                </v-col>

                 <v-col cols="12" md="4" class="info-block">
                  <p class="caption ecgray--text text--darken-2">{{$t('Active')}}</p>
                  <p v-if="model.paused" class="accent--text">{{$t("Paused")}}</p>
                  <p v-else-if="model.active" class="success--text">{{$t("Yes")}}</p>
                  <p v-else class="error--text">{{$t("No")}}</p>
                </v-col>
                <template v-if="model.paused">
                  <v-col cols="12" md="4" class="info-block">
                    <p class="caption accent--text">{{$t('From')}}</p>
                    <p>{{model.pausedFrom | ecdate}}</p>
                  </v-col>
                  <v-col cols="12" md="4" class="info-block">
                    <p class="caption accent--text">{{$t('To')}}</p>
                    <p>{{model.pausedTo | ecdate}}</p>
                  </v-col>
                </template>
                <template v-else>
                  <v-col cols="12" md="4" class="info-block">
                    <p class="caption ecgray--text text--darken-2">{{$t('NextScheduledTransaction')}}</p>
                    <p v-if="model.$nextScheduledTransaction">{{model.$nextScheduledTransaction | ecdate('L')}}</p>
                    <p v-else>-</p>
                  </v-col>
                </template>
              </v-row>
              <v-row class="pt-2 info-container" v-if="model.lastError">
                <v-col cols="12" md="4" class="info-block">
                  <p class="caption error--text text--darken-2">{{$t('LastError')}}</p>
                  <p>{{model.lastError || '-'}}</p>
                </v-col>
                <v-col cols="12" md="4" class="info-block">
                  <p class="caption error--text text--darken-2">{{$t('LastErrorCorrelationID')}}</p>
                  <p>{{model.lastErrorCorrelationID || '-'}}</p>
                </v-col>
              </v-row>
            </v-card-text>
          </v-card>
          <v-card flat class="my-2">
            <v-card-title
              class="py-3 ecdgray--text subtitle-2 text-uppercase info-block-title"
            >{{$t("BillingSchedule")}}</v-card-title>
            <v-divider></v-divider>
            <v-card-text>
              <billing-schedule-string
                :schedule="model.billingSchedule"
                replacement-text="ScheduleIsNotDefined"
              ></billing-schedule-string>
            </v-card-text>
          </v-card>
          <amount-details :model="model" amount-key="transactionAmount"></amount-details>
          <v-card flat class="my-2" v-if="model.dealDetails && model.dealDetails.items.length > 0">
            <v-card-title
              class="py-3 ecdgray--text subtitle-2 text-uppercase info-block-title"
            >{{$t("Items")}}</v-card-title>
            <v-divider></v-divider>
            <v-card-text>
              <transaction-items-list :items="model.dealDetails.items" :currency="model.$currency"></transaction-items-list>
            </v-card-text>
          </v-card>

          <deal-details
            :model="model.dealDetails"
            :consumer-name="model.creditCardDetails ? model.creditCardDetails.cardOwnerName : null"
          ></deal-details>

          <payment-details v-if="model.paymentDetails" :model="model.paymentDetails"></payment-details>
          <credit-card-details v-if="model.creditCardDetails" :model="model.creditCardDetails"></credit-card-details>
          <bank-payment-details card v-if="model.bankDetails" :model="model.bankDetails"></bank-payment-details>

          <v-card flat class="my-2" v-if="model.invoiceDetails">
            <v-card-title
              class="py-3 ecdgray--text subtitle-2 text-uppercase info-block-title"
            >{{$t("InvoiceDetails")}}</v-card-title>
            <v-divider></v-divider>
            <v-card-text>
              <v-row class="info-container body-1 black--text" v-if="model">
                <v-col cols="12" md="4" class="info-block">
                  <p class="caption ecgray--text text--darken-2">{{$t('InvoiceSubject')}}</p>
                  <p>{{model.invoiceDetails.invoiceSubject}}</p>
                </v-col>
                <v-col cols="12" md="4" class="info-block">
                  <p class="caption ecgray--text text--darken-2">{{$t('InvoiceType')}}</p>
                  <p>{{model.invoiceDetails.invoiceType}}</p>
                </v-col>
                <v-col cols="12" md="4" class="info-block">
                  <p class="caption ecgray--text text--darken-2">{{$t('SendCCTo')}}</p>
                  <p>{{model.invoiceDetails.sendCCTo || '-'}}</p>
                </v-col>
              </v-row>
            </v-card-text>
          </v-card>
        </div>
      </v-tab-item>

      <v-tab key="financialItems">{{model && model.invoiceOnly ? $t("Invoices") : $t("Transactions")}}</v-tab>
      <v-tab-item key="financialItems">
        <invoices-list v-if="model && model.invoiceOnly && financialItems" :invoices="financialItems" class="pt-4 pb-2"></invoices-list>
        <transactions-list v-else-if="model && financialItems" :transactions="financialItems" class="pt-4 pb-2"></transactions-list>
      </v-tab-item>

      <v-tab key="history">{{$t("History")}}</v-tab>
      <v-tab-item key="history">
        <billing-deal-history v-if="model" :billing-deal-id="model.$billingDealID"></billing-deal-history>
      </v-tab-item>
    </v-tabs>
  </v-flex>
</template>

<script>
export default {
  components: {
    BillingScheduleString: () =>
      import("../../components/billing-deals/BillingScheduleString"),
    BillingDealHistory: () =>
      import("../../components/billing-deals/BillingDealHistory"),
    TransactionsList: () =>
      import("../../components/transactions/TransactionsList"),
    TransactionItemsList: () =>
      import("../../components/transactions/TransactionItemsList"),
    DealDetails: () => import("../../components/details/DealDetails"),
    AmountDetails: () => import("../../components/details/AmountDetails"),
    CreditCardDetails: () =>
      import("../../components/details/CreditCardDetails"),
    BankPaymentDetails: () => import("../../components/details/BankPaymentDetails"),
    PaymentDetails: () => import("../../components/details/PaymentDetails"),
    InvoicesList: () =>
      import("../../components/invoicing/InvoicesList"),
  },
  data() {
    return {
      model: null,
      financialItems: null,
      financialItemsFilter: {
        take: 100,
        skip: 0,
        billingDealID: this.$route.params.id
      },
      numberOfRecords: 0,
      deleteDialog: false
    };
  },
  methods: {
    async deleteBillingDeal() {
      let opResult = await this.$api.billingDeals.deleteBillingDeal(
        this.$route.params.id
      );
      if (opResult.status === "success") {
        this.$router.push({ name: "BillingDeals" });
      }
    }
  },
  async mounted() {
    this.model = await this.$api.billingDeals.getBillingDeal(
      this.$route.params.id
    );

    if (!this.model) {
      return this.$router.push({name: "BillingDeals"});
    }

    let data = this.model.invoiceOnly ? (await this.$api.invoicing.get(this.financialItemsFilter)) 
      : (await this.$api.transactions.get(this.financialItemsFilter));

    data = data || {};
    this.financialItems = data.data || [];
    this.numberOfRecords = data.numberOfRecords || 0;

    if (
      this.model.invoiceDetails &&
      this.model.invoiceDetails.sendCCTo &&
      this.model.invoiceDetails.sendCCTo.length > 0
    ) {
      this.model.invoiceDetails.sendCCTo = this.model.invoiceDetails.sendCCTo.join(
        ","
      );
    }

    let $dictionaries = await this.$api.dictionaries.$getTransactionDictionaries();

    if (this.model.invoiceDetails && this.model.invoiceDetails.invoiceType) {
      this.model.invoiceDetails.invoiceType =
        $dictionaries.invoiceTypeEnum[this.model.invoiceDetails.invoiceType];
    }


    // if (!this.model.active) {
    //   var newHeader = { text: { translate: true, value: 'BillingDeal(Deleted)' } };
    //   this.$store.commit("ui/changeHeader", { value: newHeader});
    // }
  }
};
</script>