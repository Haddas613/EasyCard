<template>
  <div>
    <ec-dialog :dialog.sync="newPasswordDialog" color="ecbg">
      <template v-slot:title>{{$t('SetNewPassword')}}</template>
      <template>
        <v-alert class="pt-4" border="top" colored-border type="warning" elevation="2">
          {{$t("ShvaNewPasswordInfoMessage")}}
        </v-alert>
        <v-form ref="newPasswordFormRef" lazy-validation>
          <v-row>
            <v-col cols="12" class="pt-4 pb-0 mb-0">
              <v-text-field
                v-model="model.settings.merchantNumber"
                :label="$t('ShvaTerminalReference')"
                disabled
              ></v-text-field>
            </v-col>
            <v-col cols="12" md="6">
              <v-text-field v-model="model.settings.userName" :rules="[vr.primitives.required, vr.primitives.stringLength(3, 64)]" :label="$t('UserName')"></v-text-field>
            </v-col>
            <v-col cols="12" md="6">
              <v-text-field v-model="newPasswordModel" :rules="[vr.primitives.required, vr.primitives.stringLength(6, 64)]" :label="$t('NewPassword')"></v-text-field>
            </v-col>
          </v-row>
        </v-form>
        <div class="d-flex justify-end">
          <v-btn @click="newPasswordDialog = false" :loading="loading">{{$t("Cancel")}}</v-btn>
          <v-btn class="mx-1" color="primary" @click="setNewPassword()" :loading="loading">{{$t("Save")}}</v-btn>
        </div>
      </template>
    </ec-dialog>
    <v-form v-model="formValid" lazy-validation>
      <v-row v-if="model.settings">
        <v-col cols="12" class="pt-0 text-end pb-4">
          <v-btn small color="success" @click="testConnection()">{{$t("TestConnection")}}</v-btn>
          <v-btn small color="secondary" class="mx-1" @click="openNewPasswordDialog()">{{$t("SetNewPassword")}}</v-btn>
        </v-col>
        <v-col cols="12" md="4" class="py-0">
          <v-text-field v-model="model.settings.userName" :label="$t('SHVATerminalName')"></v-text-field>
        </v-col>
        <v-col cols="12" md="4" class="py-0">
          <v-text-field v-model="model.settings.password" :label="$t('Password')"></v-text-field>
        </v-col>
        <v-col cols="12" md="4" class="py-0">
          <v-text-field
            v-model="model.settings.merchantNumber"
            :label="$t('ShvaTerminalReference')"
          ></v-text-field>
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
      formValid: false,
      loading: false,
      newPasswordDialog: false,
      newPasswordModel: null,
      vr: ValidationRules
    };
  },
  mounted() {
    if (!this.model.settings) {
      this.model.settings = {};
    }
  },
  methods: {
    save() {
      if (!this.formValid) {
        return;
      }
      this.loading = true;
      this.$api[this.apiName].saveExternalSystem(this.terminalId, this.model);
      this.loading = false;
    },
    openNewPasswordDialog() {
      this.newPasswordModel = null;
      this.newPasswordDialog = true;
    },
    async setNewPassword(){
      if(!this.$refs.newPasswordFormRef.validate()){
        return;
      }
      this.loading = true;
      let payload = {
        newPassword: this.newPasswordModel,
        terminalID: this.apiName == 'terminals' ? this.terminalId : null,
        terminalTemplateID: this.apiName == 'terminalTemplates' ? this.terminalId : null
      }
      let operation = await this.$api.integrations.shva.setNewPassword(payload);

      if(operation && operation.status == "success"){
        this.model.settings.password = this.newPasswordModel;
        this.newPasswordDialog = false;
      }else if(operation && operation.message){
        this.$toasted.show(operation.message, { type: 'error' });
      }
      this.loading = false;
    },
    async testConnection(){
      await this.$api.integrations.shva.testConnection(this.model);
    }
  }
};
</script>