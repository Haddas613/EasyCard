<template>
  <v-card width="100%" flat color="ecbg">
    <v-card-text class="px-0">
      <billing-deal-form :data="model" class="px-4"></billing-deal-form>
    </v-card-text>
  </v-card>
</template>

<script>
export default {
  components: {
    BillingDealForm: () =>
      import("../../components/billing-deals/BillingDealForm"),
  },
  data() {
    return {
      model: {
        terminalID: null,
        currency: null,
        creditCardToken: null,
        transactionAmount: 0.0,
        numberOfPayments: 0,
        totalAmount: 0,
        dealDetails: {
          dealReference: null,
          consumerEmail: null,
          consumerPhone: null,
          consumerID: null,
          dealDescription: null
        },
        billingSchedule: {}
      }
    };
  }
};
</script>

<style lang="scss" scoped>
</style>