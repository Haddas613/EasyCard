<template>
  <v-flex>
    <v-tabs grow>
      <v-tab key="info">{{$t("GeneralInfo")}}</v-tab>
      <v-tab-item key="info">
        <div v-if="model">
          <billing-deal-pause-dialog :show.sync="pauseDialog" :billing="model" v-on:ok="billingPausedChanged($event)"></billing-deal-pause-dialog>
          <v-card flat class="mb-2">
            <v-card-text class="body-1 black--text" v-if="model">
              <div v-if="!model.invoiceOnly">
                <v-alert dense text :border="$vuetify.rtl ? 'right': 'left'" icon="mdi-alert-octagon" type="error" v-if="model.cardExpired">
                  {{$t("CreditCardExpired")}}
                </v-alert>
                <v-alert dense text :border="$vuetify.rtl ? 'right': 'left'" icon="mdi-alert-octagon" type="error" v-if="model.tokenNotAvailable">
                  {{$t("CreditCardTokenNotAvailable")}}
                </v-alert>
              </div>
              <v-row class="info-container">
                <v-col cols="12" md="4" class="info-block">
                  <p class="caption ecgray--text text--darken-2">{{$t('BillingDealID')}}</p>
                  <v-chip color="primary" small>{{model.billingDealID | guid}}</v-chip>
                </v-col>
                <v-col cols="12" md="4" class="info-block" v-if="model.invoiceOnly">
                  <p class="caption ecgray--text text--darken-2">{{$t('InvoiceOnly')}}</p>
                  <p>{{$t("Yes")}}</p>
                </v-col>
                <v-col cols="12" md="4" class="info-block">
                  <p class="caption ecgray--text text--darken-2">{{$t('Terminal')}}</p>
                  <p>{{model.terminalName}}</p>
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
                <v-row class="pt-2 info-container" v-if="model.lastError">
                  <v-col cols="12" md="4" class="info-block">
                    <p class="caption error--text text--darken-2">{{$t('LastError')}}</p>
                    <p>{{model.lastError || '-'}}</p>
                  </v-col>
                  <v-col cols="12" md="4" class="info-block">
                    <p class="caption error--text text--darken-2">{{$t('LastErrorCorrelationID')}}</p>
                    <p>{{model.lastErrorCorrelationID || '-'}}</p>
                  </v-col>
                  <v-col cols="12" md="4" class="info-block" v-if="model.failedAttemptsCount && model.failedAttemptsCount > 0">
                    <p class="caption ecgray--text text--darken-2">{{$t('FailedAttemptsCount')}}</p>
                    <p>{{model.failedAttemptsCount}}</p>
                  </v-col>
                </v-row>
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
          <amount-details :model="model" amount-key="transactionAmount" :currency="model.$currency || model.currency"></amount-details>
          <v-card flat class="my-2" v-if="model.dealDetails && model.dealDetails.items.length > 0">
            <v-card-title
              class="py-3 ecdgray--text subtitle-2 text-uppercase info-block-title"
            >{{$t("Items")}}</v-card-title>
            <v-divider></v-divider>
            <v-card-text>
              <transaction-items-list :items="model.dealDetails.items" :currency="model.$currency || model.currency"></transaction-items-list>
            </v-card-text>
          </v-card>

          <deal-details
            :model="model.dealDetails"
            :consumer-name="model.creditCardDetails ? model.creditCardDetails.cardOwnerName : null"
          ></deal-details>

          <payment-details v-if="model.paymentDetails && model.paymentDetails.length > 0" :model="model.paymentDetails"></payment-details>
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
      <v-tab-item key="financialItems" v-if="model">
        <invoices-list v-if="model.invoiceOnly && financialItems" :invoices="financialItems" class="pt-4 pb-2"></invoices-list>
        <transactions-list v-else-if="financialItems" :transactions="financialItems" class="pt-4 pb-2"></transactions-list>
      </v-tab-item>
    </v-tabs>
  </v-flex>
</template>

<script>
export default {
  components: {
    BillingScheduleString: () =>
      import("../../components/billing-deals/BillingScheduleString"),
    TransactionsList: () =>
      import("../../components/transactions/TransactionsList"),
    TransactionItemsList: () =>
      import("../../components/transactions/TransactionItemsList"),
    DealDetails: () => import("../../components/details/DealDetails"),
    AmountDetails: () => import("../../components/details/AmountDetails"),
    BankPaymentDetails: () => import("../../components/details/BankPaymentDetails"),
    CreditCardDetails: () =>
      import("../../components/details/CreditCardDetails"),
    PaymentDetails: () => import("../../components/details/PaymentDetails"),
    BillingDealPauseDialog: () =>
      import("../../components/billing-deals/BillingDealPauseDialog"),
    InvoicesList: () =>
      import("../../components/invoicing/InvoicesList"),
  },
  data() {
    return {
      model: null,
      financialItems: null, //e.g invoices or transactions
      financialItemsFilter: {
        take: this.$appConstants.config.ui.defaultTake,
        skip: 0,
        billingDealID: this.$route.params.id
      },
      numberOfRecords: 0,
      pauseDialog: false
    };
  },
  methods: {
    async switchBillingDeal() {
      let opResult = {};
      if(this.model.active) {
        opResult = await this.$api.billingDeals.disableBillingDeals(
          [this.$route.params.id]
        );
      } else {
        opResult = await this.$api.billingDeals.activateBillingDeals(
          [this.$route.params.id]
        );
      }
      if (opResult.status === "success") {
        this.model.active = !this.model.active;
        await this.initThreeDotMenu();
      }else{
        this.$toasted.show(opResult ? opResult.message : this.$t("SomethingWentWrong"), {
          type: "error"
        });
      }
    },
    async billingPausedChanged($event){
      if ($event.paused){
        this.model.pausedFrom = $event.dateFrom;
        this.model.pausedTo = $event.dateTo;
      }else{
        this.model.pausedFrom = this.model.pausedTo = null;
      }
      this.model.paused = $event.paused;
      this.initThreeDotMenu();
    },
    async initThreeDotMenu() {
      if(!this.model){
        return;
      }
      let newHeader = {
        threeDotMenu: [
          {
            text: this.$t("Edit"),
            fn: () => {
              this.$router.push({
                name: "EditBillingDeal",
                id: this.$route.params.id
              });
            }
          }
        ],
        text: {translate: true, value: this.model.invoiceOnly ? "InvoiceOnlyBillingDeal" : "BillingDeal"}
      };

      if(this.model.$nextScheduledTransaction || this.model.nextScheduledTransaction){
        newHeader.threeDotMenu.push({
          text: this.$t("Pause"),
          fn: () => {
            this.pauseDialog = true;
          }
        });
        newHeader.threeDotMenu.push({
          text: this.model.active ? this.$t("Deactivate") : this.$t("Activate"),
          fn: () => {
            this.switchBillingDeal();
          }
        });
      }
      this.$store.commit("ui/changeHeader", { value: newHeader });
    }
  },
  async mounted() {
    this.model = await this.$api.billingDeals.getBillingDeal(
      this.$route.params.id
    );

    if (!this.model) {
      return this.$router.push({ name: "BillingDeals" });
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

    await this.initThreeDotMenu();
  },
  /** Header is initialized in mounted but since components are cached (keep-alive) it's required to
    manually update menu on route change to make sure header has correct value*/
  beforeRouteEnter(to, from, next) {
    next(vm => {
      vm.initThreeDotMenu();
    });
  },
};
</script>