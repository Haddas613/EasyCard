<template>
  <v-flex class="d-flex flex-column">
    <v-row no-gutters>
      <v-col cols="12" md="6">
        <v-text-field
          :label="$t('ChequeNumber')"
          :counter="6"
          outlined
          v-model="model.chequeNumber"
          max="6"
          :rules="[vr.primitives.required, vr.primitives.stringLength(6, 6)]"
          v-bind:class="{'px-1' : $vuetify.breakpoint.mdAndUp}"
        ></v-text-field>
      </v-col>
      <v-col cols="12" md="6">
        <v-text-field
          :label="$t('Bank')"
          :counter="6"
          outlined
          v-model="model.bank"
          max="6"
          type="number"
          :rules="[vr.primitives.required, vr.primitives.numeric()]"
          v-bind:class="{'px-1' : $vuetify.breakpoint.mdAndUp}"
        ></v-text-field>
      </v-col>
      <v-col cols="12" md="6">
        <v-text-field
          :label="$t('BankBranch')"
          :counter="6"
          outlined
          v-model="model.bankBranch"
          max="6"
          type="number"
          :rules="[vr.primitives.required, vr.primitives.numeric()]"
          v-bind:class="{'px-1' : $vuetify.breakpoint.mdAndUp}"
        ></v-text-field>
      </v-col>
      <v-col cols="12" md="6">
        <v-text-field
          :label="$t('BankAccount')"
          :counter="12"
          outlined
          v-model="model.bankAccount"
          max="12"
          type="number"
          :rules="[vr.primitives.required, vr.primitives.numeric(), vr.primitives.stringLength(6, 12)]"
          v-bind:class="{'px-1' : $vuetify.breakpoint.mdAndUp}"
        ></v-text-field>
      </v-col>
      <v-col cols="12">
        <ec-date-input :key="model.chequeDate" v-model="model.chequeDate" :label="$t('DueDate')"></ec-date-input>
      </v-col>
    </v-row>
  </v-flex>
</template>

<script>
import ValidationRules from "../../helpers/validation-rules";

export default {
  components: {
    EcDateInput: () => import("../ec/EcDateInput"),
  },
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
      invoice: { ...this.data },
      model: { 
        chequeNumber: null,
        chequeDate: null,
        bank: null,
        bankBranch: null,
        bankAccount: null
      },
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
      return { ...this.model };
    }
  }
};
</script>