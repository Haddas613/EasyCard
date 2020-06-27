<template>
  <v-card outlined class="border-primary">
    <v-subheader class="primary--text font-weight-light">{{$t('CreditCardDetails')}}</v-subheader>
    <v-container fluid class="px-2">
      <v-row>
        <v-col cols="12" md="3" sm="6">
          <v-text-field
            v-model.lazy="model.cardNumber"
            :label="$t('CardNumber')"
            required
            :rules="[vr.primitives.required, vr.primitives.stringLength(9, 19)]"
            type="text"
            @keydown.native.space.prevent
            @keydown.native.enter="parseCardReader()"
            @input="checkIfCardReader($event)"
          ></v-text-field>
        </v-col>
        <v-col cols="12" md="1" sm="6">
          <v-text-field
            v-model.number="model.cardExpiration.year"
            :label="$t('Year')"
            type="number"
            v-bind:min="creditCardExpYearFrom"
            v-bind:max="creditCardExpYearTo"
            :rules="[vr.primitives.required, vr.primitives.inRangeFlat(creditCardExpYearFrom, creditCardExpYearTo)]"
            required
            @keydown.native.space.prevent
            @input="resetCardReader($event)"
          ></v-text-field>
        </v-col>
        <v-col cols="12" md="1" sm="6">
          <v-text-field
            v-model.number="model.cardExpiration.month"
            :label="$t('Month')"
            type="number"
            min="1"
            max="12"
            :rules="[...vr.complex.month, monthExpired]"
            required
            @keydown.native.space.prevent
            @input="resetCardReader($event)"
          ></v-text-field>
        </v-col>
        <v-col cols="12" md="1" sm="6">
          <v-text-field
            v-model="model.cvv"
            :label="$t('CVV')"
            type="text"
            :rules="vr.complex.cvv"
            required
            @keydown.native.space.prevent
          ></v-text-field>
        </v-col>
        <v-col cols="12" md="3" sm="6">
          <v-text-field
            v-model="model.cardOwnerName"
            :label="$t('OwnerName')"
            :rules="[vr.primitives.stringLength(2, 50)]"
            required
            type="text"
          ></v-text-field>
        </v-col>
        <v-col cols="12" md="3" sm="6">
          <v-text-field
            v-model="model.cardOwnerNationalID"
            :label="$t('NationalID')"
            :rules="[vr.primitives.required, vr.special.israeliNationalId]"
            type="text"
            @keydown.native.space.prevent
          ></v-text-field>
        </v-col>
      </v-row>
    </v-container>
  </v-card>
</template>

<script>
import ValidationRules from "../../helpers/validation-rules";

export default {
  name: "CreditCardSecureDetails",
  data() {
    return {
      todayDate: new Date(),
      creditCardExpYearFrom: (() => {
        return new Date().getFullYear() - 2000;
      })(),
      creditCardExpYearTo: (() => {
        let d = new Date();
        return (
          new Date(
            d.getFullYear() + 10,
            d.getMonth(),
            d.getDay()
          ).getFullYear() - 2000
        );
      })(),
      vr: ValidationRules,
      model: { ...this.data },
      cardReaderMode: false
    };
  },
  methods: {
    //specific for this component, no need to extract to validation-rules.js
    monthExpired(v) {
      let current = this.model.cardExpiration.year + 2000 + v / 100;
      let min =
        this.todayDate.getFullYear() + (this.todayDate.getMonth() + 1) / 100;
      return current >= min || this.$t("Expired");
    },
    resetCardReader(){
      delete this.model.cardReaderInput;
      if(this.cardReaderMode){
        this.cardReaderMode = false;
        this.$emit('card-reader-update', false);
      }
    },
    checkIfCardReader($event){
      if(!(/^;\d{16}=\d{20}$/.test(this.model.cardNumber))){
        this.resetCardReader();
        return false;
      }
      return true;
    },
    parseCardReader(){
      if(!this.checkIfCardReader())
        return;
      let sep = this.model.cardNumber.split('=');
      this.model.cardReaderInput = this.model.cardNumber;

      //get rid of ';' at the beginning
      this.model.cardNumber = sep[0].substr(1, this.model.cardNumber.length);
      this.model.cardExpiration.year = parseInt(sep[1].substr(0, 2));
      this.model.cardExpiration.month = parseInt(sep[1].substr(2, 2));

      this.cardReaderMode = true;
      this.$emit('card-reader-update', true);
    }
  },
  props: { data: Object }
};
</script>