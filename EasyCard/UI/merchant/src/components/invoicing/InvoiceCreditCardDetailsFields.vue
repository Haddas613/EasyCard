<template>
  <v-flex>
    <v-row>
      <v-col cols="12">
        <v-text-field
          prefix="**** **** ****"
          :label="$t('CreditCardLast4Numbers')"
          :counter="4"
          outlined
          dir="ltr"
          v-model="model.creditCardDetails.cardNumber"
          v-bind:class="{'text-end': $vuetify.rtl}"
          max="4"
          :rules="[vr.primitives.requiredDepends(this.required), vr.primitives.stringLength(4, 4)]"
          ref="inpCardNumber"
        ></v-text-field>
      </v-col>
      <v-col cols="12" md="12">
        <v-text-field
          :label="$t('Expiry')"
          outlined
          placeholder="MM/YY"
          dir="ltr"
          v-model="model.creditCardDetails.cardExpiration"
          v-bind:class="{'text-end': $vuetify.rtl}"
          :rules="[vr.primitives.requiredDepends(this.required), vr.primitives.requiredDepends(this.model.creditCardDetails.cardNumber)]"
          ref="inpExpiry"
          v-cardformat:formatCardExpiry
        ></v-text-field>
      </v-col>
      <!-- <v-col cols="12" md="6">
        <v-text-field
          :label="$t('NatID')"
          outlined
          v-model="model.creditCardDetails.cardOwnerNationalID"
          :rules="validation.cardOwnerNationalID"
        ></v-text-field>
      </v-col> -->
    </v-row>
  </v-flex>
</template>

<script>
import ValidationRules from "../../helpers/validation-rules";

export default {
  props: {
    data: {
      type: Object,
      default: null,
      required: false
    },
    required: {
      type: Boolean,
      default: false
    }
  },
  data() {
    return {
      model: { creditCardDetails: {}, ...this.data },
      vr: ValidationRules,
      errors: {
        cardNumber: false,
        expiry: false,
        cvv: false,
        nationalId: false
      },
    };
  },
  methods: {
    getData() {
      var cardNumberValid = this.lodash.reduce(this.$refs.inpCardNumber.rules, (e, n) => n(this.model.cardNumberValid), true);
      var expiryValid = this.lodash.reduce(this.$refs.inpExpiry.rules, (e, n) => n(this.model.cardExpiration), true);
      // var nationalIdValid = this.lodash.reduce(this.validation.cardOwnerNationalID, (e, n) => n(this.model.cardOwnerNationalID), true);
      
      if(!(cardNumberValid && expiryValid)){
        return null;
      }

      if((!this.model.creditCardDetails.cardNumber && !this.model.creditCardDetails.cardExpiration)){
        return null;
      }

      return {
        cardExpiration: this.model.creditCardDetails.cardExpiration.replace(/\s/g, ""),
        cardNumber: `000000000000${this.model.creditCardDetails.cardNumber}`
      };
    }
  }
};
</script>