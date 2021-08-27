<template>
  <v-form v-model="formValid" lazy-validation>
    <v-row v-if="model.settings">
      <v-col cols="12" md="6" class="py-0">
        <v-text-field v-model="model.settings.baseUrl" :label="$t('URL')"></v-text-field>
      </v-col>
      <v-col cols="12" md="6" class="py-0">
        <v-text-field v-model="model.settings.token" :label="$t('ApiKey')"></v-text-field>
      </v-col>
      <v-col cols="12" md="4" class="py-0">
        <v-text-field v-model="model.settings.branch" type="number" :rules="[vr.primitives.numeric()]" :label="$t('Branch')"></v-text-field>
      </v-col>
      <v-col cols="12" md="4" class="py-0">
        <v-text-field v-model="model.settings.department" type="number" :rules="[vr.primitives.numeric()]" :label="$t('Department')"></v-text-field>
      </v-col>
      <v-col cols="12" md="4" class="py-0">
        <v-text-field v-model="model.settings.company" :label="$t('Company')"></v-text-field>
      </v-col>
      <v-col cols="12" md="4" class="py-0">
        <v-text-field v-model="model.settings.ledgerAccount" :label="$t('LedgerAccount')"></v-text-field>
      </v-col>
      <v-col cols="12" md="4" class="py-0">
        <v-switch class="mt-4" v-model="model.settings.charge" :label="$t('Charge')"></v-switch>
      </v-col>
    </v-row>
    <div class="d-flex justify-end">
      <v-btn color="primary" @click="save()" :loading="loading">{{$t("Save")}}</v-btn>
    </div>
  </v-form>
</template>

<script>
import ValidationRules from "../../helpers/validation-rules";
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
      loading: false,
      vr: ValidationRules
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