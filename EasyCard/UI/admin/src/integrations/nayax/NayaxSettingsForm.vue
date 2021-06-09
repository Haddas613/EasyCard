<template>
  <v-form v-model="formValid">
    <ec-dialog :dialog.sync="authenticateDeviceDialog" color="ecbg">
      <template v-slot:title>{{$t('AuthenticateDevice')}}</template>
      <template>
        <v-form ref="authenticateDeviceDialogFormRef" v-model="otpFormCorrect">
          <v-row>
            <v-col cols="12" class="pt-4 pb-0 mb-0">
              <v-text-field v-model="authenticateDeviceOTP" :label="$t('OTP')" :rules="[vr.primitives.required]"></v-text-field>
            </v-col>
          </v-row>
        </v-form>
        <div class="d-flex justify-end">
          <v-btn @click="authenticateDeviceDialog = false" :loading="loading">{{$t("Cancel")}}</v-btn>
          <v-btn class="mx-1" color="primary" @click="authenticateDevice()" :loading="loading" :disabled="!otpFormCorrect">{{$t("OK")}}</v-btn>
        </div>
      </template>
    </ec-dialog>
    <v-row v-if="model.settings" class="pt-2">
      <v-col cols="12" class="pt-0 text-end pb-4">
        <v-btn small color="secondary" class="mx-1" @click="pairDevice()" :disabled="!formValid">{{$t("PairDevice")}}</v-btn>
      </v-col>
      <v-col cols="12" md="6" class="py-0">
        <v-text-field v-model="model.settings.terminalID" :label="$t('TerminalID')" :rules="[vr.primitives.required]"></v-text-field>
      </v-col>
      <v-col cols="12" md="6" class="py-0">
        <v-text-field v-model="model.settings.posName" :label="$t('PaxDeviceLabel')" :rules="[vr.primitives.required]"></v-text-field>
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
      authenticateDeviceOTP: null,
      authenticateDeviceDialog: false,
      otpFormCorrect: false
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
    async pairDevice(){
      let payload = {
        ecTerminalID: this.terminalId,
        ...this.model.settings
      };
      let operation = await this.$api.integrations.nayax.pairDevice(payload);
      if (!this.$apiSuccess(operation)) return;

      this.authenticateDeviceDialog = true;
    },
    async authenticateDevice(){
      let payload = {
        ecTerminalID: this.terminalId,
        ...this.model.settings,
        OTP: this.authenticateDeviceOTP
      };

      let operation = await this.$api.integrations.nayax.authenticateDevice(payload);
      if (!this.$apiSuccess(operation)) return;

      this.authenticateDeviceDialog = false;
      this.authenticateDeviceOTP = null;
    }
  },
};
</script>