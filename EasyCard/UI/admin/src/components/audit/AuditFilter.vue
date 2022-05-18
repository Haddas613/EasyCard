<template>
  <v-container fluid class="py-0">
    <v-form :v-model="formIsValid" ref="form">
      <v-row>
        <v-col cols="12" md="4">
          <v-select
            hide-details="auto"
            :items="dictionaries.operationCodesEnum"
            item-text="description"
            item-value="code"
            v-model="model.code"
            :label="$t('Code')"
            clearable
          ></v-select>
        </v-col>
        <v-col cols="12" md="4">
          <ec-date-input :key="model.from" v-model="model.from" :label="$t('DateFrom')"></ec-date-input>
        </v-col>
        <v-col cols="12" md="4">
          <ec-date-input :key="model.to" v-model="model.to" :label="$t('DateTo')"></ec-date-input>
        </v-col>
        <v-col cols="12" md="4">
          <v-text-field
            hide-details="auto"
            :rules="[vr.primitives.guid]"
            v-model="model.terminalID"
            :label="$t('TerminalID')"
            clearable
          ></v-text-field>
        </v-col>
        <v-col cols="12" md="4">
          <v-text-field hide-details="auto" v-model="model.terminalName" :label="$t('TerminalName')"></v-text-field>
        </v-col>
        <v-col cols="12" md="4">
          <v-text-field
            hide-details="auto"
            :rules="[vr.primitives.guid]"
            v-model="model.merchantID"
            :label="$t('MerchantID')"
            clearable
          ></v-text-field>
        </v-col>
        <v-col cols="12" md="4">
          <v-text-field hide-details="auto" v-model="model.merchantName" :label="$t('MerchantName')"></v-text-field>
        </v-col>
        <v-col cols="12" md="4">
          <v-text-field
            hide-details="auto"
            :rules="[vr.primitives.guid]"
            v-model="model.userID"
            :label="$t('UserID')"
            clearable
          ></v-text-field>
        </v-col>
        <v-col cols="12" md="4">
          <v-text-field hide-details="auto" v-model="model.userName" :label="$t('UserName')"></v-text-field>
        </v-col>
      </v-row>
      <v-row>
        <v-col cols="12" class="d-flex justify-end">
          <v-btn color="primary" class="mx-2" outlined @click="clear()">{{$t('Clear')}}</v-btn>
          <v-btn color="success" class="mr-4" @click="apply()" :disabled="!formIsValid">{{$t('Apply')}}</v-btn>
        </v-col>
      </v-row>
    </v-form>
  </v-container>
</template>

<script>
import ValidationRules from "../../helpers/validation-rules";
export default {
  name: "AuditFilter",
  components: {
    MerchantTerminalFilter: () => import("../filtering/MerchantTerminalFilter"),
    EcDateInput: () => import("../inputs/EcDateInput"),
  },
  data() {
    return {
      model: { ...this.filterData },
      dictionaries: {},
      vr: ValidationRules,
      formIsValid: true,
    };
  },
  async mounted() {
    this.dictionaries = await this.$api.dictionaries.getMerchantDictionaries();
  },
  props: {
    filterData: {
      type: Object
    }
  },
  methods: {
    apply() {
      if(this.$refs.form.validate()){
        this.$emit("apply", this.model);
      }
    },
    clear(){
      this.model = {
        from: null,
        to: null
      };
    }
  }
};
</script>