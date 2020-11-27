<template>
  <v-flex>
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
                  <p class="caption ecgray--text text--darken-2">{{$t('ID')}}</p>
                  <v-chip color="primary" small>{{model.$billingDealID | guid}}</v-chip>
                </v-col>
                <v-col cols="12" md="4" class="info-block">
                  <p class="caption ecgray--text text--darken-2">{{$t('Terminal')}}</p>
                  <p>{{terminalName}}</p>
                </v-col>
                <v-col cols="12" md="4" class="info-block">
                  <p class="caption ecgray--text text--darken-2">{{$t('Created')}}</p>
                  <p>{{model.$billingDealTimestamp | ecdate('LLLL')}}</p>
                </v-col>
              </v-row>
              <v-row class="info-container">
                <billing-schedule-string
                  :schedule="model.billingSchedule"
                  replacement-text="ScheduleIsNotDefined"
                ></billing-schedule-string>
              </v-row>
            </v-card-text>
          </v-card>
          <v-card flat class="my-2">
            <v-card-title
              class="py-3 ecdgray--text subtitle-2 text-uppercase info-block-title"
            >{{$t("TransactionDetails")}}</v-card-title>
            <v-divider></v-divider>
            <v-card-text>
              <v-row class="info-container body-1 black--text">
                <v-col cols="6" md="4" class="info-block">
                  <p class="caption ecgray--text text--darken-2">{{$t('VAT')}}</p>
                  <p>{{model.vatRate * 100}}%</p>
                </v-col>
                <v-col cols="6" md="4" class="info-block">
                  <p class="caption ecgray--text text--darken-2">{{$t('VATAmount')}}</p>
                  <p>{{model.vatTotal | currency(model.$currency)}}</p>
                </v-col>
                <v-col cols="6" md="4" class="info-block">
                  <p class="caption ecgray--text text--darken-2">{{$t('NetAmount')}}</p>
                  <p>{{model.netTotal | currency(model.$currency)}}</p>
                </v-col>
              </v-row>
              <v-row class="info-container body-1 black--text">
                <!-- <v-col cols="12" md="4" class="info-block">
                  <p class="caption ecgray--text text--darken-2">{{$t('NumberOfPayments')}}</p>
                  <p>{{model.numberOfPayments}}</p>
                </v-col>-->
                <v-col cols="12" md="4" class="info-block">
                  <p class="caption ecgray--text text--darken-2">{{$t('TransactionAmount')}}</p>
                  <p>{{model.transactionAmount | currency(model.$currency)}}</p>
                </v-col>
                <v-col cols="12" md="4" class="info-block">
                  <p class="caption ecgray--text text--darken-2">{{$t('TotalAmount')}}</p>
                  <p>{{model.totalAmount | currency(model.$currency)}}</p>
                </v-col>
              </v-row>
            </v-card-text>
          </v-card>
          <v-card flat class="my-2" v-if="model.dealDetails && model.dealDetails.items.length > 0">
            <v-card-title
              class="py-3 ecdgray--text subtitle-2 text-uppercase info-block-title"
            >{{$t("Items")}}</v-card-title>
            <v-divider></v-divider>
            <v-card-text>
              <transaction-items-list :items="model.dealDetails.items"></transaction-items-list>
            </v-card-text>
          </v-card>
          <v-card flat class="my-2">
            <v-card-title
              class="py-3 ecdgray--text subtitle-2 text-uppercase info-block-title"
            >{{$t("CreditCardDetails")}}</v-card-title>
            <v-divider></v-divider>
            <v-card-text>
              <v-row class="info-container body-1 black--text" v-if="model">
                <template v-if="model.dealDetails.consumerID">
                  <v-col cols="12" md="4" class="info-block">
                    <p class="caption ecgray--text text--darken-2">{{$t('CustomerID')}}</p>
                    <router-link
                      class="primary--text"
                      link
                      :to="{name: 'Customer', params: {id: model.dealDetails.consumerID}}"
                    >
                      <small>{{(model.dealDetails.consumerID || '-') | guid}}</small>
                    </router-link>
                  </v-col>
                  <v-col cols="12" md="4" class="info-block">
                    <p class="caption ecgray--text text--darken-2">{{$t('CustomerEmail')}}</p>
                    <p>{{(model.dealDetails.consumerEmail || '-')}}</p>
                  </v-col>
                  <v-col cols="12" md="4" class="info-block">
                    <p class="caption ecgray--text text--darken-2">{{$t('CustomerPhone')}}</p>
                    <p>{{(model.dealDetails.consumerPhone || '-')}}</p>
                  </v-col>
                </template>
                <v-col cols="12" md="4" class="info-block">
                  <p class="caption ecgray--text text--darken-2">{{$t('CreditCardToken')}}</p>
                  <p>{{(model.creditCardToken || '-') | guid}}</p>
                </v-col>
                <v-col cols="12" md="4" class="info-block">
                  <p class="caption ecgray--text text--darken-2">{{$t('CardNumber')}}</p>
                  <p>{{model.creditCardDetails.cardNumber}}</p>
                </v-col>
                <v-col cols="12" md="4" class="info-block">
                  <p class="caption ecgray--text text--darken-2">{{$t('CardExpiration')}}</p>
                  <p>{{model.creditCardDetails.cardExpiration}}</p>
                </v-col>
                <v-col cols="12" md="4" class="info-block">
                  <p class="caption ecgray--text text--darken-2">{{$t('CardOwnerName')}}</p>
                  <p>{{model.creditCardDetails.cardOwnerName || '-'}}</p>
                </v-col>
                <v-col cols="12" md="4" class="info-block">
                  <p class="caption ecgray--text text--darken-2">{{$t('CardOwnerNationalID')}}</p>
                  <p>{{model.creditCardDetails.cardOwnerNationalID || '-'}}</p>
                </v-col>
              </v-row>
            </v-card-text>
          </v-card>
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
                  <p>{{model.invoiceDetails.isendCCTo || '-'}}</p>
                </v-col>
              </v-row>
            </v-card-text>
          </v-card>
        </div>
      </v-tab-item>

      <v-tab key="transactions">{{$t("Transactions")}}</v-tab>
      <v-tab-item key="transactions">
        <transactions-list v-if="transactions" :transactions="transactions" class="pt-4 pb-2"></transactions-list>
      </v-tab-item>
    </v-tabs>
  </v-flex>
