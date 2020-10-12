<template>
  <v-card class="ec-card d-flex flex-column" fill-height>
    <ec-dialog :dialog.sync="tokensDialog">
      <template v-slot:title>{{$t('SavedTokens')}}</template>
      <template>
        <div class="d-flex px-2 justify-end">
          <v-btn
            color="red"
            class="white--text"
            :disabled="selectedToken == null"
            :block="$vuetify.breakpoint.smAndDown"
            @click="resetToken()"
          >
          <v-icon left>mdi-delete</v-icon>
          {{$t("CancelSelection")}}
          </v-btn>
        </div>
        <ec-radio-group
          :data="customerTokens"
          labelkey="cardNumber"
          valuekey="creditCardTokenID"
          return-object
          :model.sync="token"
        ></ec-radio-group>
      </template>
    </ec-dialog>
    <v-card-text class="py-2">
      <ec-dialog-invoker
        v-on:click="handleClick()"
        v-if="customerTokens"
        :clickable="(customerTokens.length > 0)"
        class="py-2"
      >
        <template v-slot:prepend>
          <v-icon>mdi-credit-card-outline</v-icon>
        </template>
        <template v-slot:left >
          <div v-if="!token">
            <span v-if="customerTokens.length > 0" >{{$t("@ChooseFromSavedCount").replace("@count", customerTokens.length)}}</span>
            <span v-if="customerTokens.length === 0">{{$t("NoSavedCards")}}</span>
          </div>
          <div v-if="token">
            <span class="primary--text">
              {{token.cardNumber}}
            </span>
          </div>
        </template>
        <template v-slot:append>
          <re-icon>mdi-chevron-right</re-icon>
        </template>
      </ec-dialog-invoker>
      <v-form class="ec-form" ref="form" lazy-validation v-if="!token">
        <credit-card-secure-details-form
          :data="model.creditCardSecureDetails"
          ref="ccsecuredetailsform"
        ></credit-card-secure-details-form>
        <v-checkbox v-model="model.saveCreditCard" :label="$t('SaveCard')" :disabled="!model.dealDetails.consumerID"></v-checkbox>
      </v-form>
    </v-card-text>
    <v-card-actions class="px-4">
      <v-btn color="primary" bottom :x-large="true" block @click="ok()">{{$t(btnText)}}</v-btn>
    </v-card-actions>
  </v-card>
</template>

<script>
import ValidationRules from "../../helpers/validation-rules";

export default {
  components: {
    CreditCardSecureDetailsForm: () => import("./CreditCardSecureDetailsForm"),
    EcDialog: () => import("../../components/ec/EcDialog"),
    EcDialogInvoker: () => import("../../components/ec/EcDialogInvoker"),
    EcRadioGroup: () => import("../../components/inputs/EcRadioGroup"),
    ReIcon: () => import("../../components/misc/ResponsiveIcon")
  },
  props: {
    data: {
      type: Object,
      default: null,
      required: false
    },
    btnText: {
      type: String,
      default: 'OK',
      required: false
    }
  },
  data() {
    return {
      model: { ...this.data },
      tokensDialog: false,
      customerTokens: null,
      selectedToken: null,
      selectedTokenObj: null,
    };
  },
  async mounted() {
    if (this.model.dealDetails.consumerID) {
      this.customerTokens =
        (
          await this.$api.cardTokens.getCustomerCardTokens(
            this.model.dealDetails.consumerID
          )
        ).data || [];
      this.selectedToken = this.model.creditCardToken;
    }else{
      this.model.saveCreditCard = false;
      this.selectedToken = this.selectedTokenObj = null;
    }
  },
  computed: {
    token: {
      get: function() {
        return this.selectedTokenObj;
      },
      set: function(nv) {
        this.selectedToken = nv ? nv.creditCardTokenID : null;
        this.selectedTokenObj = nv;
      }
    }
  },
  methods: {
    ok() {
      if (this.selectedToken) {
        this.okCardToken();
      } else {
        this.okCreditCard();
      }
    },
    resetToken() {
      this.token = null;
    },
    okCardToken() {
      if (!this.selectedToken) return;

      this.$emit("ok", {
        type: "token",
        data: this.selectedToken
      });
    },
    okCreditCard() {
      let form = this.$refs.form.validate();

      if (!form) return;

      let data = this.$refs.ccsecuredetailsform.getData();

      if (!data) {
        return;
      }

      this.$emit("ok", {
        type: "creditcard",
        data: {
          ...this.model.creditCardSecureDetails,
          ...data,
          saveCreditCard: this.model.saveCreditCard
        }
      });
    },
    handleClick() {
      this.tokensDialog = this.customerTokens && this.customerTokens.length > 0;
    }
  }
};
</script>