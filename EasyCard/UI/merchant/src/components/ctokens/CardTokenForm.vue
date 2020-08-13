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
        <credit-card-secure-details-form
          :key="customer != null"
          :data="model"
          ref="ccsecuredetailsform"
        ></credit-card-secure-details-form>
      </v-form>
    </v-card-text>
    <v-card-actions>
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
import ValidationRules from "../../helpers/validation-rules";
import CreditCardSecureDetailsForm from "../transactions/CreditCardSecureDetailsForm";

export default {
  components: {
    CreditCardSecureDetailsForm
  },
  props: {
    data: {
      type: Object,
      default: null,
      required: false
    }
  },
  data() {
    return {
      model: { ...this.data },
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
        this.model.terminalID = this.customer.terminalID;
      }
    }
    else if (!this.customer) {
      return this.$router.push({ name: "404" });
    }
    this.model.cardOwnerName = this.customer.consumerName;
    this.model.cardOwnerNationalID = this.customer.consumerNationalID;
    this.model.consumerEmail = this.customer.consumerEmail;
  },
  methods: {
    ok() {
      let form = this.$refs.form.validate();

      if (!form) return;

      let data = this.$refs.ccsecuredetailsform.getData();

      if (!data) {
        return;
      }

      this.$emit("ok", {
        ...this.model,
        ...data
      });
    }
  }
};
</script>