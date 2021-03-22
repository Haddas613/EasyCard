<template>
  <v-form ref="form" v-model="valid" lazy-validation>
    <v-row>
      <v-col cols="12">
        <v-text-field
          v-model="model.itemName"
          :counter="80"
          :rules="[vr.primitives.required, vr.primitives.maxLength(80)]"
          :label="$t('Name')"
          class="px-1"
          outlined
        ></v-text-field>
      </v-col>
      <v-col cols="12" md="6">
        <v-select
          :items="dictionaries.currencyEnum"
          item-text="description"
          item-value="code"
          v-model="model.currency"
          required
          :label="$t('Currency')"
           class="px-1"
          outlined
        ></v-select>
      </v-col>
      <v-col cols="12" md="6">
        <v-text-field
          v-model.lazy="model.price"
          :label="$t('Price')"
          :rules="[vr.primitives.biggerThan(0), vr.primitives.precision(2)]"
          required
          class="px-1"
          outlined
        ></v-text-field>
      </v-col>
      <v-col cols="12" class="d-flex justify-end" v-if="!$vuetify.breakpoint.smAndDown">
        <v-btn class="mx-1" color="white" :to="'/admin/items/list'">{{$t('Cancel')}}</v-btn>
        <v-btn color="primary" @click="ok()">{{$t('Save')}}</v-btn>
      </v-col>
      <v-col cols="12" v-if="$vuetify.breakpoint.smAndDown">
        <v-btn block color="white" :to="'/admin/items/list'">{{$t('Cancel')}}</v-btn>
        <v-spacer class="py-2"></v-spacer>
        <v-btn block color="primary" @click="ok()">{{$t('Save')}}</v-btn>
      </v-col>
    </v-row>
  </v-form>
</template>

<script>
import ValidationRules from "../../helpers/validation-rules";

export default {
  props: {
    data: {
      type: Object,
      default: null,
      required: true
    }
  },
  data() {
    return {
      model: { ...this.data },
      vr: ValidationRules,
      dictionaries: {},
      valid: true
    };
  },
  async mounted(){
    this.dictionaries = await this.$api.dictionaries.getTransactionDictionaries();
    if(!this.model.currency){
      this.model.currency = this.dictionaries.currencyEnum[0].code;
    }
  },
  methods: {
    ok() {
      if (!this.$refs.form.validate()) return;
      this.$emit('ok', this.model);
    }
  },
};
</script>

<style lang="scss" scoped>
</style>