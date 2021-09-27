<template>
  <ec-dialog :dialog.sync="visible">
    <template v-slot:title>{{$t('FilterTransactions')}}</template>
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
            <v-col cols="12" md="4" class="pt-0 pb-4">
               <v-switch v-model="model.hasInvoice" hide-details>
                <template v-slot:label>
                  <small>{{$t('HasInvoice')}}</small>
                </template>
              </v-switch>
            </v-col>
            <v-col cols="12" md="4" class="pt-0 pb-4">
               <v-switch v-model="model.isPaymentRequest" hide-details>
                <template v-slot:label>
                  <small>{{$t('IsPaymentRequest')}}</small>
                </template>
              </v-switch>
            </v-col>
            <v-col cols="12" md="4" class="pt-0 pb-4">
               <v-switch v-model="model.hasMasavFile" hide-details>
                <template v-slot:label>
                  <small>{{$t('HasMasavFile')}}</small>
                </template>
              </v-switch>
            </v-col>
            <v-col cols="12" md="6" class="py-0">
              <v-text-field
                v-model="model.paymentTransactionID"
                :label="$t('PaymentTransactionIDFull')"
                :rules="[vr.primitives.guid]"
                placeholder="b51f5306-e075-4c9a-a4bf-680f91dba205"
                clearable
                outlined
              ></v-text-field>
            </v-col>
            <v-col cols="12" md="6" class="py-0">
              <v-text-field
                v-model="model.paymentTransactionIDShort"
                :label="$t('PaymentTransactionIDShort')"
                :rules="[vr.primitives.stringLength(8,8)]"
                placeholder="b51f5306"
                clearable
                outlined
              ></v-text-field>
            </v-col>
            <v-col cols="12" md="12" class="py-0">
              <terminal-select v-model="model.terminalID" :disabled="model.consumerID != null" clearable></terminal-select>
            </v-col>
            <v-col cols="12" md="6" class="py-0">
              <v-select
                :items="paymentTypesFiltered"
                item-text="description"
                item-value="code"
                v-model="model.paymentType"
                :label="$t('PaymentType')"
                outlined
                clearable
              ></v-select>
            </v-col>
            <v-col cols="12" md="6" class="py-0">
              <v-text-field
                v-model="model.shvaDealIDLastDigits"
                :label="$t('ShvaDealIdLast5Digits')"
                :rules="[vr.primitives.stringLength(5,5), vr.primitives.numeric()]"
                clearable
                outlined
              ></v-text-field>
            </v-col>
            <v-col cols="12" md="6" class="py-0">
              <v-select
                :items="dictionaries.transactionTypeEnum"
                item-text="description"
                item-value="code"
                v-model="model.transactionType"
                :label="$t('TransactionType')"
                outlined
                clearable
              ></v-select>
            </v-col>
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
            <date-from-to-filter class="px-3" v-model="model"></date-from-to-filter>
            <v-col cols="12" md="6" class="py-0">
              <v-select
                :items="dictionaries.quickStatusFilterTypeEnum"
                item-text="description"
                item-value="code"
                v-model="model.quickStatusFilter"
                :label="$t('Status')"
                :disabled="model.notTransmitted"
                outlined
                clearable
                :persistent-hint="model.notTransmitted"
                :hint="$t('CantBeUsedInNotTransmittedMode')"
              ></v-select>
            </v-col>
            <v-col cols="12" md="6" class="py-0">
              <v-select
                :items="dictionaries.cardPresenceEnum"
                item-text="description"
                item-value="code"
                v-model="model.cardPresence"
                :label="$t('CardPresence')"
                outlined
                clearable
              ></v-select>
            </v-col>
          </v-row>
          <v-row>
            <v-col cols="12" md="6" class="py-0">
              <v-text-field
                v-model="model.amountFrom"
                :label="$t('AmountFrom')"
                type="number"
                min="0"
                step="0.01"
                outlined
              ></v-text-field>
            </v-col>
            <v-col cols="12" md="6" class="py-0">
              <v-text-field
                v-model="model.amountTo"
                :label="$t('AmountTo')"
                type="number"
                min="0"
                step="0.01"
                outlined
              ></v-text-field>
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
              <v-select
                :items="dictionaries.specialTransactionTypeEnum"
                item-text="description"
                item-value="code"
                v-model="model.specialTransactionType"
                :label="$t('SpecialTransactionType')"
                outlined
                clearable
              ></v-select>
            </v-col>
            <v-col cols="12" md="6" class="py-0">
              <v-select
                :items="dictionaries.jDealTypeEnum"
                item-text="description"
                item-value="code"
                v-model="model.jDealType"
                :label="$t('JDealType')"
                outlined
                clearable
              ></v-select>
            </v-col>
            <v-col cols="12" md="6" class="py-0">
              <v-select
                :items="dictionaries.rejectionReasonEnum"
                item-text="description"
                item-value="code"
                v-model="model.rejectionReason"
                :label="$t('RejectionReason')"
                outlined
                clearable
              ></v-select>
            </v-col>
            <v-col cols="12" md="6" class="py-0">
              <v-text-field
                v-model="model.cardNumber"
                :label="$t('CardNumber')"
                outlined
              ></v-text-field>
            </v-col>
            <v-col cols="12" md="6" class="py-0">
              <v-select
                :items="dictionaries.documentOriginEnum"
                item-text="description"
                item-value="code"
                v-model="model.documentOrigin"
                :label="$t('Origin')"
                outlined
                clearable
              ></v-select>
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
    CustomerDialogInvoker: () => import("../../components/dialog-invokers/CustomerDialogInvoker"),
    DateFromToFilter: () => import("../filtering/DateFromToFilter"),
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
      model: { 
        consumerID: null,
        terminalID: null,
        ...this.filter
       },
      dictionaries: {},
      customersDialog: false,
      selectedCustomer: null,
      formIsValid: true,
      vr: ValidationRules,
      paymentTypesFiltered: null
    };
  },
  async mounted() {
    this.dictionaries = await this.$api.dictionaries.getTransactionDictionaries();
    this.paymentTypesFiltered = this.lodash.filter(this.dictionaries.paymentTypeEnum, e => e.code == "bank" || e.code == "card");
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

      this.$emit("ok", this.model);
      this.visible = false;
    },
    processCustomer(data) {
      this.$set(this.model, 'terminalID', data.terminalID);
      this.model.consumerID = data.consumerID;
    },
    resetFilter(){
      this.model = {};
      this.selectedCustomer = null;
    }
  }
};
</script>