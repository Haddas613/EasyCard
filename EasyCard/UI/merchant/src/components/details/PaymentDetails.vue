<template>
  <v-card flat class="my-2">
    <v-card-title
      class="py-3 ecdgray--text subtitle-2 text-uppercase info-block-title"
    >{{$t("PaymentDetails")}}</v-card-title>
    <v-divider></v-divider>
    <v-card-text v-if="dictionaries">
      <fieldset v-for="d in model" :key="d.paymentType" class="ec-fieldset mb-2">
        <legend>
            <b>{{dictionaries.paymentTypeEnum[d.paymentType]}}</b>
        </legend>
        <v-flex class="d-flex px-2">
            <credit-card-payment-details :model="d" v-if="d.paymentType == appConstants.transaction.paymentTypes.card"></credit-card-payment-details>
            <cheque-payment-details :model="d" v-else-if="d.paymentType == appConstants.transaction.paymentTypes.cheque"></cheque-payment-details>
        </v-flex>
      </fieldset>
    </v-card-text>
  </v-card>
</template>

<script>
import appConstants from "../../helpers/app-constants";

export default {
  components: {
    CreditCardPaymentDetails: () => import("./CreditCardPaymentDetails"),
    ChequePaymentDetails: () => import("./ChequePaymentDetails"),
  },
  props: {
    model: {
      type: Array,
      required: true
    }
  },
  data() {
    return {
      dictionaries: null,
      appConstants: appConstants
    };
  },
  async mounted() {
    this.dictionaries = await this.$api.dictionaries.$getTransactionDictionaries();
  }
};
</script>