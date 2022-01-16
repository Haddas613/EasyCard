<template>
  <v-form v-model="formValid">
    <integration-ready-check v-if="apiName === appConstants.terminal.api.terminals" :integration="model" @test="testConnection()"></integration-ready-check>
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
import appConstants from "../../helpers/app-constants";

export default {
  components: {
    EcDialog: () => import("../../components/ec/EcDialog"),
    IntegrationReadyCheck: () => import("../../components/integrations/IntegrationReadyCheck"),
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
    async save() {
      if (!this.formValid || this.loading) {
        return;
      }
      this.loading = true;
      await this.$api[this.apiName].saveExternalSystem(this.terminalId, this.model);
      this.loading = false;
    },
    async testConnection(){
      let operation = await this.$api.integrations.clearingHouse.testConnection({
        ...this.model,
        terminalID: this.terminalId,
      });
      if(!this.$apiSuccess(operation)){
        this.$toasted.show(operation.message, { type: "error" })
        this.model.valid = false;
      }else{
        this.model.valid = true;
        await this.save();
      }
      this.$emit('update', this.model);
    },
  },
};
</script>