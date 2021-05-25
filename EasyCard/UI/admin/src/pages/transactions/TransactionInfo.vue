<template>
  <v-flex>
    <v-tabs grow color="primary" v-model="tab">
      <v-tab key="info">{{$t("Info")}}</v-tab>
      <v-tab key="history">{{$t("History")}}</v-tab>
    </v-tabs>
    <v-tabs-items v-model="tab" class="bg-ecbg">
      <v-tab-item key="info">
        <div v-if="model">
          <transaction-printout ref="printout" :transaction="model"></transaction-printout>
          <transaction-slip-dialog ref="slipDialog" :transaction="model" :show.sync="transactionSlipDialog"></transaction-slip-dialog>
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
                  <p class="error--text">
                     <router-link link :to="{name: 'EditTerminal', params: {id: model.$terminalID || model.terminalID}}">
                       {{model.terminalName}}
                    </router-link>
                  </p>
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
          <amount-details :model="model" amount-key="transactionAmount"></amount-details>
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
          <credit-card-details :model="model.creditCardDetails"></credit-card-details>

          <installment-details v-if="isInstallmentTransaction" :model="model"></installment-details>
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
          <shva-transaction-details :model="model.shvaTransactionDetails"></shva-transaction-details>
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
      </v-tab-item>
      <v-tab-item key="history">
        <div v-if="model">
          <transaction-history :transaction-id="model.$paymentTransactionID"></transaction-history>
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
    CreditCardDetails: () =>
      import("../../components/details/CreditCardDetails"),
    ShvaTransactionDetails: () =>
      import("../../components/details/ShvaTransactionDetails"),
    InstallmentDetails: () =>
      import("../../components/details/InstallmentDetails"),
    TransactionHistory: () =>
      import("../../components/transactions/TransactionHistory"),
    TransactionPrintout: () =>
      import("../../components/printouts/TransactionPrintout"),
    TransactionSlipDialog: () =>
      import("../../components/transactions/TransactionSlipDialog")
  },
  data() {
    return {
      model: null,
      quickStatusesColors: {
        Pending: "primary--text",
        None: "ecgray--text",
        Completed: "success--text",
        Failed: "error--text",
        Canceled: "accent--text"
      },
      tab: "info",
      transactionSlipDialog: false
    };
  },
  async mounted() {
    this.model = await this.$api.transactions.getTransaction(
      this.$route.params.id
    );

    if (!this.model) {
      return this.$router.push({ name: "Transactions" });
    }

    var threeDotMenu = [{
        text: this.$t("Print"),
        fn: () => {
          this.$refs.printout.print();
        }
      }
    ];

    if(this.model.$status == 'completed' && this.model.$jDealType == 'J4'){
      threeDotMenu.push({
        text: this.$t("SendTransactionSlipEmail"),
        fn: () => {
          this.transactionSlipDialog = true;
        }
      });
    }

    this.$store.commit("ui/changeHeader", {
      value: {
        threeDotMenu: threeDotMenu
      }
    });
  },
  methods: {
    async transmit() {
      let operation = await this.$api.transmissions.transmit({
        terminalID: this.model.terminalID,
        paymentTransactionIDs: [this.model.$paymentTransactionID]
      });
      
      //400
      if(!operation || (operation && operation.status == "error")){
        this.$toasted.show(operation ? operation.message : this.$t("SomethingWentWrong"), {
          type: "error"
        });
        return;
      }
      let opResult = operation[0];
      
      if (
        opResult.paymentTransactionID == this.$route.params.id &&
        opResult.transmissionStatus == "Transmitted"
      ) {
        this.$toasted.show(this.$t("TransactionTransmitted"), {
          type: "success",
          duration: 5000
        });
        let tr = await this.$api.transactions.getTransaction(
          this.$route.params.id
        );
        this.model.quickStatus = tr.quickStatus;
        this.model.allowTransmission = false;
      }
    },
    async cancelTransmission() {
      let operation = await this.$api.transmissions.cancelTransmission({
        terminalID: this.model.terminalID,
        paymentTransactionID: this.model.$paymentTransactionID
      });
      
      //400
      if(!operation || (operation && operation.status == "error")){
        this.$toasted.show(operation ? operation.message : this.$t("SomethingWentWrong"), {
          type: "error"
        });
        return;
      }
      let opResult = operation[0];

      if (
        opResult.paymentTransactionID == this.$route.params.id &&
        opResult.transmissionStatus == "Transmitted"
      ) {
        this.$toasted.show(this.$t("TransactionTransmitted"), {
          type: "success",
          duration: 5000
        });
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