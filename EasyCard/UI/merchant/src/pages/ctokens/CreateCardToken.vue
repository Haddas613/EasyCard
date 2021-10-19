<template>
  <v-card width="100%" flat color="ecbg">
    <v-card-text class="px-4">
      <card-token-form :auth-code-required="needAuthorizationCodeMsg" :data="model" v-on:ok="createToken($event)"></card-token-form>
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
      needAuthorizationCodeMsg: null,
      loading: false
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
      if(this.loading) return;

      this.loading = true;
      let result = await this.$api.cardTokens.createCardToken(data);
      this.loading = false;

      if(result.additionalData && result.additionalData.authorizationCodeRequired){
        this.needAuthorizationCodeMsg = result.additionalData.message;
      }else{
        this.needAuthorizationCodeMsg = null;
      }
      
      if (!this.$apiSuccess(result)) return;

      this.$router.push({ name: 'Customer', params: {id: this.model.consumerID} });
    }
  }
};
</script>