<template>
  <v-card width="100%" flat color="ecbg">
    <v-card-text class="px-0">
      <merchant-form :data="model" v-on:ok="createMerchant($event)" class="px-4" v-if="model"></merchant-form>
    </v-card-text>
  </v-card>
</template>

<script>
// import MerchantForm from "../../components/merchants/MerchantForm";

export default {
  components: { MerchantForm },
  data() {
    return {
      model: {
          terminalID: null,
          merchantName: null,
          merchantPhone: null,
          merchantEmail: null,
          merchantAddress: null,
      }
    };
  },
  methods: {
    async createMerchant(data) {
        
      let result = await this.$api.merchants.createConsumer(data);
      //server errors will be displayed automatically
      if (!result) return;

      if (result.status === "success") {
        this.$router.push("/admin/merchants/list");
      } else {
        this.$toasted.show(result.message, { type: "error" });
      }
    }
  }
};
</script>

<style lang="scss" scoped>
</style>