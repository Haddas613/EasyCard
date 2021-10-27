<template>
  <v-card width="100%" flat color="ecbg">
    <v-card-text class="px-0">
      <customer-form :data="model" v-on:ok="createCustomer($event)" class="px-4" v-if="model"></customer-form>
    </v-card-text>
  </v-card>
</template>

<script>
export default {
  components: {
    CustomerForm: () => import("../../components/customers/CustomerForm")
  },
  data() {
    return {
      model: {
        terminalID: null,
        consumerName: null,
        consumerPhone: null,
        consumerEmail: null,
        consumerAddress: null,
        externalReference: null,
        origin: null
      },
      loading: false
    };
  },
  methods: {
    async createCustomer(data) {
      if (this.loading) return;

      this.loading = true;
      let result = await this.$api.consumers.createConsumer(data);
      this.loading = false;
      if (!this.$apiSuccess(result)) return;

      this.$router.push({
        name: "Customer",
        params: { id: result.entityReference }
      });
    }
  }
};
</script>