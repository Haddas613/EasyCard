<template>
  <v-card width="100%" flat color="ecbg">
    <v-card-text>
      <merchant-form :data="model" v-on:ok="createMerchant($event)" class="px-4" v-if="model"></merchant-form>
    </v-card-text>
  </v-card>
</template>

<script>
 import MerchantForm from "../../components/merchants/MerchantForm";

export default {
  components: { MerchantForm },
  data() {
    return {
      model: {
          businessName: null,
          marketingName: null,
          businessID: null,
          contactPerson: null,
      }
    };
  },
  methods: {
    async createMerchant(data) {
      let result = await this.$api.merchants.createMerchant(data);
      //server errors will be displayed automatically
      if (!this.$apiSuccess(result)) return;

      this.$router.push({ name: "Merchant", params: { id: result.entityReference }});
    }
  }
};
</script>