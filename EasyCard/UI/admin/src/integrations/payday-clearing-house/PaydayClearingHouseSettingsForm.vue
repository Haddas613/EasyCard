<template>
  <v-form v-model="formValid" lazy-validation>
    <ec-dialog :dialog.sync="loadMerchantDialog" color="ecbg">
      <template v-slot:title>{{$t('LoadMerchantData')}}</template>
      <template>
        <v-form ref="loadMerchantFormRef" lazy-validation>
          <v-row>
            <v-col cols="12" class="py-0">
              <v-text-field v-model="loadMerchantID" :rules="[vr.primitives.required, vr.primitives.stringLength(3, 64)]" :label="$t('MerchantID')" outlined></v-text-field>
            </v-col>
          </v-row>
        </v-form>
        <v-row v-if="loadedMerchantData">
          <v-col cols="12" md="6">
            <v-text-field v-model="loadedMerchantData.userName" disabled :label="$t('UserName')" outlined></v-text-field>
          </v-col>
          <v-col cols="12" md="6">
            <v-text-field v-model="loadedMerchantData.email" disabled :label="$t('Email')" outlined></v-text-field>
          </v-col>
          <v-col cols="12" md="6">
            <v-text-field v-model="loadedMerchantData.merchantReference" disabled :label="$t('MerchantReference')" outlined></v-text-field>
          </v-col>
          <v-col cols="12" md="6">
            <v-text-field v-model="loadedMerchantData.shvaTerminalReference" disabled :label="$t('ShvaTerminalReference')" outlined></v-text-field>
          </v-col>
        </v-row>
        <div class="d-flex justify-end">
          <v-btn @click="loadMerchantDialog = false" :loading="loading">{{$t("Cancel")}}</v-btn>
          <v-btn class="mx-1" color="secondary" @click="loadMerchantData()" :loading="loading" :disabled="!loadMerchantID">{{$t("LoadMerchantData")}}</v-btn>
          <v-btn color="primary" :loading="loading" @click="saveWithMerchantData()" :disabled="!loadedMerchantData">{{$t("Save")}}</v-btn>
        </div>
      </template>
    </ec-dialog>
    <v-row v-if="model.settings">
      <v-col cols="12" class="pt-0 text-end pb-4">
        <v-btn small class="mx-1" color="secondary" @click="loadMerchantDialog = true;">{{$t("LoadMerchantData")}}</v-btn>
      </v-col>
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
      loadMerchantDialog: false,
      loadMerchantID: null,
      loadedMerchantData: null,
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
    async loadMerchantData(){
      if(!this.$refs.loadMerchantFormRef.validate()){
        return;
      }

      this.loadedMerchantData = await this.$api.integrations.clearingHouse.getCustomerData(this.loadMerchantID)
    },
    async saveWithMerchantData(){
      this.model.settings.merchantReference = this.loadedMerchantData.merchantReference;
      this.model.settings.shvaTerminalReference = this.loadedMerchantData.shvaTerminalReference;
      this.loadMerchantDialog = false;
      await this.save();
    }
  },
};
</script>