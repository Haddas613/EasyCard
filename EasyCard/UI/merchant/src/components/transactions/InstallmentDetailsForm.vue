<template>
  <v-card outlined class="ecbg border-primary">
    <v-card-subtitle v-if="!hideTitle" class="subtitle-1 px-1 pt-0">{{$t('InstallmentDetails')}}</v-card-subtitle>
    <v-container fluid class="px-0">
      <v-row no-gutters>
        <v-col cols="12" md="6">
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
        <v-col cols="12" md="6">
          <v-text-field
            v-model.number="model.initialPaymentAmount"
            :label="$t('InitialPaymentAmount')"
            type="number"
            min="0.01"
            :max="totalAmount"
            step="0.01"
            :rules="[vr.primitives.required, vr.primitives.lessThan(totalAmount)]"
            v-bind:class="{'px-1' : $vuetify.breakpoint.mdAndUp}"
            outlined
          ></v-text-field>
        </v-col>
        <v-col cols="12" md="6">
          <v-text-field
            :value="installmentPaymentAmount"
            :label="$t('InstallmentPaymentAmount')"
            type="number"
            min="0.01"
            step="0.01"
            :rules="[vr.primitives.required]"
            outlined
            readonly
          ></v-text-field>
        </v-col>
        <v-col cols="12" md="6">
          <v-text-field
            v-model.number="totalAmount"
            :label="$t('TotalAmount')"
            type="number"
            v-bind:class="{'px-1' : $vuetify.breakpoint.mdAndUp}"
            readonly
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
    },
    totalAmount: {
      type: Number,
      required: true,
      default: 0
    }
  },
  data() {
    return {
      vr: ValidationRules,
      model: {
        ...this.data
      }
    };
  },
  methods: {
    getData() {
      return {
        ...this.model,
        installmentPaymentAmount: this.installmentPaymentAmount,
        totalAmount: this.totalAmount
      };
    },
    valid(){
      
    }
  },
  mounted() {
    if (!this.model.numberOfPayments) {
      this.model.numberOfPayments = 1;
    }
    if (!this.model.initialPaymentAmount) {
      this.model.initialPaymentAmount =
        this.totalAmount / this.model.numberOfPayments;
    }
  },
  computed: {
    installmentPaymentAmount() {
      if (!this.model.initialPaymentAmount || !this.model.numberOfPayments) {
        return 0;
      }

      // if(this.model.numberOfPayments === 1){ return this.totalAmount;}

      return ((this.totalAmount - this.model.initialPaymentAmount) / this.model.numberOfPayments).toFixed(2);
    }
    // totalAmount() {
    //   if (
    //     !this.model.initialPaymentAmount ||
    //     !this.model.numberOfPayments ||
    //     !this.model.installmentPaymentAmount
    //   )
    //     return null;

    //   this.model.totalAmount =
    //     this.model.initialPaymentAmount +
    //     (this.model.numberOfPayments - 1) * this.model.installmentPaymentAmount;

    //   return this.model.totalAmount;
    // }
  }
};
</script>