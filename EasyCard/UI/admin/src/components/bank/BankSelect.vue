<template>
  <v-autocomplete
    :items="banks"
    item-text="description"
    item-value="value"
    v-model="model"
    :label="$t('Bank')"
    hide-details
    :rules="required ? [vr.primitives.required] : []"
    @change="ok()"
    outlined
    :disabled="disabled"
    ></v-autocomplete>
</template>

<script>
import ValidationRules from "../../helpers/validation-rules";

export default {
  model: {
    prop: 'data',
    event: 'change'
  },
  props: {
    data: {
      required: true
    },
    required: {
      type: Boolean,
      default: true
    },
    disabled: {
      type: Boolean,
      default: true
    }
  },
  data() {
    return {
      model: this.data,
      banks: [],
      vr: ValidationRules
    };
  },
  async mounted () {
      this.banks = await this.$api.dictionaries.getBanks();

      if(this.data){
          this.model = this.lodash.find(this.banks, e => e.value == this.data) || this.banks[0];
      }else if(!this.disabled){
          this.model = this.banks[0];
      }
  },
  methods: {
    ok() {
      this.$emit("ok", this.model);
      this.$emit("change", this.model);
    }
  }
};
</script>