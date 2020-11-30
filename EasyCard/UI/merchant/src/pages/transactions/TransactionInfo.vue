<template>
  <v-flex>
    <div v-if="model">
      <v-card flat class="mb-2">
        <v-card-title class="py-3 ecdgray--text subtitle-2 text-uppercase">{{$t('GeneralInfo')}}</v-card-title>
        <v-divider></v-divider>
        <v-card-text>
          <v-row class="info-container body-1 black--text" v-if="model">
            <v-col cols="12" md="4" class="info-block">
              <p class="caption ecgray--text text--darken-2">{{$t('ID')}}</p>
              <v-chip color="primary" small>{{model.$paymentTransactionID | guid}}</v-chip>
            </v-col>
            <v-col cols="12" md="4" class="info-block">
              <p class="caption ecgray--text text--darken-2">{{$t('Terminal')}}</p>
              <p>{{terminalName}}</p>
            </v-col>
            <v-col cols="12" md="4" class="info-block">
              <p class="caption ecgray--text text--darken-2">{{$t('TransactionType')}}</p>
              <p>{{model.transactionType}}</p>
            </v-col>
            <v-col cols="12" md="4" class="info-block">
              <p class="caption ecgray--text text--darken-2">{{$t('Status')}}</p>
              <p
                v-bind:class="quickStatusesColors[model.quickStatus]"
              >{{$t(model.quickStatus || 'None')}}</p>
            </v-col>
            <v-col cols="12" md="4" class="info-block">
              <p class="caption ecgray--text text--darken-2">{{$t('TransactionTime')}}</p>
              <p>{{model.$transactionTimestamp | ecdate('LLLL')}}</p>
            </v-col>
            <v-col cols="12" md="4" class="info-block">
              <p class="caption ecgray--text text--darken-2">{{$t('TransmissionTime')}}</p>
              <p>
                <span
                  v-if="model.shvaTransactionDetails && model.shvaTransactionDetails.transmissionDate"
                >{{model.shvaTransactionDetails.transmissionDate | ecdate('LLLL')}}</span>
                <span v-if="!model.shvaTransactionDetails.transmissionDate">-</span>
              </p>
            </v-col>
            <v-col cols="12" md="4" class="info-block" v-if="model.invoiceID">
              <p class="caption ecgray--text text--darken-2">{{$t('InvoiceID')}}</p>
              <router-link
                class="primary--text"
                link
                :to="{name: 'Invoice', params: {id: model.invoiceID}}"
              >
                <small>{{(model.invoiceID || '-') | guid}}</small>
              </router-link>
            </v-col>
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
            <v-col cols="6" md="3" class="info-block">
              <p class="caption ecgray--text text--darken-2">{{$t('VAT')}}</p>
              <p>{{model.vatRate * 100}}%</p>
            </v-col>
            <v-col cols="6" md="3" class="info-block">
              <p class="caption ecgray--text text--darken-2">{{$t('VATAmount')}}</p>
              <p>{{model.vatTotal | currency(model.$currency)}}</p>
            </v-col>
            <v-col cols="6" md="3" class="info-block">
              <p class="caption ecgray--text text--darken-2">{{$t('NetAmount')}}</p>
              <p>{{model.netTotal | currency(model.$currency)}}</p>
            </v-col>
            <v-col cols="6" md="3" class="info-block">
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
      <v-card flat class="my-2" v-if="isInstallmentTransaction">
        <v-card-title
          class="py-3 ecdgray--text subtitle-2 text-uppercase info-block-title"
        >{{$t('InstallmentDetails')}}</v-card-title>
        <v-divider></v-divider>
        <v-card-text>
          <v-row class="info-container body-1 black--text" v-if="model">
            <v-col cols="12" md="4" class="info-block">
              <p class="caption ecgray--text text--darken-2">{{$t('NumberOfPayments')}}</p>
              <p>{{model.numberOfPayments}}</p>
            </v-col>
            <v-col cols="12" md="4" class="info-block">
              <p class="caption ecgray--text text--darken-2">{{$t('InitialPaymentAmount')}}</p>
              <p>{{model.initialPaymentAmount | currency(model.$currency)}}</p>
            </v-col>
            <v-col cols="12" md="4" class="info-block">
              <p class="caption ecgray--text text--darken-2">{{$t('InstallmentPaymentAmount')}}</p>
              <p>{{model.installmentPaymentAmount | currency(model.$currency)}}</p>
            </v-col>
            <v-col cols="12" md="4" class="info-block">
              <p class="caption ecgray--text text--darken-2">{{$t('TotalAmount')}}</p>
              <p>{{model.totalAmount | currency(model.$currency)}}</p>
            </v-col>
          </v-row>
        </v-card-text>
      </v-card>
      <v-card flat class="my-2">
        <v-card-title
          class="py-3 ecdgray--text subtitle-2 text-uppercase info-block-title"
        >{{$t('Advanced')}}</v-card-title>
        <v-divider></v-divider>
        <v-card-text>
          <v-row class="info-container body-1 black--text" v-if="model">
            <v-col cols="12" md="4" class="info-block">
              <p class="caption ecgray--text text--darken-2">{{$t('SpecialTransactionType')}}</p>
              <p>{{model.specialTransactionType}}</p>
            </v-col>
            <v-col cols="12" md="4" class="info-block">
              <p class="caption ecgray--text text--darken-2">{{$t('JDealType')}}</p>
              <p>{{model.jDealType}}</p>
            </v-col>
            <v-col cols="12" md="4" class="info-block">
              <p class="caption ecgray--text text--darken-2">{{$t('CardPresence')}}</p>
              <p>{{model.cardPresence}}</p>
            </v-col>
            <v-col cols="12" md="4" class="info-block">
              <p class="caption ecgray--text text--darken-2">{{$t('RejectionReason')}}</p>
              <p>{{model.rejectionReason}}</p>
            </v-col>
          </v-row>
        </v-card-text>
      </v-card>
      <v-card flat class="my-2">
        <v-card-title
          class="py-3 ecdgray--text subtitle-2 text-uppercase info-block-title"
        >{{$t('ShvaTransactionDetails')}}</v-card-title>
        <v-divider></v-divider>
        <v-card-text>
          <v-row class="info-container body-1 black--text">
            <v-col cols="12" md="4" class="info-block">
              <p class="caption ecgray--text text--darken-2">{{$t('ShvaDealID')}}</p>
              <p>{{model.shvaTransactionDetails.shvaDealID || '-'}}</p>
            </v-col>
            <v-col cols="12" md="4" class="info-block">
              <p class="caption ecgray--text text--darken-2">{{$t('ShvaShovarNumber')}}</p>
              <p>{{model.shvaTransactionDetails.shvaShovarNumber || '-'}}</p>
            </v-col>
            <v-col cols="12" md="4" class="info-block">
              <p class="caption ecgray--text text--darken-2">{{$t('ShvaTerminalID')}}</p>
              <p>{{model.shvaTransactionDetails.shvaTerminalID || '-'}}</p>
            </v-col>
            <v-col cols="12" md="4" class="info-block">
              <p class="caption ecgray--text text--darken-2">{{$t('ShvaTransmissionNumber')}}</p>
              <p>{{model.shvaTransactionDetails.shvaTransmissionNumber || '-'}}</p>
            </v-col>
            <v-col cols="12" md="4" class="info-block">
              <p class="caption ecgray--text text--darken-2">{{$t('Solek')}}</p>
              <p>{{model.shvaTransactionDetails.solek || '-'}}</p>
            </v-col>
          </v-row>
        </v-card-text>
      </v-card>
    </div>
    <v-row no-gutters v-if="model && model.allowTransmission" class="py-2">
      <v-col cols="12" class="d-flex justify-end" v-if="!$vuetify.breakpoint.smAndDown">
        <v-btn class="mx-1" color="primary" @click="transmit()">{{$t('Transmission')}}</v-btn>
        <v-btn
          color="red"
          class="white--text"
          outlined
          @click="cancelTransmission()"
        >{{$t('CancelTransmission')}}</v-btn>
      </v-col>
      <v-col cols="12" v-if="$vuetify.breakpoint.smAndDown" class="px-2 pb-2">
        <v-btn block color="primary" @click="transmit()">{{$t('Transmission')}}</v-btn>
        <v-spacer class="py-2"></v-spacer>
        <v-btn
          block
          color="red"
          class="white--text"
          outlined
          @click="cancelTransmission()"
        >{{$t('CancelTransmission')}}</v-btn>
      </v-col>
    </v-row>
  </v-flex>
