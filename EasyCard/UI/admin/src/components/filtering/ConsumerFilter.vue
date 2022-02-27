<template>
  <v-row>
    <v-col cols="12" md="3" sm="6">
      <v-text-field v-model="data.consumerID" :label="$t('CustomerID')" hide-details></v-text-field>
    </v-col>
    <v-col cols="12" md="3" sm="6">
      <v-text-field v-model="data.consumerEmail" :label="$t('Email')" hide-details></v-text-field>
    </v-col>
    <v-col cols="12" md="3" sm="6">
      <v-text-field
        v-model="data.cardOwnerName"
        :label="$t('CardOwnerName')"
        hide-details
      ></v-text-field>
    </v-col>
    <v-col cols="12" md="3" sm="6">
      <v-text-field v-model="data.cardNumber" :label="$t('CardNumber')" hide-details></v-text-field>
    </v-col>
    <v-col cols="12" md="3" sm="6">
      <v-text-field
        v-model="data.consumerExternalReference"
        :label="$t('CustomerExternalReference')"
      ></v-text-field>
    </v-col>
  </v-row>
</template>

<script>
import ValidationRules from '../../helpers/validation-rules';

export default {
  model: {
    prop: 'data',
    event: 'change',
  },
  props: {
    data: {
      type: Object,
      default: null,
      required: true,
    },
  },
  data() {
    return {
      vr: ValidationRules,
    };
  },
};
</script>
