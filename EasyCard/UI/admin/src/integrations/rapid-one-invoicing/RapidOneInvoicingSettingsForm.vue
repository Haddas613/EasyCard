<template>
  <v-form v-model="formValid" lazy-validation>
    <v-row v-if="model.settings">
      <v-col cols="12" md="5" class="py-0">
        <v-text-field v-model="model.settings.baseUrl" :error="apiError" :label="$t('URL')"></v-text-field>
      </v-col>
      <v-col cols="12" md="5" class="py-0">
        <v-text-field v-model="model.settings.token" :error="apiError" :label="$t('ApiKey')"></v-text-field>
      </v-col>
      <v-col cols="12" md="1">
        <v-btn :disabled="!authDataValid" color="success" small @click="reloadApiData()">
          {{$t("Apply")}}
          <v-icon right>mdi-refresh</v-icon>
        </v-btn>
      </v-col>
      <v-col cols="12" md="4" class="py-0">
        <v-select
          :items="companies"
          :disabled="requireReload"
          item-text="name"
          item-value="dbName"
          v-model="model.settings.company"
          :label="$t('Company')"
        ></v-select>
      </v-col>
      <v-col cols="12" md="4" class="py-0">
        <v-select
          :items="branches"
          :disabled="requireReload"
          item-text="name"
          item-value="branchID"
          v-model="model.settings.branch"
          :label="$t('Branch')"
          @change="getDepartments()"
        ></v-select>
      </v-col>
      <v-col cols="12" md="4" class="py-0">
        <v-select
          :items="departments"
          :disabled="requireReload"
          item-text="name"
          item-value="departmentID"
          v-model="model.settings.department"
          :label="$t('Department')"
        ></v-select>
      </v-col>
      <v-col cols="12" md="4" class="py-0">
        <v-text-field v-model="model.settings.ledgerAccount" :label="$t('LedgerAccount')"></v-text-field>
      </v-col>
      <v-col cols="12" md="4" class="py-0">
        <v-switch class="mt-4" v-model="model.settings.charge" :label="$t('Charge')"></v-switch>
      </v-col>
    </v-row>
    <div class="d-flex justify-end">
      <v-btn :color="apiError ? 'error' : 'primary'" :disabled="requireReload" @click="save()" :loading="loading">{{$t("Save")}}</v-btn>
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
      requireReload: false,
      vr: ValidationRules,
      authDataValid: false,
      apiError: false,
      branches: [],
      companies: [],
      departments: [],
    }
  },
  async mounted () {
    if(!this.model.settings){
      this.model.settings = {};
    }
    if(this.model.settings.token && this.isValidHttpUrl(this.model.settings.baseUrl)){
      this.authDataValid = true;
      await this.getDictionaries();
    }
  },
  watch: {
    'model.settings.token'(newValue, oldValue) {
      this.authDataValid = !!newValue && this.isValidHttpUrl(this.model.settings.baseUrl);
    },
    'model.settings.baseUrl'(newValue, oldValue) {
       this.authDataValid =  this.model.settings.token && this.isValidHttpUrl(newValue);
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
    async getDictionaries(){
      if(!this.authDataValid || this.loading){
        return;
      }
      this.loading = true;
      //check if first request is correct, only makes sense to do all of them in this case
      if(await this.getCompanies()){
        this.apiError = false;
        await this.getBranches();
        await this.getDepartments();
      }else{
        this.apiError = true;
      }
      this.loading = false;
    },
    async getCompanies(){
      if(!this.authDataValid){
        return false;
      }
      let result = await this.$api.integrations
        .rapidOne.getCompanies(this.model.settings.baseUrl, this.model.settings.token);
      
      if(!result.length) return false;
      this.companies = result;

      if(!this.model.settings.company){
        this.model.settings.company = this.companies[0].dbName;
      }
      return true;
    },
    async getBranches(){
      if(!this.authDataValid){
        return false;
      }
      let result = await this.$api.integrations
        .rapidOne.getBranches(this.model.settings.baseUrl, this.model.settings.token);

      if(!result.length) return false;
      this.branches = result;

      if(!this.model.settings.branch){
        this.model.settings.branch = this.branches[0].branchID;
      }
      return true;
    },
    async getDepartments(){
      if(!this.authDataValid || !this.model.settings.branch){
        return false;
      }
      let result = await this.$api.integrations
        .rapidOne.getDepartments(this.model.settings.baseUrl, this.model.settings.token, this.model.settings.branch);

      if(!result.length) return false;
      this.departments = result;

      if(!this.model.settings.department){
        this.model.settings.department = this.departments[0].departmentID;
      }
      return true;
    },
    async reloadApiData(){
      await this.getDictionaries();
    },
    isValidHttpUrl(string) {
      if(!string){
        return false;
      }
      let url;
      
      try {
        url = new URL(string);
      } catch (_) {
        return false;  
      }
      return url.protocol === "http:" || url.protocol === "https:";
    }
  },
};
</script>