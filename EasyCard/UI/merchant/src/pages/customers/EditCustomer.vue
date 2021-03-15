<template>
  <v-card width="100%" flat color="ecbg">
    <v-card-text class="px-0">
      <customer-form :data="model" v-on:ok="updateCustomer($event)" class="px-4" v-if="model"></customer-form>
    </v-card-text>
  </v-card>
</template>

<script>
import CustomerForm from "../../components/customers/CustomerForm";

export default {
  components: { CustomerForm },
  data() {
    return {
      model: null
    };
  },
  methods: {
    async updateCustomer(data) {
      let result = await this.$api.consumers.updateConsumer(
        this.$route.params.id,
        data
      );

      //server errors will be displayed automatically
      if (!result) return;

      if (result.status === "success") {
        this.$router.push("/admin/customers/list");
      } else {
        this.$toasted.show(result.message, { type: "error" });
      }
    }
  },
  async mounted() {
    let result = await this.$api.consumers.getConsumer(this.$route.params.id);

    if (!result) {
      this.$router.push("/admin/customers/list");
    }

    this.model = result;
    this.$store.commit("ui/changeHeader", {
      value: {
        text: { translate: false, value: this.model.consumerName },
      }
    });
  }
};
</script>

<style lang="scss" scoped>
</style>