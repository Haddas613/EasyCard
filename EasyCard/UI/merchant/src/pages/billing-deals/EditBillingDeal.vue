<template>
  <v-card width="100%" flat color="ecbg">
    <v-card-text>
      <billing-deal-form :data="model" v-on:ok="updateBilling($event)" class="px-4" v-if="model"></billing-deal-form>
    </v-card-text>
  </v-card>
</template>

<script>
import BillingDealForm from "../../components/billing-deals/BillingDealForm";

export default {
  components: { BillingDealForm },
  data() {
    return {
      model: null
    };
  },
  methods: {
    async updateBilling(data) {
      let result = data.invoiceOnly ? 
        await this.$api.billingDeals.updateBillingDealInvoice(this.$route.params.id, data)
        : await this.$api.billingDeals.updateBillingDeal(this.$route.params.id, data);

      if (!this.$apiSuccess(result)) return;
      this.$router.push({ name: "BillingDeal", params: { id: this.$route.params.id } })
    }
  },
  async mounted () {
    let result = await this.$api.billingDeals.getBillingDeal(this.$route.params.id, true);

    if(!result){
      this.$router.push({ name: "BillingDeals" })
    }
    
    this.model = result;
  },
};
</script>