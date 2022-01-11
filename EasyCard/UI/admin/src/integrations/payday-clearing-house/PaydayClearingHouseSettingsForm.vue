<template>
  <v-form v-model="formValid" lazy-validation>
    <ec-dialog :dialog.sync="loadMerchantDialog">
      <template v-slot:title>{{$t('LoadMerchantData')}}</template>
      <template>
        <v-form ref="loadMerchantFormRef" lazy-validation>
          <v-row>
            <v-col cols="12" md="6" class="py-0">
              <v-text-field v-model="loadMerchantParams.merchantID" type="number" :rules="[vr.primitives.biggerThan(0)]" :label="$t('MerchantID')" ></v-text-field>
            </v-col>
            <v-col cols="12" md="6" class="py-0">
              <v-text-field v-model="loadMerchantParams.merchantName" :rules="[vr.primitives.stringLength(3, 64)]" :label="$t('MerchantName')" ></v-text-field>
            </v-col>
            <v-col cols="12">
              <v-select
                :items="loadedMerchantsData"
                item-text="merchantName"
                return-object
                v-model="selectedMerchant"
                :label="$t('Merchants')"
              >
              </v-select>
            </v-col>
          </v-row>
        </v-form>
        <v-card v-if="selectedMerchant" class="mb-4" tile>
          <v-card-text>
            <v-row>
              <v-col cols="12" md="6">
                <v-text-field :value="selectedMerchant.merchantName" disabled :label="$t('UserName')"></v-text-field>
              </v-col>
              <v-col cols="12" md="6">
                <v-text-field :value="selectedMerchant.businessId" disabled :label="$t('BusinessID')"></v-text-field>
              </v-col>
              <v-col cols="12" md="6">
                <v-text-field :value="selectedMerchant.phone" disabled :label="$t('Phone')"></v-text-field>
              </v-col>
              <v-col cols="12" md="6">
                <v-text-field :value="selectedMerchant.email" disabled :label="$t('Email')"></v-text-field>
              </v-col>
              <v-col cols="12" md="12">
                <v-text-field :value="selectedMerchant.merchantReference" disabled :label="$t('MerchantReference')"></v-text-field>
              </v-col>
              <v-col cols="12" md="4">
                <v-text-field :value="selectedMerchant.nikionEnabled" disabled :label="$t('NikionEnabled')"></v-text-field>
              </v-col>
              <v-col cols="12" md="4">
                <v-text-field :value="selectedMerchant.ravMotav" disabled :label="$t('RavMotav')"></v-text-field>
              </v-col>
              <v-col cols="12" md="4">
                <v-text-field :value="selectedMerchant.parentMerchantID != null" disabled :label="$t('IsTerminal')"></v-text-field>
              </v-col>
            </v-row>
          </v-card-text>
        </v-card>
        <div class="d-flex justify-end">
          <v-btn @click="loadMerchantDialog = false" :loading="loading">{{$t("Cancel")}}</v-btn>
          <v-btn class="mx-1" color="secondary" @click="loadMerchantData()" :loading="loading" :disabled="!formValid">{{$t("LoadMerchantData")}}</v-btn>
          <v-btn color="primary" :loading="loading" @click="saveWithMerchantData()" :disabled="!selectedMerchant">{{$t("Save")}}</v-btn>
        </div>
      </template>
    </ec-dialog>
    <v-row v-if="model.settings">
      <v-col cols="12" class="pt-0 text-end pb-4">
        <v-btn small class="mx-1" color="secondary" @click="loadMerchantDialog = true;">{{$t("LoadMerchantData")}}</v-btn>
      </v-col>
      <v-col cols="12" md="4" class="py-0">
        <v-text-field v-model="model.settings.merchantID" type="number" disabled :label="$t('MerchantID')" ></v-text-field>
      </v-col>
      <v-col cols="12" md="4" class="py-0">
        <v-text-field v-model="model.settings.merchantReference" :label="$t('MerchantReference')" ></v-text-field>
      </v-col>
      <v-col cols="12" md="4" class="py-0">
        <v-text-field v-model="model.settings.shvaTerminalReference" :label="$t('ShvaTerminalReference')" ></v-text-field>
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
      loadMerchantParams: {
        merchantID: null,
        merchantName: null
      },
      loadedMerchantsData: [],
      selectedMerchant: null,
      vr: ValidationRules
    }
  },
  mounted () {
    if(!this.model.settings){
      this.model.settings = {};
    }
  },
  methods: {
    async save() {
      if(!this.formValid){
        return;
      }
      this.loading = true;
      await this.$api[this.apiName].saveExternalSystem(this.terminalId, this.model);
      this.loading = false;
    },
    async loadMerchantData(){
      if(!this.$refs.loadMerchantFormRef.validate()){
        return;
      }

      let merchants = await this.$api.integrations.clearingHouse.getCustomerData(this.loadMerchantParams);
      this.loadedMerchantsData = merchants.data || [];
      this.selectedMerchant = this.loadedMerchantsData[0];
    },
    async saveWithMerchantData(){
      this.model.settings.merchantReference = this.selectedMerchant.merchantReference;
      this.model.settings.merchantID = this.selectedMerchant.merchantID;
      this.loadMerchantDialog = false;
      await this.save();
    }
  },
};
</script>