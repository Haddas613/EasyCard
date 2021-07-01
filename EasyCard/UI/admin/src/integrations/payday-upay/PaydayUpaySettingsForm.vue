<template>
  <v-form v-model="formValid">
    <v-row v-if="model.settings" class="pt-2">
      <v-col cols="12" md="6" class="py-0">
        <v-text-field v-model="model.settings.email" :label="$t('Email')" :rules="[vr.primitives.required, vr.primitives.email]"></v-text-field>
      </v-col>
      <v-col cols="12" md="6" class="py-0">
        <v-text-field v-model="model.settings.password" :label="$t('Password')" :rules="[vr.primitives.required]"></v-text-field>
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
  components: {
    EcDialog: () => import("../../components/ec/EcDialog")
  },
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
      vr: ValidationRules,
      formValid: false,
      loading: false,
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