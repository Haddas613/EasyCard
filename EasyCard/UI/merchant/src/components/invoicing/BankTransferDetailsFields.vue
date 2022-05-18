<template>
  <v-flex class="d-flex flex-column">
    <v-row no-gutters>
      <bank-fields :data="model" ref="bankFields"></bank-fields>
      <v-col cols="12">
        <v-text-field
          :label="$t('Reference')"
          :counter="30"
          outlined
          v-model="model.reference"
          max="30"
          :rules="[vr.primitives.stringLength(3, 30)]"
          v-bind:class="{'px-1' : $vuetify.breakpoint.mdAndUp}"
        ></v-text-field>
      </v-col>
      <v-col cols="12">
        <ec-date-input v-bind:class="{'px-1' : $vuetify.breakpoint.mdAndUp}" :key="model.dueDate" v-model="model.dueDate" :label="$t('DueDate')"></ec-date-input>
      </v-col>
    </v-row>
  </v-flex>
</template>

<script>
import ValidationRules from "../../helpers/validation-rules";

export default {
  components: {
    EcDateInput: () => import("../ec/EcDateInput"),
    BankFields: () => import("../transactions/BankDetailsFields"),
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
      model: { 
        reference: null,
        dueDate: null,
        bank: null,
        bankBranch: null,
        bankAccount: null,
        ...this.data
      },
      vr: ValidationRules,
    };
  },
  methods: {
    getData() {
      return { 
        ...this.$refs.bankFields.getData(),
        reference: this.model.reference,
        dueDate: this.model.dueDate
      };
    }
  }
};
</script>