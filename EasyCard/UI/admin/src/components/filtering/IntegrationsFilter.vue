<template>
  <v-row>
    <v-col cols="12" :md="md" sm="6">
      <v-text-field v-model="data.shvaShovarNumber" :label="$t('VoucherNumber')" hide-details></v-text-field>
    </v-col>
    <v-col cols="12" :md="md" sm="6">
      <v-text-field v-model="data.clearingHouseTransactionID" :label="$t('AggregatorReference')" hide-details></v-text-field>
    </v-col>
    <v-col cols="12" :md="md" sm="6">
      <v-text-field v-model="data.shvaTransactionID" :label="$t('ProcessorReference')" hide-details></v-text-field>
    </v-col>
  </v-row>
</template>

<script>
import ValidationRules from "../../helpers/validation-rules";

export default {
  model: {
    prop: "data",
    event: "change"
  },
  props: {
    data: {
      type: Object,
      default: null,
      required: true
    },
    md:{
      default: "3",
      required: false
    }
  },
  data() {
    return {
      vr: ValidationRules
    };
  }
};
</script>