<template>
  <v-container fluid>
    <v-form :v-model="formIsValid" ref="form">
      <v-row>
        <v-col cols="12" md="6" class="pb-0">
          <v-text-field v-model="model.search" :label="$t('Search')"></v-text-field>
        </v-col>
        <v-col cols="12" md="6" class="pb-0">
          <v-text-field v-model="model.merchantID" :label="$t('MerchantID')" :rules="[vr.primitives.guid]"></v-text-field>
        </v-col>
      </v-row>
      <v-row>
        <v-col cols="12" class="d-flex justify-end">
          <v-btn color="success" class="mr-4" @click="apply()" :disabled="!formIsValid">{{$t('Apply')}}</v-btn>
        </v-col>
      </v-row>
    </v-form>
  </v-container>
</template>

<script>
import ValidationRules from "../../helpers/validation-rules";

export default {
  name: "MerchantsFilter",
  components: {
    MerchantTerminalFilter: () => import("../filtering/MerchantTerminalFilter")
  },
  data() {
    return {
      model: { ...this.filterData },
      dictionaries: {},
      vr: ValidationRules,
      formIsValid: true,
    };
  },
  async mounted() {
    this.dictionaries = await this.$api.dictionaries.getMerchantDictionaries();
  },
  props: {
    filterData: {
      type: Object
    }
  },
  mounted () {
    window.addEventListener("keydown", this.handleKeyPress);
  },
  destroyed () {
    window.removeEventListener("keydown", this.handleKeyPress);
  },
  methods: {
    apply() {
      if(!this.$refs.form.validate()){
        return;
      }
      this.$emit("apply", this.model);
    },
    handleKeyPress($event){
      if($event.key === "Enter"){
        this.apply();
      }
    },
  }
};
</script>