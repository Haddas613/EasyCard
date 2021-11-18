<template>
  <v-flex>
    <v-tabs grow color="primary" v-model="tab">
      <v-tab key="info">{{$t("Info")}}</v-tab>
      <v-tab key="integrations">{{$t("Integrations")}}</v-tab>
    </v-tabs>
    <v-tabs-items v-model="tab" class="bg-ecbg">
      <v-tab-item key="info">
        <div v-if="model">
          <v-card flat class="mb-2">
            <v-card-title class="py-3 ecdgray--text subtitle-2 text-uppercase">{{$t('GeneralInfo')}}</v-card-title>
            <v-divider></v-divider>
            <v-card-text class="body-1 black--text" v-if="model">
              <v-row class="info-container">
                <v-col cols="12" md="4" class="info-block">
                  <p class="caption ecgray--text text--darken-2">{{$t('InvoiceID')}}</p>
                  <v-chip color="primary" small>{{model.$invoiceID | guid}}</v-chip>
                </v-col>
                <v-col cols="12" md="4" class="info-block">
                  <p class="caption ecgray--text text--darken-2">{{$t('InvoiceNumber')}}</p>
                  <p>{{model.invoiceNumber || '-'}}</p>
                </v-col>
                <v-col cols="12" md="4" class="info-block">
                  <p class="caption ecgray--text text--darken-2">{{$t('TransactionID')}}</p>
                  <router-link
                    v-if="model.paymentTransactionID"
                    class="primary--text"
                    link
                    :to="{name: 'Transaction', params: {id: model.$paymentTransactionID}}"
                  >
                    <small>{{(model.dealDetails.consumerID || model.$paymentTransactionID) | guid}}</small>
                  </router-link>
                  <p v-else>-</p>
                </v-col>
              </v-row>
              <v-row class="info-container">
                <v-col cols="12" md="4" class="info-block">
                  <p class="caption ecgray--text text--darken-2">{{$t('Created')}}</p>
                  <p>{{model.$invoiceTimestamp | ecdate('LLLL')}}</p>
                </v-col>
                <v-col cols="12" md="4" class="info-block">
                  <p class="caption ecgray--text text--darken-2">{{$t('InvoiceDate')}}</p>
                  <p>{{model.$invoiceDate | ecdate('LLLL')}}</p>
                </v-col>
                <v-col cols="12" md="4" class="info-block">
                  <p class="caption ecgray--text text--darken-2">{{$t('Terminal')}}</p>
                  <p class="error--text">
                      <router-link link :to="{name: 'EditTerminal', params: {id: model.$terminalID || model.terminalID}}">
                        {{model.terminalName}}
                    </router-link>
                  </p>
                </v-col>
              </v-row>
              <v-row class="info-container">
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
              <v-row class="info-container">
                <v-col cols="12" md="4" class="info-block">
                  <p class="caption ecgray--text text--darken-2">{{$t('Status')}}</p>
                  <v-btn outlined color="success" x-small v-if="model.$status == 'sent'" :title="$t('ClickToDownload')" @click="downloadInvoicePDF(model.$invoiceID)">
                    {{$t(model.status)}}
                    <v-icon right color="red" size="1rem">mdi-file-pdf-outline</v-icon>
                  </v-btn>
                  <p
                    v-bind:class="statusColors[model.status]"
                    v-else
                  >{{$t(model.status || 'None')}}</p>
                </v-col>
              </v-row>
            </v-card-text>
          </v-card>
          <amount-details :model="model" amount-key="invoiceAmount"></amount-details>
          <installment-details v-if="isInstallmentTransaction" :model="model"></installment-details>
          <v-card flat class="my-2" v-if="model.dealDetails && model.dealDetails.items.length > 0">
            <v-card-title
              class="py-3 ecdgray--text subtitle-2 text-uppercase info-block-title"
            >{{$t("Items")}}</v-card-title>
            <v-divider></v-divider>
            <v-card-text>
              <transaction-items-list :items="model.dealDetails.items"></transaction-items-list>
            </v-card-text>
          </v-card>

          <deal-details
            :model="model.dealDetails"
            :consumer-name="model.creditCardDetails ? model.creditCardDetails.cardOwnerName : null"
          ></deal-details>
          <credit-card-details v-if="model.creditCardDetails" :model="model.creditCardDetails"></credit-card-details>
          <payment-details v-if="model.paymentDetails" :model="model.paymentDetails"></payment-details>
        </div>
      </v-tab-item>
      <v-tab-item key="integrations">
        <div v-if="model">
          <integration-logs-list :entity-id="model.$invoiceID"></integration-logs-list>
        </div>
      </v-tab-item>
    </v-tabs-items>
  </v-flex>
</template>

<script>
export default {
  components: {
    TransactionItemsList: () =>
      import("../../components/transactions/TransactionItemsList"),
    DealDetails: () => import("../../components/details/DealDetails"),
    AmountDetails: () => import("../../components/details/AmountDetails"),
    CreditCardDetails: () => import("../../components/details/CreditCardDetails"),
    PaymentDetails: () => import("../../components/details/PaymentDetails"),
    InstallmentDetails: () => import("../../components/details/InstallmentDetails"),
    IntegrationLogsList: () =>
      import("../../components/integration-logs/IntegrationLogsList")
  },
  data() {
    return {
      model: null,
      numberOfRecords: 0,
      statusColors: {
        Pending: "gray--text",
        None: "",
        Sent: "success--text",
        Sending: "primary--text",
        SendingFailed: "error--text"
      },
      tab: "info"
    };
  },
  methods: {
    async downloadInvoicePDF(invoiceID){
      let opResult = await this.$api.invoicing.downloadPDF(invoiceID);

      if(opResult.status === "success" && opResult.downloadLinks){
        for(var link of opResult.downloadLinks){
          window.open(link, '_blank');
        }
      }
    }
  },
  async mounted() {
    this.model = await this.$api.invoicing.getInvoice(this.$route.params.id);

    if (!this.model) {
      return this.$router.push({ name: "Invoices" });
    }

    let $dictionaries = await this.$api.dictionaries.$getTransactionDictionaries();

    if (
      this.model.invoiceDetails.sendCCTo &&
      this.model.invoiceDetails.sendCCTo.length > 0
    ) {
      this.model.invoiceDetails.sendCCTo = this.model.invoiceDetails.sendCCTo.join(
        ","
      );
    }

    if (this.model.invoiceDetails.invoiceType) {
      this.model.invoiceDetails.invoiceType =
        $dictionaries.invoiceTypeEnum[this.model.invoiceDetails.invoiceType];
    }
  },
  computed: {
    isInstallmentTransaction() {
      return (
        this.model.transactionType === "installments" ||
        this.model.transactionType === "credit"
      );
    }
  }
};
</script>