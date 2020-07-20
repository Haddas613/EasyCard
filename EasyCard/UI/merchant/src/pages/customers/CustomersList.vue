<template>
  <v-flex width="100%">
    <v-card width="100%" flat>
      <v-card-title>{{$t('Customers')}}</v-card-title>
    </v-card>
    <customers-list :show-previously-charged="false" v-on:ok="customerClicked($event)"></customers-list>
  </v-flex>
</template>

<script>
import CustomersList from "../../components/customers/CustomersList";

export default {
  components: { CustomersList },
  data() {
    return {
      totalAmount: 0,
      customers: [],
      loading: true,
      dictionaries: {}
    };
  },
  methods: {
    customerClicked(customer) {
      this.$router.push({
        name: "Customer",
        params: { id: customer.consumerID }
      });
    }
  },
  async mounted() {}
};
</script>

<style lang="scss" scoped>
</style>