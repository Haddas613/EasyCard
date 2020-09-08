<template>
  <v-flex width="100%">
    <v-card width="100%" flat>
      <v-card-title class="hidden-sm-and-down">{{$t('Customers')}}</v-card-title>
    </v-card>
    <customers-list :show-previously-charged="false" v-on:ok="customerClicked($event)" :filter-by-terminal="true"></customers-list>
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
  async mounted() {
    this.$store.commit("ui/changeHeader", {
      value: {
        threeDotMenu: [
          {
            text: this.$t("CreateCustomer"),
            fn: () => {this.$router.push({name: 'CreateCustomer'});}
          }
        ]
      }
    });
  }
};
</script>

<style lang="scss" scoped>
</style>