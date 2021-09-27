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
      
      if (!this.$apiSuccess(result)) return;
      this.$router.push({ name: "BillingDeal", params: { id: result.entityReference } });
    }
  },
};
</script>

<style lang="scss" scoped>
</style>