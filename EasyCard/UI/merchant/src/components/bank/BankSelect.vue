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
          this.model = (this.lodash.find(this.banks, e => e.value == this.data) || this.banks[0]).value;
      }else{
          this.model = this.banks[0].value;
      }
      this.$emit("change", this.model);
  },
  methods: {
    ok() {
      this.$emit("ok", this.model);
      this.$emit("change", this.model);
    }
  }
};
</script>