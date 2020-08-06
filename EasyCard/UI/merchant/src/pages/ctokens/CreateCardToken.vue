<template>
  <v-card width="100%" flat color="ecbg">
    <v-card-title class="hidden-sm-and-down">{{$t("CreateCardToken")}}</v-card-title>
    <v-card-text class="px-4">
      <card-token-form :data="model" v-on:ok="createToken($event)"></card-token-form>
    </v-card-text>
  </v-card>
</template>

<script>
import CardTokenForm from "../../components/ctokens/CardTokenForm";
import { mapState } from "vuex";

export default {
  components: {
    CardTokenForm
  },
  props: {
    customer: {
      type: Object,
      default: null,
      required: false
    }
  },
  data() {
    return {
      model: {
        cardExpiration: null,
        cardNumber: null,
        cvv: null,
        cardOwnerNationalID: null,
        cardOwnerName: null,
        terminalID: null,
        consumerID: this.$route.params.customerid,
        consumerEmail: null
      },
    };
  },
  computed: {
    ...mapState({
      terminalStore: state => state.settings.terminal
    })
  },
  async mounted() {
  },
  methods: {
    async createToken(data) {
      let result = await this.$api.cardTokens.createCardToken(data);
      //server errors will be displayed automatically
      if (!result) return;
      if (result.status === "success") {
        this.$router.push({ name: 'Customer', params: {id: model.consumerID} });
      } else {
        this.$toasted.show(result.message, { type: "error" });
      }
    }
  }
};
</script>