<template>
  <v-row class="px-3">
    <v-col cols="12" md="6" sm="6">
      <v-text-field
        outlined
        hide-details="true"
        v-model="data.merchantID"
        :label="$t('Merchant')"
        :rules="[vr.primitives.guid]"
        clearable
      ></v-text-field>
    </v-col>
    <v-col cols="12" md="6" sm="6">
      <v-text-field
        outlined
        hide-details="true"
        v-model="data.terminalID"
        :label="$t('Terminal')"
        :rules="[vr.primitives.guid]"
        clearable
      ></v-text-field>
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
    }
  },
  data() {
    return {
      vr: ValidationRules
    }
  },
};
</script>