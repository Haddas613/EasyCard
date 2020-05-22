<template>
  <v-card outlined>
    <v-container fluid class="px-2">
      <v-row>
        <v-col cols="12" md="4">
          <v-text-field
            v-model="model.cardNumber"
            :label="$t('CardNumber')"
            required
            :rules="[vr.primitives.required, vr.primitives.stringLength(10, 19)]"
            type="'text'"
          ></v-text-field>
        </v-col>
        <v-col cols="12" md="4">
          <v-text-field
            v-model="model.cardOwnerName"
            :label="$t('OwnerName')"
            :rules="[vr.primitives.stringLength(2, 50)]"
            required
            type="'text'"
          ></v-text-field>
        </v-col>
        <v-col cols="12" md="4">
          <v-text-field
            v-model="model.cardOwnerNationalID"
            :label="$t('NationalID')"
            :rules="[vr.special.israeliNationalId]"
            type="'text'"
          ></v-text-field>
        </v-col>
      </v-row>
      <v-row>
        <v-col cols="12" md="4">
          <v-text-field
            v-model="model.cardExpiration.year"
            :label="$t('ExpirationYear')"
            type="number"
            v-bind:min="todayDate.getFullYear()"
            :rules="[vr.primitives.required, vr.primitives.inRangeFlat(todayDate.getFullYear(), creditCardExpToDate.getFullYear())]"
            required
          ></v-text-field>
        </v-col>
        <v-col cols="12" md="4">
          <v-text-field
            v-model="model.cardExpiration.month"
            :label="$t('ExpirationMonth')"
            type="number"
            min="1"
            max="12"
            :rules="[...vr.complex.month, vr.primitives.expired(todayDate.getMonth() + 1)]"
            required
          ></v-text-field>
        </v-col>
        <v-col cols="12" md="2">
          <v-text-field
            v-model="model.cvv"
            :label="$t('CVV')"
            type="text"
            :counter="5"
            :rules="vr.complex.cvv"
            required
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
      creditCardExpToDate: (() => {
        let d = new Date();
        return new Date(d.getFullYear() + 10, d.getMonth(), d.getDay());
      })(),
      vr: ValidationRules,
      model: {...this.data}
    };
  },
  props: { data: Object }
};
</script>