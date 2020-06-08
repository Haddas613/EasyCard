<template>
  <v-card outlined class="border-primary">
    <v-subheader class="primary--text font-weight-light">{{$t('CreditCardDetails')}}</v-subheader>
    <v-container fluid class="px-2">
      <v-row>
        <v-col cols="12" md="3" sm="6">
          <v-text-field
            v-model="model.cardNumber"
            :label="$t('CardNumber')"
            required
            :rules="[vr.primitives.required, vr.primitives.stringLength(10, 19)]"
            type="text"
            @keydown.native.space.prevent
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
      model: { ...this.data }
    };
  },
  methods: {
    //specific for this component, no need to extract to validation-rules.js
    monthExpired(v) {
      let current = this.model.cardExpiration.year + 2000 + v / 100;
      let min =
        this.todayDate.getFullYear() + (this.todayDate.getMonth() + 1) / 100;
      return current >= min || this.$t("Expired");
    }
  },
  props: { data: Object }
};
</script>