<template>
  <ec-dialog :dialog.sync="visible">
    <template v-slot:title>{{$t('AddToken')}}</template>
    <template>
      <card-token-form :auth-code-required="needAuthorizationCodeMsg" :data="model" :show-actions="false" ref="ctokenFormRef"></card-token-form>
      <div class="d-flex px-2 pt-4 justify-end">
        <v-btn
          color="primary"
          class="white--text"
          :block="$vuetify.breakpoint.smAndDown"
          :loading="loading"
          @click="ok()"
        >{{$t("OK")}}</v-btn>
      </div>
    </template>
  </ec-dialog>
</template>

<script>
export default {
  props: {
    customerId: {
      type: String,
      required: true
    },
    show: {
      type: Boolean,
      default: false,
      required: true
    }
  },
  components: {
    EcDialog: () => import("../../components/ec/EcDialog"),
    CardTokenForm: () => import("./CardTokenForm")
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
        consumerEmail: null,
        consumerID: this.customerId
      },
      needAuthorizationCodeMsg: null,
      loading: false,
    };
  },
  computed: {
    visible: {
      get: function() {
        return this.show;
      },
      set: function(val) {
        this.$emit("update:show", val);
      }
    }
  },
  methods: {
    async ok() {  
      if(this.loading) return;

      let data = this.$refs.ctokenFormRef.getData();
      if(!data) return;

      this.loading = true;
      let result = await this.$api.cardTokens.createCardToken(data);
      this.loading = false;
      if(result.additionalData && result.additionalData.authorizationCodeRequired){
        this.needAuthorizationCodeMsg = result.additionalData.message;
      }else{
        this.needAuthorizationCodeMsg = null;
      }

      if (!this.$apiSuccess(result)) return;

      data.creditCardTokenID = result.entityReference;
      this.$emit("ok", data);
    },
    reset() {
      this.model = {
        cardExpiration: null,
        cardNumber: null,
        cvv: null,
        cardOwnerNationalID: null,
        cardOwnerName: null,
        terminalID: null,
        consumerID: this.customerId,
        consumerEmail: null
      };
    }
  }
};
</script>