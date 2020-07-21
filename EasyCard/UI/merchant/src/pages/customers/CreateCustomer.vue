<template>
  <v-card width="100%" flat color="ecbg">
    <v-card-title class="hidden-sm-and-down">{{$t("CreateCustomer")}}</v-card-title>
    <v-card-text class="px-0">
      <customer-form :data="model" v-on:ok="createCustomer($event)" class="px-4" v-if="model"></customer-form>
    </v-card-text>
  </v-card>
</template>

<script>
import CustomerForm from "../../components/customers/CustomerForm";

export default {
  components: { CustomerForm },
  data() {
    return {
      model: {
          terminalID: null,
          consumerName: null,
          consumerPhone: null,
          consumerEmail: null,
          consumerAddress: null,
      }
    };
  },
  methods: {
    async createCustomer(data) {
        
      let result = await this.$api.consumers.createConsumer(data);
      //server errors will be displayed automatically
      if (!result) return;

      if (result.status === "success") {
        this.$router.push("/admin/customers/list");
      } else {
        this.$toasted.show(result.message, { type: "error" });
      }
    }
  }
};
</script>

<style lang="scss" scoped>
</style>