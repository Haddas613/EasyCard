<template>
  <v-row class="px-3">
    <v-col cols="12" md="6" sm="6">
      <v-text-field outlined hide-details="true" v-model="data.merchantID" :label="$t('Merchant')"></v-text-field>
    </v-col>
    <v-col cols="12" md="6" sm="6">
      <v-text-field outlined hide-details="true" v-model="data.terminalID" :label="$t('Terminal')"></v-text-field>
    </v-col>
  </v-row>
</template>

<script>
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
    }
  }
};
</script>