</template>

<script>
export default {
  components: {
    TransactionItemsList: () =>
      import("../../components/transactions/TransactionItemsList")
  },
  data() {
    return {
      model: null,
      terminalName: "-",
      quickStatusesColors: {
        Pending: "primary--text",
        None: "ecgray--text",
        Completed: "success--text",
        Failed: "error--text",
        Canceled: "accent--text"
      }
    };
  },
  async mounted() {
    this.model = await this.$api.transactions.getTransaction(
      this.$route.params.id
    );

    if (!this.model) {
      return this.$router.push({ name: "Transactions" });
    }

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
  },
  methods: {
    async transmit() {
      let operation = await this.$api.transmissions.transmit({
        terminalID: this.model.$terminalID,
        paymentTransactionIDs: [this.model.$paymentTransactionID]
      });

      if (!operation || operation.numberOfRecords !== 1) return;
      let opResult = operation.data[0];

      if (
        opResult.paymentTransactionID == this.$route.params.id &&
        opResult.transmissionStatus == "Transmitted"
      ) {
        let tr = await this.$api.transactions.getTransaction(
          this.$route.params.id
        );
        this.model.quickStatus = tr.quickStatus;
        this.model.allowTransmission = false;
      }
    },
    async cancelTransmission() {
      let operation = await this.$api.transmissions.cancelTransmission({
        terminalID: this.model.$terminalID,
        paymentTransactionID: this.model.$paymentTransactionID
      });

      if (!operation || operation.numberOfRecords !== 1) return;
      let opResult = operation.data[0];

      if (
        opResult.paymentTransactionID == this.$route.params.id &&
        opResult.transmissionStatus == "Transmitted"
      ) {
        let tr = await this.$api.transactions.getTransaction(
          this.$route.params.id
        );
        this.model.quickStatus = tr.quickStatus;
        this.model.allowTransmission = false;
      }
    }
  },
  computed: {
    isInstallmentTransaction() {
      return (
        this.model.$transactionType === "installments" ||
        this.model.$transactionType === "credit"
      );
    }
  }
};
</script>