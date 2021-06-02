<template>
  <div>
    <ec-dialog :dialog.sync="newCustomerDialog" color="ecbg">
      <template v-slot:title>{{$t('CreateNewCustomer')}}</template>
      <template>
        <v-form ref="newCustomerFormRef" lazy-validation>
          <v-row>
          <v-col cols="12" md="4" class="py-0">
            <v-text-field v-model="newCustomerModel.userName" :rules="[vr.primitives.required, vr.primitives.email]" :label="$t('Email')"></v-text-field>
          </v-col>
          <v-col cols="12" md="4" class="py-0">
            <v-text-field v-model="newCustomerModel.password" :rules="[vr.primitives.required, vr.primitives.stringLength(6, 64)]" :label="$t('Password')"></v-text-field>
          </v-col>
          <v-col cols="12" md="4" class="py-0">
            <v-text-field v-model="newCustomerModel.businessID" :rules="[vr.primitives.required, vr.primitives.stringLength(6, 64)]" :label="$t('BusinessID')"></v-text-field>
          </v-col>
        </v-row>
        </v-form>
        <div class="d-flex justify-end">
          <v-btn @click="newCustomerDialog = false" :loading="loading">{{$t("Cancel")}}</v-btn>
          <v-btn class="mx-1" color="primary" @click="createNewCustomer()" :loading="loading">{{$t("Save")}}</v-btn>
        </div>
      </template>
    </ec-dialog>
    <v-form v-model="formValid" lazy-validation>
      <v-row v-if="model.settings">
        <v-col cols="12" class="pt-0 text-end pb-4">
          <v-btn small color="secondary" class="mx-1" @click="openNewCustomerDialog()">{{$t("CreateNewCustomer")}}</v-btn>
        </v-col>
        <v-col cols="12" md="4" class="py-0">
          <v-text-field v-model="model.settings.keyStorePassword" :label="$t('KeyStorePassword')"></v-text-field>
        </v-col>
        <v-col cols="12" md="4" class="py-0">
          <v-text-field v-model="model.settings.userName" :label="$t('UserName')"></v-text-field>
        </v-col>
        <v-col cols="12" md="4" class="py-0">
          <v-text-field v-model="model.settings.password" :label="$t('Password')"></v-text-field>
        </v-col>
      </v-row>
      <div class="d-flex justify-end">
        <v-btn color="primary" @click="save()" :loading="loading">{{$t("Save")}}</v-btn>
      </div>
    </v-form>
  </div>
</template>

<script>
import ValidationRules from "../../helpers/validation-rules";

export default {
  components: {
    EcDialog: () => import("../../components/ec/EcDialog"),
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
      formValid: false,
      loading: false,
      newCustomerDialog: false,
      newCustomerModel: {
        userName: null,
        password: null
      },
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
    },
    openNewCustomerDialog() {
      this.newCustomerDialog = true;
    },
    async createNewCustomer(){
      if(!this.$refs.newCustomerFormRef.validate()){
        return;
      }
      this.loading = true;
      let payload = {
        terminalID: this.terminalId,
        ...this.newCustomerModel
      }
      let operation = await this.$api.integrations.easyInvoice.createCustomer(payload);
      if (!this.$apiSuccess(operation)) return;

      this.$toasted.show(operation.message, { type: "success" });
      this.model.settings.userName = operation.additionalData.userName;
      this.model.settings.password = operation.additionalData.password;
      this.model.settings.keyStorePassword = operation.additionalData.keyStorePassword;
      this.newCustomerModel.userName = null;
      this.newCustomerModel.password = null;
      this.newCustomerDialog = false;
      this.loading = false;
    }
  },
};
</script>