</template>

<script>
export default {
  data() {
    return {
      model: null,
      terminalName: "-",
      transactions: null,
      transactionsFilter: {
        take: 100,
        skip: 0,
        billingDealID: this.$route.params.id
      },
      numberOfRecords: 0
    };
  },
  components: {
    BillingScheduleString: () =>
      import("../../components/billing-deals/BillingScheduleString"),
    TransactionsList: () =>
      import("../../components/transactions/TransactionsList"),
    TransactionItemsList: () => import("../../components/transactions/TransactionItemsList"),
  },
  async mounted() {
    this.model = await this.$api.billingDeals.getBillingDeal(
      this.$route.params.id
    );

    if (!this.model) {
      return this.$router.push("/admin/billing-deals/list");
    }

    let data =
      (await this.$api.transactions.get(this.transactionsFilter)) || {};
    this.transactions = data.data || [];
    this.numberOfRecords = data.numberOfRecords || 0;

    let terminals = (await this.$api.terminals.getTerminals()).data;
    let usedTerminal = this.lodash.find(
      terminals,
      t => t.terminalID == this.model.$terminalID
    );
    if (usedTerminal) {
      this.terminalName = usedTerminal.label;
    } else {
      this.terminalName = this.$t("NotAccessible");
    }

    let $dictionaries = await this.$api.dictionaries.$getTransactionDictionaries();

    if (this.model.invoiceDetails && this.model.invoiceDetails.invoiceType) {
      this.model.invoiceDetails.invoiceType =
        $dictionaries.invoiceTypeEnum[this.model.invoiceDetails.invoiceType];
    }

    this.$store.commit("ui/changeHeader", {
      value: {
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
        ]
      }
    });
  }
};
</script>