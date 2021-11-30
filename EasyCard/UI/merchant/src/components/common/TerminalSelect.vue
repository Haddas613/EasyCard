<template>
  <v-autocomplete
    :items="terminals"
    item-text="label"
    item-value="terminalID"
    v-model="value"
    outlined
    :label="$t('Terminal')"
    :rules="rules"
    :disabled="disabled"
    :clearable="clearable"
    v-on:change="$emit('change', value)"
  ></v-autocomplete>
</template>

<script>
import ValidationRules from "../../helpers/validation-rules";

export default {
  name: "TerminalSelect",
  model: {
    prop: "val",
    event: "change"
  },
  props: {
    val: {
      required: true
    },
    showDeleted: {
      type: Boolean,
      default: false
    },
    required: {
      type: Boolean,
      default: false
    },
    disabled: {
      type: Boolean,
      default: false
    },
    clearable: {
      type: Boolean,
      default: false
    }
  },
  data() {
    return {
      terminals: [],
      rules: [],
      vr: ValidationRules,
      value: this.val,
    };
  },
  async mounted() {
    if (this.required) {
      this.rules = [this.vr.primitives.required];
    }
    this.terminals = (await this.$api.terminals.getTerminals(null, {
          showDeleted: this.$showDeleted(this.showDeleted)
        })
    ).data || [];
  },
  watch: {
    'val'(newValue, oldValue) {
      this.value = newValue;
    }
  },
};
</script>