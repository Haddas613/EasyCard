<template>
  <v-card class="ec-card d-flex flex-column" fill-height>
    <v-card-text class="py-2">
      <v-form class="ec-form" ref="form" lazy-validation>
        <credit-card-secure-details-form :data="model.creditCardSecureDetails" ref="ccsecuredetailsform"></credit-card-secure-details-form>
        <v-checkbox v-model="model.creditCardSecureDetails.save" :label="$t('SaveCard')"></v-checkbox>
      </v-form>
    </v-card-text>
    <v-card-actions class="px-0">
      <v-btn color="primary" bottom :x-large="true" block @click="ok()">{{$t('Charge')}}</v-btn>
    </v-card-actions>
  </v-card>
</template>

<script>
import ValidationRules from "../../helpers/validation-rules";
import CreditCardSecureDetailsForm from "./CreditCardSecureDetailsForm";

export default {
  components: {
    CreditCardSecureDetailsForm,
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
      model: { ...this.data }
    };
  },
  methods: {
    ok() {
      let form = this.$refs.form.validate();

      if (!form) return;
      
      let data = this.$refs.ccsecuredetailsform.getData()

      if(!data){
        return;
      }
      
      this.$emit("ok", {
        ...this.model.reditCardSecureDetails,
        ...data
      });
    }
  }
};
</script>