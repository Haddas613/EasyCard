<template>
  <v-card outlined class="ecbg border-primary">
    <v-card-subtitle v-if="!hideTitle" class="subtitle-1">{{$t('InstallmentDetails')}}</v-card-subtitle>
    <v-container fluid class="px-0">
      <v-row no-gutters>
        <v-col cols="12" md="4">
          <v-text-field
            v-model.number="model.numberOfPayments"
            :label="$t('NumberOfPayments')"
            :rules="[vr.primitives.required, vr.primitives.inRange(1, 100)]"
            type="number"
            min="1"
            step="1"
            outlined
          ></v-text-field>
        </v-col>
        <v-col cols="12" md="4">
          <v-text-field
            v-model.number="model.initialPaymentAmount"
            :label="$t('InitialPaymentAmount')"
            type="number"
            min="0.01"
            step="0.01"
            :rules="[vr.primitives.required]"
            outlined
          ></v-text-field>
        </v-col>
        <v-col cols="12" md="4">
          <v-text-field
            v-model.number="model.installmentPaymentAmount"
            :label="$t('InstallmentPaymentAmount')"
            type="number"
            min="0.01"
            step="0.01"
            :rules="[vr.primitives.required]"
            outlined
          ></v-text-field>
        </v-col>
        <v-col cols="12">
          <v-text-field
            v-model.number="totalAmount"
            :label="$t('TotalAmount')"
            type="number"
            disabled
            outlined
          ></v-text-field>
        </v-col>
      </v-row>
    </v-container>
  </v-card>
</template>

<script>
import ValidationRules from "../../helpers/validation-rules";

export default {
  name: "InstallmentDetailsForm",
  props: {
    data: Object,
    hideTitle: {
      type: Boolean,
      required: false,
      default: false
    }
  },
  data() {
    return {
      vr: ValidationRules,
      model: { ...this.data }
    };
  },
  computed: {
    totalAmount() {
      if (
        !this.model.initialPaymentAmount ||
        !this.model.numberOfPayments ||
        !this.model.installmentPaymentAmount
      )
        return null;

      this.model.totalAmount =
        this.model.initialPaymentAmount +
        (this.model.numberOfPayments - 1) * this.model.installmentPaymentAmount;

      return this.model.totalAmount;
    }
  }
};
</script>