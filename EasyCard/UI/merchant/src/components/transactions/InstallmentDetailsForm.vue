<template>
  <v-container fluid class="px-2">
    <v-row>
      <v-col cols="12" md="3">
        <v-text-field
          v-model="model.numberOfPayments"
          :label="$t('NumberOfPayments')"
          required
          :rules="[vr.primitives.required, vr.primitives.inRange(1, 100)]"
          type="number"
          min="1"
          step="1"
        ></v-text-field>
      </v-col>
      <v-col cols="12" md="3">
        <v-text-field
          v-model="model.initialPaymentAmount"
          :label="$t('InitialPaymentAmount')"
          type="number"
          min="0.01"
          step="0.01"
          :rules="[vr.primitives.required]"
          required
        ></v-text-field>
      </v-col>
      <v-col cols="12" md="3">
        <v-text-field
          v-model="model.installmentPaymentAmount"
          :label="$t('InstallmentPaymentAmount')"
          type="number"
          min="0.01"
          step="0.01"
          :rules="[vr.primitives.required]"
          required
        ></v-text-field>
      </v-col>
      <v-col cols="12" md="3">
        <v-text-field
          v-model="totalAmount"
          :label="$t('TotalAmount')"
          type="number"
          disabled
          required
        ></v-text-field>
      </v-col>
    </v-row>
  </v-container>
</template>

<script>
import ValidationRules from "../../helpers/validation-rules";

export default {
  name: "InstallmentDetailsForm",
  props: { data: Object },
  data() {
    return {
      vr: ValidationRules,
      model: { ...this.data }
    };
  },
  computed: {
    totalAmount() {
      return (
        this.model.initialPaymentAmount +
        (this.model.numberOfPayments - 1) *
          this.model.installmentPaymentAmount
      );
    }
  }
};
</script>