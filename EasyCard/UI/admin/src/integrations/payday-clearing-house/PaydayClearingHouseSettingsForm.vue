<template>
  <v-form v-model="formValid" lazy-validation>
    <v-row v-if="model.settings">
      <v-col cols="12" md="6" class="py-0">
        <v-text-field v-model="model.settings.merchantReference" :label="$t('MerchantReference')" outlined></v-text-field>
      </v-col>
      <v-col cols="12" md="6" class="py-0">
        <v-text-field v-model="model.settings.shvaTerminalReference" :label="$t('ShvaTerminalReference')" outlined></v-text-field>
      </v-col>
    </v-row>
    <div class="d-flex justify-end">
      <v-btn color="primary" @click="save()" :loading="loading">{{$t("Save")}}</v-btn>
    </div>
  </v-form>
</template>

<script>
export default {
  props: {
    data: {
      type: Object,
      required: true
    },
    terminalId: {
      required: true
    },
    apiName: {
      type: String,
      required: true
    }
  },
  data() {
    return {
      model: {
        ...this.data
      },
      formValid: false,
      loading: false
    }
  },
  mounted () {
    if(!this.model.settings){
      this.model.settings = {};
    }
  },
  methods: {
    save() {
      if(!this.formValid){
        return;
      }
      this.loading = true;
      this.$api[this.apiName].saveExternalSystem(this.terminalId, this.model);
      this.loading = false;
    }
  },
};
</script>