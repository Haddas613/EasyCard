<template>
  <v-card width="100%" flat color="ecbg">
    <v-card-text class="px-0">
      <billing-deal-form :data="model" class="px-4" v-on:ok="createBillingDeal($event)"></billing-deal-form>
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
          dealDescription: null,
          items: []
        },
        billingSchedule: {}
      }
    };
  },
  methods: {
    async createBillingDeal(data) {
      let result = await this.$api.billingDeals.createBillingDeal(data);
      //server errors will be displayed automatically
      if (!result) return;

      if (result.status === "success") {
        this.$router.push({ name: "BillingDeals" });
      } else {
        this.$toasted.show(result.message, { type: "error" });
      }
    }
  },
};
</script>

<style lang="scss" scoped>
</style>