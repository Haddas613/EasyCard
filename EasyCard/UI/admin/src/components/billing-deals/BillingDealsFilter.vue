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
            v-model="model.cardOwnerNationalID"
            :label="$t('CardOwnerNationalID')"
          ></v-text-field>
        </v-col>
        <v-col cols="12" md="4" class="py-0">
          <v-text-field
            v-model="model.consumerName"
            :label="$t('CustomerName')"
          ></v-text-field>
        </v-col>
        <v-col cols="12" md="4" class="py-0">
          <v-text-field
            v-model="model.consumerEmail"
            :label="$t('CustomerEmail')"
          ></v-text-field>
        </v-col>
        <v-col cols="12" md="4" class="py-0">
          <v-select
            :items="dictionaries.quickDateFilterTypeEnum"
            item-text="description"
            item-value="code"
            v-model="model.quickDateFilter"
            :label="$t('UpdatedDate')"
            clearable
          ></v-select>
        </v-col>
        <v-col cols="12" md="4" class="py-0">
          <v-text-field
            v-model="model.consumerExternalReference"
            :label="$t('CustomerExternalReference')"
          ></v-text-field>
        </v-col>
      </v-row>
      <v-row>
        <v-col class="pb-0 mb-0" cols="4" md="12">
          <v-switch v-model="model.filterDateByNextScheduledTransaction" hide-details>
            <template v-slot:label>
              <small>{{$t('FilterDateByNextScheduledTransaction')}}</small>
            </template>
          </v-switch>
        </v-col>
        <date-from-to-filter class="px-3" v-model="model" :from-today="model.filterDateByNextScheduledTransaction "
          :date-from-label="model.filterDateByNextScheduledTransaction ? 'NextScheduledDateFrom' : 'CreatedFrom'"
          :date-to-label="model.filterDateByNextScheduledTransaction ? 'NextScheduledDateTo' : 'CreatedTo'"></date-from-to-filter>
      </v-row>
      <v-row class="d-flex">
        <v-col cols="3" md="2">
          <v-switch v-model="model.invoiceOnly">
            <template v-slot:label>
              <small>{{$t('InvoiceOnly')}}</small>
            </template>
          </v-switch>
        </v-col>
        <v-col cols="3" md="2">
          <v-switch v-model="model.hasError" @change="switchFilterChanged('hasError')">
            <template v-slot:label>
              <small>{{$t('HasError')}}</small>
            </template>
          </v-switch>
        </v-col>
        <v-col cols="3" md="2">
          <v-switch v-model="model.showDeleted" @change="switchFilterChanged('showDeleted')" true-value="1" false-value="0">
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
        <v-col cols="3" md="2">
          <v-switch v-model="model.inProgress" @change="switchFilterChanged('inProgress')">
            <template v-slot:label>
              <small>{{$t('InProgress')}}</small>
            </template>
          </v-switch>
        </v-col>
        <v-col cols="3" md="2">
          <v-switch v-model="model.creditCardExpired" @change="switchFilterChanged('creditCardExpired')">
            <template v-slot:label>
              <small>{{$t('CreditCardExpired')}}</small>
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
      let allTypes = ['showDeleted', 'actual', 'paused', 'finished', 'hasError', 'inProgress', 'creditCardExpired'].filter(v => v != type);
      for(var t of allTypes){
        if(t === "showDeleted"){
          this.$set(this.model, t, 0);
        }else{
          this.$set(this.model, t, false);
        }
      }
    }
  }
};
</script>