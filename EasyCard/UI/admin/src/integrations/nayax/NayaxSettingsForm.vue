<template>
  <v-form v-model="formValid">
    <integration-ready-check v-if="apiName === appConstants.terminal.api.terminals" :integration="model" @test="testConnection()"></integration-ready-check>
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
    <v-row class="pt-2" v-if="model.settings">
      <v-col cols="6">
        <v-select :items="model.settings.devices" v-model="selectedDevice" return-object :item-value="deviceValue">
          <template v-slot:item="{ item }">
            {{item.terminalID + '-' + item.posName}}
          </template>
          <template v-slot:selection="{ item }">
            {{item.terminalID + '-' + item.posName}}
          </template>
        </v-select>
      </v-col>
      <v-col cols="6" class="text-end" v-if="!isTemplate">
        <v-btn small color="success" class="mx-1" @click="addDevice()" :disabled="!formValid || (selectedDevice && !selectedDevice.terminalID)">{{$t("Add")}}</v-btn>
        <v-btn small color="secondary" class="mx-1" @click="pairDevice()" :disabled="!selectedDevice || !formValid">{{$t("PairDevice")}}</v-btn>
        <v-btn small color="error" class="mx-1" @click="removeCurrentDevice()" :disabled="!selectedDevice">{{$t("Delete")}}</v-btn>
      </v-col>
    </v-row>
    <v-row v-if="selectedDevice" class="pt-2">
      <v-col cols="12" md="6" class="py-0">
        <v-text-field v-model="selectedDevice.terminalID" :label="$t('TerminalID')" :rules="[vr.primitives.required, deviceExists]"></v-text-field>
      </v-col>
      <v-col cols="12" md="6" class="py-0">
        <v-text-field v-model="selectedDevice.posName" :label="$t('PaxDeviceLabel')" :rules="[vr.primitives.required]"></v-text-field>
      </v-col>
    </v-row>
    <div class="d-flex justify-end">
      <v-btn color="primary" @click="save()" :loading="loading" :disabled="!formValid">{{$t("Save")}}</v-btn>
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
      authenticateDeviceOTP: null,
      authenticateDeviceDialog: false,
      otpFormCorrect: false,
      selectedDevice: null,
      appConstants: appConstants,
    }
  },
  mounted () {
    if(!this.model.settings){
      this.model.settings = {};
    }
    if(!this.model.settings.devices){
      this.$set(this.model.settings, 'devices', []);
    }else{
      this.selectedDevice = this.model.settings.devices[0];
    }
  },
  methods: {
    async save(showSuccessToastr = true) {
      if(this.loading || !this.formValid){
        return;
      }
      this.loading = true;
      await this.$api[this.apiName].saveExternalSystem(this.terminalId, this.model, showSuccessToastr);
      this.loading = false;
    },
    async pairDevice(){
      await this.save(false);
      let payload = {
        ecTerminalID: this.terminalId,
        terminalID: this.selectedDevice.terminalID,
        posName: this.selectedDevice.posName,
      };
      let operation = await this.$api.integrations.nayax.pairDevice(payload);
      if (!this.$apiSuccess(operation)){
        this.$toasted.show(operation.message, { type: "error" })
        return;
      };

      this.authenticateDeviceDialog = true;
    },
    async authenticateDevice(){
      let payload = {
        ecTerminalID: this.terminalId,
        terminalID: this.selectedDevice.terminalID,
        posName: this.selectedDevice.posName,
        OTP: this.authenticateDeviceOTP
      };

      let operation = await this.$api.integrations.nayax.authenticateDevice(payload);
      if (!this.$apiSuccess(operation)) return;

      this.authenticateDeviceDialog = false;
      this.authenticateDeviceOTP = null;
    },
    async removeCurrentDevice(){
      let idx = this.model.settings.devices.findIndex(i => this.deviceValue(i) === this.deviceValue(this.selectedDevice));
      if (idx > -1){
        this.model.settings.devices.splice(idx, 1);
        this.selectedDevice = (--idx>=0) ? this.model.settings.devices[idx] : this.model.settings.devices[0];
      }
      await this.save(false);
    },
    async addDevice(){
      let idx = this.model.settings.devices.push({
        terminalID: null,
        posName: null
      });
      this.selectedDevice = this.model.settings.devices[idx - 1];
    },
    deviceValue(a){
      return `${a.terminalID}-${a.posName}`;
    },
    deviceExists(val){
      if(!val || !this.model.settings.devices.length){
        return false;
      }
      
      return (this.lodash.countBy(this.model.settings.devices, d => d.terminalID  == val).true < 2) || this.$t("AlreadyPresent");
    },
    async testConnection(){
      let operation = await this.$api.integrations.nayax.testConnection({
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
  computed: {
    isTemplate() {
      return this.apiName == appConstants.terminal.api.terminalTemplates;
    }
  },
};
</script>