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
          <v-btn @click="newCustomerDialog = false" :disabled="loading">{{$t("Cancel")}}</v-btn>
          <v-btn class="mx-1" color="primary" @click="createNewCustomer()" :disabled="loading">{{$t("Save")}}</v-btn>
        </div>
      </template>
    </ec-dialog>
    <ec-dialog :dialog.sync="documentNumberDialog" color="ecbg">
      <template v-slot:title>{{$t('SetDocumentNumber')}}</template>
      <template>
        <v-form ref="documentNumberFormRef" lazy-validation>
          <v-row>
          <v-col cols="12" md="4" class="py-0">
            <v-select
              :items="ecInvoiceTypes"
              item-text="description"
              item-value="code"
              v-model="documentNumberModel.docType"
              :label="$t('InvoiceType')"
              :rules="[vr.primitives.required]"
              @change="getDocumentNumber()"
            ></v-select>
          </v-col>
          <v-col cols="12" md="4" class="py-0">
            <v-text-field v-model.number="documentNumberModel.currentNum" :rules="[vr.primitives.required, vr.primitives.numeric(), vr.primitives.biggerThan(documentNumberModel.minNum, true)]" :label="$t('Number')"></v-text-field>
          </v-col>
        </v-row>
        </v-form>
        <div class="d-flex justify-end">
          <v-btn @click="documentNumberDialog = false" :disabled="loading">{{$t("Cancel")}}</v-btn>
          <v-btn class="mx-1" color="primary" @click="setDocumentNumber()" :disabled="loading">{{$t("Save")}}</v-btn>
        </div>
      </template>
    </ec-dialog>
    <v-form v-model="formValid" lazy-validation>
      <v-row v-if="model.settings">
        <v-col v-if="!isTemplate" cols="12" class="pt-0 text-end pb-4">
          <v-btn small color="secondary" class="mx-1" @click="openNewCustomerDialog()">{{$t("CreateNewCustomer")}}</v-btn>
          <v-btn small class="mx-1" @click="documentNumberDialog = true;">{{$t("SetDocumentNumber")}}</v-btn>
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
import appConstants from "../../helpers/app-constants";

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
      documentNumberModel: {
        docType: null,
        currentNum: null,
        minNum: 0
      },
      ecInvoiceTypes: {},
      vr: ValidationRules,
      documentNumberDialog: false
    }
  },
  async mounted () {
    if(!this.model.settings){
      this.model.settings = {};
    }

    this.ecInvoiceTypes = await this.$api.integrations.easyInvoice.getDocumentTypes();
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
      this.loading = false;

      if (!this.$apiSuccess(operation)) return;

      this.$toasted.show(operation.message, { type: "success" });
      this.model.settings.userName = operation.additionalData.userName;
      this.model.settings.password = operation.additionalData.password;
      this.model.settings.keyStorePassword = operation.additionalData.keyStorePassword;
      this.newCustomerModel.userName = null;
      this.newCustomerModel.password = null;
      this.newCustomerDialog = false;
    },
    async getDocumentNumber(){
      if (!this.documentNumberModel.docType){
        return;
      }

      let res = await this.$api.integrations.easyInvoice.getDocumentNumber(this.terminalId, this.documentNumberModel.docType);
      if (res && (res.entityReference || res.entityReference === 0)){
        this.documentNumberModel.currentNum = res.entityReference;
        this.documentNumberModel.minNum = res.entityReference;
      }
    },
    async setDocumentNumber(){
      if (!this.$refs.documentNumberFormRef.validate()){
        return;
      }
      
      await this.$api.integrations.easyInvoice.setDocumentNumber({
        terminalID: this.terminalId,
        docType: this.documentNumberModel.docType,
        currentNum: this.documentNumberModel.currentNum
      });

      this.documentNumberDialog = false;
    }
  },
  computed: {
    isTemplate() {
      return this.apiName == appConstants.terminal.api.terminalTemplates;
    }
  },
};
</script>