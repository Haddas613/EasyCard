<template>
  <v-container fluid>
    <v-form ref="form" v-model="formIsValid">
      <v-row>
        <merchant-terminal-filter v-model="model"></merchant-terminal-filter>
        <v-col cols="12" md="4" class="py-0">
          <v-text-field
            v-model="model.billingDealID"
            :label="$t('BillingDealID')"
          ></v-text-field>
        </v-col>
        <v-col cols="12" md="4" class="py-0">
          <v-text-field
            v-model="model.cardOwnerNationalID"
            :label="$t('CardOwnerNationalID')"
          ></v-text-field>
        </v-col>
        <v-col cols="12" md="4" class="py-0">
          <v-select
            :items="paymentTypesFiltered"
            item-text="description"
            item-value="code"
            v-model="model.paymentType"
            :label="$t('PaymentType')"
            hide-details="true"
            clearable
          ></v-select>
        </v-col>
        <v-col cols="12" md="4" class="py-0">
          <v-text-field
            v-model="model.cardOwnerName"
            :label="$t('CardOwnerName')"
          ></v-text-field>
        </v-col>
        <v-col cols="12" md="4" class="py-0">
          <v-text-field
            v-model="model.consumerEmail"
            :label="$t('CustomerEmail')"
          ></v-text-field>
        </v-col>
      </v-row>
      <v-row>
        <v-col cols="12" md="4" class="py-0">
          <v-select
            :items="dictionaries.quickDateFilterTypeEnum"
            item-text="description"
            item-value="code"
            v-model="model.quickDateFilter"
            :label="$t('QuickDate')"
            clearable
          ></v-select>
        </v-col>
        <date-from-to-filter v-model="model"></date-from-to-filter>
      </v-row>
      <v-row class="d-flex" justify="end">
        <v-col cols="3" md="2">
          <v-switch v-model="model.showDeleted" @change="switchFilterChanged('showDeleted')">
            <template v-slot:label>
              <small>{{$t('Inactive')}}</small>
            </template>
          </v-switch>
        </v-col>
        <v-col cols="3" md="2">
          <v-switch v-model="model.paused" @change="switchFilterChanged('paused')">
            <template v-slot:label>
              <small>{{$t('Paused')}}</small>
            </template>
          </v-switch>
        </v-col>
        <v-col cols="3" md="2">
          <v-switch v-model="model.finished" @change="switchFilterChanged('finished')">
            <template v-slot:label>
              <small>{{$t('Finished')}}</small>
            </template>
          </v-switch>
        </v-col>
      </v-row>
      <v-row>
        <v-col cols="12" class="d-flex justify-end">
          <v-btn
            color="success"
            class="mr-4"
            @click="apply()"
            :disabled="!formIsValid"
          >{{$t('Apply')}}</v-btn>
        </v-col>
      </v-row>
    </v-form>
  </v-container>
</template>

<script>
import ValidationRules from "../../helpers/validation-rules";

export default {
  name: "MerchantsFilter",
  components: {
    MerchantTerminalFilter: () => import("../filtering/MerchantTerminalFilter"),
    DateFromToFilter: () => import("../filtering/DateFromToFilter"),
  },
  data() {
    return {
      model: { ...this.filterData },
      dictionaries: {},
      vr: ValidationRules,
      formIsValid: true,
      paymentTypesFiltered: []
    };
  },
  async mounted() {
    this.dictionaries = await this.$api.dictionaries.getTransactionDictionaries();
    this.paymentTypesFiltered = this.lodash.filter(this.dictionaries.paymentTypeEnum, e => e.code == "bank" || e.code == "card");
  },
  props: {
    filterData: {
      type: Object
    }
  },
  methods: {
    apply() {
      if (!this.$refs.form.validate()) {
        return;
      }
      this.$emit("apply", {
        ...this.model,
      });
    },
    async switchFilterChanged(type){
      let allTypes = ['showDeleted', 'actual', 'paused', 'finished'].filter(v => v != type);
      for(var t of allTypes){
        this.$set(this.model, t, false);
      }
    }
  }
};
</script>