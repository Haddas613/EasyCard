<template>
  <ec-dialog :dialog.sync="visible">
    <template v-slot:title>{{$t('FilterBillingDeals')}}</template>
    <template v-slot:right>
      <v-btn color="primary" @click="apply()" :disabled="!formIsValid">{{$t('Apply')}}</v-btn>
    </template>
    <template>
      <div class="d-flex px-4 py-2 justify-end">
        <v-btn color="primary" 
          :block="$vuetify.breakpoint.smAndDown" 
          outlined
          @click="model = {}">{{$t("Reset")}}</v-btn>
      </div>
      <div class="px-4 py-2">
        <v-form ref="form" v-model="formIsValid">
          <v-row>
            <v-col cols="12" md="12" class="py-0">
              <payment-type v-model="model.paymentType" :exclude-types="['cash', 'cheque']" all></payment-type>
            </v-col>
            <!-- <v-col cols="12" md="12" class="py-0">
              <v-switch
                v-model="model.finished"
                :label="$t('Finished')"
              ></v-switch>
            </v-col> -->
            <v-col cols="12" md="6" class="py-0">
              <v-text-field
                v-model="model.billingDealID"
                :label="$t('BillingDealID')"
                :rules="[vr.primitives.guid]"
                outlined
              ></v-text-field>
            </v-col>
            <v-col cols="12" md="6" class="py-0">
              <terminal-select v-model="model.terminalID" clearable show-deleted></terminal-select>
            </v-col>
            <v-col cols="12" md="6" class="py-0">
              <v-select
                :items="dictionaries.currencyEnum"
                item-text="description"
                item-value="code"
                v-model="model.currency"
                :label="$t('Currency')"
                outlined
                clearable
                hide-details
              ></v-select>
            </v-col>
            <v-col cols="12" md="6" class="py-0">
              <v-select
                :items="dictionaries.quickDateFilterTypeEnum"
                item-text="description"
                item-value="code"
                v-model="model.quickDateFilter"
                :label="$t('UpdatedDate')"
                outlined
                clearable
                hide-details
              ></v-select>
            </v-col>
            <v-col class="pb-0 mb-0" cols="6" md="12">
              <v-switch v-model="model.filterDateByNextScheduledTransaction" hide-details>
                <template v-slot:label>
                  <small>{{$t('FilterDateByNextScheduledTransaction')}}</small>
                </template>
              </v-switch>
            </v-col>
            <date-from-to-filter class="px-3" v-model="model" :from-today="model.filterDateByNextScheduledTransaction"
              :date-from-label="model.filterDateByNextScheduledTransaction ? 'NextScheduledDateFrom' : 'CreatedFrom'"
              :date-to-label="model.filterDateByNextScheduledTransaction ? 'NextScheduledDateTo' : 'CreatedTo'"></date-from-to-filter>
          </v-row>
          <v-row>
            <v-col cols="12" md="6" class="py-0">
              <v-text-field
                v-model="model.cardOwnerNationalID"
                :label="$t('CardOwnerNationalID')"
                outlined
              ></v-text-field>
            </v-col>
            <v-col cols="12" md="6" class="py-0">
              <v-text-field
                v-model="model.consumerName"
                :label="$t('CustomerName')"
                outlined
              ></v-text-field>
            </v-col>
            <v-col cols="12" md="12" class="py-0">
              <v-text-field
                v-model="model.consumerEmail"
                :label="$t('CustomerEmail')"
                outlined
              ></v-text-field>
            </v-col>
          </v-row>
        </v-form>
      </div>
    </template>
  </ec-dialog>
</template>

<script>
import ValidationRules from "../../helpers/validation-rules";

export default {
  components: {
    EcRadioGroup: () => import("../../components/inputs/EcRadioGroup"),
    EcDialog: () => import("../../components/ec/EcDialog"),
    PaymentType: () => import("../transactions/PaymentType"),
    DateFromToFilter: () => import("../filtering/DateFromToFilter")
  },
  props: {
    show: {
      type: Boolean,
      default: false,
      required: true
    },
    filter: {
      type: Object,
      default: null,
      required: true
    }
  },
  data() {
    return {
      model: { ...this.filter },
      dictionaries: {},
      formIsValid: true,
      vr: ValidationRules
    };
  },
  async mounted() {
    this.dictionaries = await this.$api.dictionaries.getTransactionDictionaries();
  },
  computed: {
    visible: {
      get: function() {
        return this.show;
      },
      set: function(val) {
        this.$emit("update:show", val);
      }
    }
  },
  methods: {
    apply() {
      if(!this.$refs.form.validate()){
        return;
      }

      this.$emit('ok', this.model);
      this.visible = false;
    }
  },
};
</script>

<style lang="scss" scoped>
</style>