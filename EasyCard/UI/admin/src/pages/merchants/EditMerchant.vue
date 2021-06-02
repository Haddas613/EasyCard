<template>
  <v-card width="100%" flat color="ecbg">
    <v-card-text>
      <merchant-form :data="model" v-on:ok="updateMerchant($event)" class="px-4" v-if="model"></merchant-form>
    </v-card-text>
  </v-card>
</template>

<script>
import MerchantForm from "../../components/merchants/MerchantForm";

export default {
  components: { MerchantForm },
  data() {
    return {
      model: null
    };
  },
  methods: {
    async updateMerchant(data) {
      let result = await this.$api.merchants.updateMerchant(data);
      if (!this.$apiSuccess(result)) return;

      this.$router.push({name: 'Merchant', params: {id: this.model.$merchantID || this.model.merchantID}});
    }
  },
  async mounted() {
    let result = await this.$api.merchants.getMerchant(this.$route.params.id);

    if (!result) {
      this.$router.push({name: "Merchants"});
    }

    this.model = result;
    this.$store.commit("ui/changeHeader", {
      value: {
        text: { translate: false, value: this.model.businessName },
      }
    });
  }
};
</script>