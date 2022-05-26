<template>
  <ec-dialog :dialog.sync="visible">
    <template v-slot:title>{{$t('FilterInvoices')}}</template>
    <template v-slot:right>
      <v-btn color="primary" @click="apply()" :disabled="!formIsValid">{{$t('Apply')}}</v-btn>
    </template>
    <template>
      <div class="d-flex px-4 py-2 justify-end">
        <v-btn
          color="primary"
          :block="$vuetify.breakpoint.smAndDown"
          outlined
          @click="resetFilter()"
        >{{$t("Reset")}}</v-btn>
      </div>
      <div class="px-4 py-2">
        <v-form ref="form" v-model="formIsValid">
          <v-row>
            <v-col cols="12" md="12" class="pb-2 pt-0">
              <customer-dialog-invoker 
              :key="model.consumerID" 
              :terminal="model.terminalID" 
              :customer-id="model.consumerID"
              @update="processCustomer($event)"></customer-dialog-invoker>
            </v-col>
            <v-col cols="12" md="6" class="py-0">
              <v-text-field v-model="model.invoiceID" :label="$t('InvoiceID')" :rules="[vr.primitives.guid]" outlined></v-text-field>
            </v-col>
            <v-col cols="12" md="6" class="py-0">
              <terminal-select v-model="model.terminalID" clearable></terminal-select>
            </v-col>
            <v-col cols="12" md="6" class="py-0">
              <v-select
                :items="dictionaries.invoiceTypeEnum"
                item-text="description"
                item-value="code"
                v-model="model.invoiceType"
                :label="$t('InvoiceType')"
                outlined
                clearable
              ></v-select>
            </v-col>
            <v-col cols="12" md="6" class="py-0">
              <v-select
                :items="dictionaries.invoiceBillingTypeEnum"
                item-text="description"
                item-value="code"
                v-model="model.invoiceBillingType"
                :label="$t('InvoiceOrigin')"
                outlined
                clearable
              ></v-select>
            </v-col>
          </v-row>
          <v-row>
            <v-col cols="12" md="6" class="py-0">
              <v-text-field v-model="model.consumerEmail" :label="$t('CustomerEmail')" outlined></v-text-field>
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
              ></v-select>
            </v-col>
            <v-col cols="12" md="6" class="py-0">
              <v-text-field
                v-model="model.invoiceAmount"
                :label="$t('Amount')"
                type="number"
                min="0"
                step="0.01"
                outlined
                clearable
              ></v-text-field>
            </v-col>
          </v-row>
          <v-row>
            <v-col cols="12" md="6" class="py-0">
              <v-select
                :items="dictionaries.quickDateFilterTypeEnum"
                item-text="description"
                item-value="code"
                v-model="model.quickDateFilter"
                :label="$t('QuickDate')"
                outlined
                clearable
              ></v-select>
            </v-col>
            <v-col cols="12" md="6" class="py-0">
              <v-text-field
                v-model="model.consumerExternalReference"
                :label="$t('CustomerExternalReference')"
                outlined
              ></v-text-field>
            </v-col>
           <date-from-to-filter class="px-3" v-model="model"></date-from-to-filter>
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
    EcDialog: () => import("../ec/EcDialog"),
    DateFromToFilter: () => import("../filtering/DateFromToFilter"),
    CustomerDialogInvoker: () => import("../../components/dialog-invokers/CustomerDialogInvoker"),
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
      vr: ValidationRules,
      selectedCustomer: null,
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

      this.$emit("ok", {
        ...this.model,
        dateFrom: this.$formatDate(this.model.dateFrom),
        dateTo: this.$formatDate(this.model.dateTo),
      });
      this.visible = false;
    },
    processCustomer(data) {
      this.$set(this.model, 'terminalID', data.terminalID);
      this.model.consumerID = data.consumerID;
    },
    resetFilter(){
      this.model = {};
      this.selectedCustomer = null;
    },
  }
};
</script>