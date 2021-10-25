<template>
  <v-card outlined class="ecbg border-primary">
    <v-card-subtitle v-if="!hideTitle" class="subtitle-1 px-1 pt-0">{{$t('InstallmentDetails')}}</v-card-subtitle>
    <v-container fluid class="px-0">
      <v-row no-gutters>
        <v-col cols="12" md="6">
          <!-- <v-text-field
            v-model.number="model.numberOfPayments"
            :label="$t('NumberOfPayments')"
            :rules="[vr.primitives.required, vr.primitives.inRange(minInstallments, maxInstallments)]"
            type="number"
            :min="minInstallments"
            :max="maxInstallments"
            step="1"
            @input="updateInstallments()"
            outlined
          ></v-text-field> -->
          <v-select
            v-model.number="model.numberOfPayments"
            :label="$t('NumberOfPayments')"
            :rules="[vr.primitives.required, vr.primitives.inRange(minInstallments, maxInstallments)]"
            @input="updateInstallments()"
            outlined
            :items="numberOfPaymentsArr"
          ></v-select>
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
            @input="updateInstallmentsTimeout(true)"
            outlined
          ></v-text-field>
        </v-col>
        <v-col cols="12" md="6">
          <v-text-field
            :value="model.installmentPaymentAmount"
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
import { mapState } from "vuex";
import ValidationRules from "../../helpers/validation-rules";
import appConstants from "../../helpers/app-constants";

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
    },
    transactionType: {
      type: String,
      required: false
    }
  },
  data() {
    return {
      vr: ValidationRules,
      model: {
        ...this.data
      },
      minInstallments: 1,
      maxInstallments: 36,
      numberOfPaymentsArr: [],
      installmentsTimeout: null
    };
  },
  computed: {
    ...mapState({
      terminalStore: state => state.settings.terminal
    }),
  },
  methods: {
    getData() {
      return {
        ...this.model,
        totalAmount: this.totalAmount
      };
    },
    updateInstallmentsTimeout(skipInitial = false){
      if(this.installmentsTimeout){
        clearTimeout(this.installmentsTimeout);
      }
      this.installmentsTimeout = setTimeout(() => this.updateInstallments(skipInitial), 1000);
    },
    updateInstallments(skipInitial = false){
      // if (!this.model.initialPaymentAmount || !this.model.numberOfPayments) {
      //   return 0;
      // }
      if (!this.model.numberOfPayments || this.model.numberOfPayments === 1) {
        return 0;
      }

      let leftover = this.totalAmount % 1;
      // this.model.installmentPaymentAmount = ((this.totalAmount - this.model.initialPaymentAmount) / (this.model.numberOfPayments)).toFixed(2);

      if(!skipInitial){
        let installmentPaymentAmountRaw = (this.totalAmount - leftover) / (this.model.numberOfPayments);
        this.model.installmentPaymentAmount = installmentPaymentAmountRaw.toFixed(2);
        this.model.initialPaymentAmount = (installmentPaymentAmountRaw + leftover).toFixed(2);
        // this.model.initialPaymentAmount = (this.totalAmount - (this.model.installmentPaymentAmount * (this.model.numberOfPayments - 1))).toFixed(2);

      }else{
        let installmentPaymentAmountRaw = (this.totalAmount - this.model.initialPaymentAmount) / (this.model.numberOfPayments - 1);
        this.model.installmentPaymentAmount = installmentPaymentAmountRaw.toFixed(2);
        this.model.initialPaymentAmount = (this.totalAmount - this.model.installmentPaymentAmount * (this.model.numberOfPayments - 1)).toFixed(2);
      }
    }
  },
  mounted() {
    if(this.transactionType == appConstants.transaction.types.credit){
      this.minInstallments = this.terminalStore.settings.minCreditInstallments || 1;
      this.maxInstallments = this.terminalStore.settings.maxCreditInstallments || 36;
    }else{
      this.minInstallments = this.terminalStore.settings.minInstallments || 1;
      this.maxInstallments = this.terminalStore.settings.maxInstallments || 36;
    }
    this.numberOfPaymentsArr = this.lodash.range(this.minInstallments, this.maxInstallments + 1);
    this.model.numberOfPayments = this.minInstallments;

    // if (!this.model.initialPaymentAmount) {
    //   this.model.initialPaymentAmount = this.totalAmount / this.model.numberOfPayments;
    // }
    this.updateInstallments();
  }
};
</script>