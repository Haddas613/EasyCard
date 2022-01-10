<template>
  <v-card class="ec-card d-flex flex-column" fill-height>
    <v-card-text class="py-2 px-0">
      <v-form class="ec-form" ref="form" lazy-validation>
        <v-text-field
          v-model="model.consumerEmail"
          :counter="50"
          :rules="[vr.primitives.email]"
          :label="$t('Email')"
          outlined
        ></v-text-field>
        <credit-card-secure-details-fields
          :key="customer != null"
          :data="model"
          :tokens="customerTokens"
          ref="ccsecuredetailsform"
        ></credit-card-secure-details-fields>

        <v-alert
          class="mt-4"
          dense 
          color="error"
          colored-border
          icon="mdi-alert"
          :border="$vuetify.rtl ? 'right': 'left'"
          v-if="authCodeRequired"
        >
          <small>{{authCodeRequired}}</small>
        </v-alert>
        <v-text-field
          v-bind:class="{'mt-4': !authCodeRequired, 'pt-0': authCodeRequired}"
          outlined
          v-model="model.oKNumber"
          :label="authCodeRequired ? $t('AuthorizationCode') : $t('AuthorizationCodeOptional')"
          :rules="[vr.primitives.requiredDepends(authCodeRequired), vr.primitives.stringLength(1, 50)]">
        </v-text-field>
      </v-form>
    </v-card-text>
    <v-card-actions v-if="showActions">
      <v-col cols="12" class="d-flex justify-end" v-if="!$vuetify.breakpoint.smAndDown">
        <v-btn
          class="mx-1"
          color="white"
          :to="{ name: 'Customer', params: {id: model.consumerID} }"
        >{{$t('Cancel')}}</v-btn>
        <v-btn color="primary" @click="ok()">{{$t('Save')}}</v-btn>
      </v-col>
      <v-col cols="12" v-if="$vuetify.breakpoint.smAndDown">
        <v-btn
          block
          color="white"
          :to="{ name: 'Customer', params: {id: model.consumerID} }"
        >{{$t('Cancel')}}</v-btn>
        <v-spacer class="py-2"></v-spacer>
        <v-btn block color="primary" @click="ok()">{{$t('Save')}}</v-btn>
      </v-col>
    </v-card-actions>
  </v-card>
</template>

<script>
import { mapState } from "vuex";
import ValidationRules from "../../helpers/validation-rules";
import CreditCardSecureDetailsFields from "../transactions/CreditCardSecureDetailsFields";

export default {
  components: {
    CreditCardSecureDetailsFields
  },
  props: {
    data: {
      type: Object,
      default: null,
      required: false
    },
    showActions: {
      type: Boolean,
      default: true,
      required: false
    },
    authCodeRequired: {
      type: String,
      default: null,
      required: false
    }
  },
  data() {
    return {
      model: { ...this.data },
      customerTokens: [],
      customer: null,
      vr: ValidationRules
    };
  },
  async mounted() {
    if (this.model.consumerID) {
      this.customer = await this.$api.consumers.getConsumer(
        this.model.consumerID
      );

      if (!this.model.terminalID) {
        this.model.terminalID = this.terminalStore.terminalID;
      }

      this.customerTokens =
        (
          await this.$api.cardTokens.getCustomerCardTokens(
            this.model.consumerID
          )
        ).data || [];
    }
    // else if (!this.customer) {
    //   return this.$router.push({ name: "404" });
    // }
    
    this.model.cardOwnerName = this.customer.consumerName;
    this.model.cardOwnerNationalID = this.customer.consumerNationalID;
    this.model.consumerEmail = this.customer.consumerEmail;
  },
  watch: {
    authCodeRequired(newValue, oldValue) {
      this.$nextTick(() => this.$refs.form.validate())
    }
  },
  methods: {
    ok() {
      let data = this.getData();
      
      if(!data){ return;}

      this.$emit("ok", data);
    },
    getData(){
      let form = this.$refs.form.validate();

      if (!form) return;

      let data = this.$refs.ccsecuredetailsform.getData();

      if (!data) return;

      return {
        ...this.model,
        ...data
      };
    }
  },
  computed: {
    ...mapState({
      terminalStore: state => state.settings.terminal,
    }),
  },
};
</script>