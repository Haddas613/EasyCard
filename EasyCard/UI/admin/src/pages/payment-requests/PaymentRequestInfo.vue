<template>
  <v-flex>
    <div v-if="model">
      <v-card flat class="mb-2">
        <v-card-title class="py-3 ecdgray--text subtitle-2 text-uppercase">{{$t('GeneralInfo')}}</v-card-title>
        <v-divider></v-divider>
        <v-card-text class="body-1 black--text" v-if="model">
          <payment-request-history v-if="model.history && model.history.length" :data="model.history"></payment-request-history>
          <v-row class="info-container">
            <v-col cols="12" md="4" class="info-block">
              <p class="caption ecgray--text text--darken-2">{{$t('PaymentRequestID')}}</p>
              <v-chip color="primary" small>{{model.$paymentRequestID | guid}}</v-chip>
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
              <p class="caption ecgray--text text--darken-2">{{$t('TransactionID')}}</p>
              <router-link
                v-if="model.paymentTransactionID"
                class="primary--text"
                link
                :to="{name: 'Transaction', params: {id: model.$paymentTransactionID}}"
              >
                <small>{{(model.dealDetails.consumerID || '-') | guid}}</small>
              </router-link>
              <p v-else>-</p>
            </v-col>
            <v-col cols="12" md="4" class="info-block">
              <p class="caption ecgray--text text--darken-2">{{$t('Created')}}</p>
              <p>{{model.$paymentRequestTimestamp | ecdate('LLLL')}}</p>
            </v-col>
            <v-col cols="12" md="4" class="info-block">
              <p class="caption ecgray--text text--darken-2">{{$t('DueDate')}}</p>
              <p>{{model.$dueDate | ecdate('LLLL')}}</p>
            </v-col>
            <v-col cols="12" md="4" class="info-block">
              <p class="caption ecgray--text text--darken-2 py-1">
                {{$t('PaymentRequestURL')}} 
              </p>
              <v-chip v-if="model.paymentRequestUrl" class="mb-1" @click="$copyToClipboard(model.paymentRequestUrl)" small color="primary">{{$t("Copy")}}</v-chip>
              <p v-else>-</p>
              <!-- <a role="button" target="_blank" :href="model.paymentRequestUrl">
                <small>{{model.paymentRequestUrl}}</small>
              </a> -->
            </v-col>
          </v-row>
        </v-card-text>
      </v-card>
      <amount-details :model="model" amount-key="paymentRequestAmount"></amount-details>
      <amount-details title="UserPaid" :currency="model.$currency" v-if="model.userPaidDetails" :model="model.userPaidDetails" amount-key="transactionAmount"></amount-details>
      <v-card flat class="my-2" v-if="model.dealDetails && model.dealDetails.items.length > 0">
        <v-card-title
          class="py-3 ecdgray--text subtitle-2 text-uppercase info-block-title"
        >{{$t("Items")}}</v-card-title>
        <v-divider></v-divider>
        <v-card-text>
          <transaction-items-list
            :items="model.dealDetails.items"
          ></transaction-items-list>
        </v-card-text>
      </v-card>
      <deal-details
        :model="model.dealDetails"
        :consumer-name="model.creditCardDetails ? model.creditCardDetails.cardOwnerName : null"
      ></deal-details>
      <installment-details v-if="model.installmentDetails" :model="model"></installment-details>
    </div>
  </v-flex>
</template>

<script>
export default {
  components: {
    TransactionItemsList: () => import("../../components/transactions/TransactionItemsList"),
    PaymentRequestHistory: () => import("../../components/payment-requests/PaymentRequestHistory"),
    DealDetails: () => import("../../components/details/DealDetails"),
    AmountDetails: () => import("../../components/details/AmountDetails"),
    InstallmentDetails: () => import("../../components/details/InstallmentDetails")
  },
  data() {
    return {
      model: null,
      numberOfRecords: 0
    };
  },
  async mounted() {
    this.model = await this.$api.paymentRequests.getPaymentRequest(
      this.$route.params.id
    );

    if (!this.model) {
      return this.$router.push({name: "PaymentRequests"});
    }
  }
};
</script>