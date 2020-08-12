<template>
  <v-flex>
    <v-row no-gutters>
      <v-col cols="12" md="6" lg="6" xl="6">
        <v-card class="mx-2 my-2" :loading="model == null">
          <v-card-title class="py-2">
            <v-row no-gutters class="py-0">
              <span class="pt-2 ecdgray--text subtitle-2 text-uppercase">{{$t('GeneralInfo')}}</span>
            </v-row>
          </v-card-title>
          <v-divider></v-divider>
          <v-card-text class="body-1 black--text" v-if="model">
            <div class="info-block">
              <p class="caption ecgray--text text--darken-2">{{$t('ID')}}</p>
              <p>{{model.paymentTransactionID}}</p>
            </div>
            <div class="info-block">
              <p class="caption ecgray--text text--darken-2">{{$t('Terminal')}}</p>
              <p>{{terminalName}}</p>
            </div>
            <div class="info-block">
              <p class="caption ecgray--text text--darken-2">{{$t('Status')}}</p>
              <p v-bind:class="quickStatusesColors[model.quickStatus]">{{model.quickStatus || '-'}}</p>
            </div>
            <div class="info-block">
              <p class="caption ecgray--text text--darken-2">{{$t('TransactionTime')}}</p>
              <p>{{model.$transactionTimestamp | ecdate('LLLL')}}</p>
            </div>
            <div class="info-block">
              <p class="caption ecgray--text text--darken-2">{{$t('TransmissionTime')}}</p>
              <p>
                <span
                  v-if="model.transmittedTimestamp"
                >{{model.transmittedTimestamp | ecdate('LLLL')}}</span>
                <span v-if="!model.transmittedTimestamp">-</span>
              </p>
            </div>
          </v-card-text>
        </v-card>
      </v-col>
      <v-col cols="12" md="6" lg="6" xl="6">
        <v-card class="mx-2 my-2" :loading="model == null">
          <v-card-title class="py-2">
            <v-row no-gutters class="py-0">
              <span class="pt-2 ecdgray--text subtitle-2 text-uppercase">{{$t('CreditCardDetails')}}</span>
            </v-row>
          </v-card-title>
          <v-divider></v-divider>
          <v-card-text class="body-1 black--text" v-if="model">
            <div class="info-block">
              <p class="caption ecgray--text text--darken-2">{{$t('CreditCardToken')}}</p>
              <p>{{(model.creditCardToken || '-') | guid}}</p>
            </div>
            <div class="info-block">
              <p class="caption ecgray--text text--darken-2">{{$t('CardNumber')}}</p>
              <p>{{model.creditCardDetails.cardNumber}}</p>
            </div>
            <div class="info-block">
              <p class="caption ecgray--text text--darken-2">{{$t('CardExpiration')}}</p>
              <p>{{model.creditCardDetails.cardExpiration}}</p>
            </div>
            <div class="info-block">
              <p class="caption ecgray--text text--darken-2">{{$t('CardOwnerName')}}</p>
              <p>{{model.creditCardDetails.cardOwnerName || '-'}}</p>
            </div>
            <div class="info-block">
              <p class="caption ecgray--text text--darken-2">{{$t('CardOwnerNationalID')}}</p>
              <p>{{model.creditCardDetails.cardOwnerNationalID || '-'}}</p>
            </div>
          </v-card-text>
        </v-card>
      </v-col>

      <v-col cols="12" md="6" lg="6" xl="6">
        <v-card class="mx-2 my-2" :loading="model == null">
          <v-card-title class="py-2">
            <v-row no-gutters class="py-0">
              <span
                class="pt-2 ecdgray--text subtitle-2 text-uppercase"
              >{{$t('InstallmentDetails')}}</span>
            </v-row>
          </v-card-title>
          <v-divider></v-divider>
          <v-card-text class="body-1 black--text" v-if="model">
            <div class="info-block">
              <p class="caption ecgray--text text--darken-2">{{$t('NumberOfPayments')}}</p>
              <p>{{model.numberOfPayments}}</p>
            </div>
            <div class="info-block">
              <p class="caption ecgray--text text--darken-2">{{$t('InitialPaymentAmount')}}</p>
              <p>{{model.initialPaymentAmount | currency(model.$currency)}}</p>
            </div>
            <div class="info-block">
              <p class="caption ecgray--text text--darken-2">{{$t('InstallmentPaymentAmount')}}</p>
              <p>{{model.installmentPaymentAmount | currency(model.$currency)}}</p>
            </div>
            <div class="info-block">
              <p class="caption ecgray--text text--darken-2">{{$t('TotalAmount')}}</p>
              <p>{{model.totalAmount | currency(model.$currency)}}</p>
            </div>
          </v-card-text>
        </v-card>
      </v-col>
      <v-col cols="12" md="6" lg="6" xl="6">
        <v-card class="mx-2 my-2" :loading="model == null">
          <v-card-title class="py-2">
            <v-row no-gutters class="py-0">
              <span class="pt-2 ecdgray--text subtitle-2 text-uppercase">{{$t('Advanced')}}</span>
            </v-row>
          </v-card-title>
          <v-divider></v-divider>
          <v-card-text class="body-1 black--text" v-if="model">
            <div class="info-block">
              <p class="caption ecgray--text text--darken-2">{{$t('TransactionType')}}</p>
              <p>{{model.transactionType}}</p>
            </div>
            <div class="info-block">
              <p class="caption ecgray--text text--darken-2">{{$t('SpecialTransactionType')}}</p>
              <p>{{model.specialTransactionType}}</p>
            </div>
            <div class="info-block">
              <p class="caption ecgray--text text--darken-2">{{$t('JDealType')}}</p>
              <p>{{model.jDealType}}</p>
            </div>
            <div class="info-block">
              <p class="caption ecgray--text text--darken-2">{{$t('CardPresence')}}</p>
              <p>{{model.cardPresence}}</p>
            </div>
          </v-card-text>
        </v-card>
      </v-col>
    </v-row>
    <v-row no-gutters>
      <v-col cols="12" class="d-flex justify-end" v-if="!$vuetify.breakpoint.smAndDown">
        <v-btn class="mx-1" color="success">{{$t('Transmission')}}</v-btn>
        <v-btn color="red" class="white--text">{{$t('CancelTransmission')}}</v-btn>
      </v-col>
      <v-col cols="12" v-if="$vuetify.breakpoint.smAndDown">
        <v-btn class="mx-1" color="success">{{$t('Transmission')}}</v-btn>
        <v-spacer class="py-2"></v-spacer>
        <v-btn color="red">{{$t('CancelTransmission')}}</v-btn>
      </v-col>
    </v-row>
  </v-flex>
</template>

<script>
export default {
  data() {
    return {
      model: null,
      terminalName: "-",
      quickStatusesColors: {
        Pending: "ecgray--text",
        None: "",
        Completed: "success--text",
        Failed: "error--text"
      }
    };
  },
  async mounted() {
    this.model = await this.$api.transactions.getTransaction(
      this.$route.params.id
    );

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

    if (!this.model) {
      return this.$router.push("/admin/transactions/list");
    }
  }
};
</script>

<style lang="scss" scoped>
.info-block {
  p {
    font-size: 0.85rem;
  }
}
</style>