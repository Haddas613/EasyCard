<template>
  <v-container fluid>
    <v-form v-model="formIsValid" ref="form">
      <v-row>
        <merchant-terminal-filter class="pt-3" v-model="model"></merchant-terminal-filter>
        <terminal-template-filter class="pt-3" v-model="model"></terminal-template-filter>
        <v-col cols="12" md="3" sm="6">
          <v-select
            hide-details="true"
            :items="dictionaries.transactionTypeEnum"
            item-text="description"
            item-value="code"
            v-model="model.transactionType"
            :label="$t('TransactionType')"
            clearable
          ></v-select>
        </v-col>
        <v-col cols="12" md="3" sm="6">
          <v-text-field
            v-model="model.paymentTransactionID"
            :label="$t('PaymentTransactionIDFull')"
            :rules="[vr.primitives.guid]"
            placeholder="b51f5306-e075-4c9a-a4bf-680f91dba205"
            clearable
          ></v-text-field>
        </v-col>
        <v-col cols="12" md="3" sm="6">
          <v-text-field
            v-model="model.paymentTransactionIDShort"
            :label="$t('PaymentTransactionIDShort')"
            :rules="[vr.primitives.stringLength(8,8)]"
            placeholder="b51f5306"
            clearable
          ></v-text-field>
        </v-col>
        <v-col cols="12" md="3" sm="6">
          <v-select
            :items="dictionaries.quickDateFilterTypeEnum"
            item-text="description"
            item-value="code"
            v-model="model.quickDateFilter"
            :label="$t('QuickDate')"
            hide-details="true"
            clearable
          ></v-select>
        </v-col>
        <v-col cols="12" md="3">
          <v-text-field
            v-model="model.shvaDealIDLastDigits"
            :label="$t('ShvaDealIdLast5Digits')"
            :rules="[vr.primitives.stringLength(5,5), vr.primitives.numeric()]"
            clearable
          ></v-text-field>
        </v-col>
        <v-col cols="12" md="3" sm="6">
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
        <v-col cols="12" md="3" sm="6">
          <v-select
            :items="dictionaries.quickDateFilterTypeEnum"
            item-text="description"
            item-value="code"
            v-model="model.transmissionQuickDate"
            :label="$t('TransmissionDate')"
            hide-details="true"
            clearable
          ></v-select>
        </v-col>
        <date-from-to-filter class="px-3 pt-3" v-model="model"></date-from-to-filter>
        <v-col cols="12" md="3" sm="6">
          <v-select
            :items="dictionaries.quickStatusFilterTypeEnum"
            item-text="description"
            item-value="code"
            v-model="model.quickStatusFilter"
            :label="$t('Status')"
            hide-details="true"
            clearable
          ></v-select>
        </v-col>
        <v-col cols="12" md="3" sm="6">
          <v-select
            :items="dictionaries.cardPresenceEnum"
            item-text="description"
            item-value="code"
            v-model="model.cardPresence"
            :label="$t('CardPresence')"
            hide-details="true"
            clearable
          ></v-select>
        </v-col>
        <v-col cols="12" md="3" sm="6">
          <v-text-field
            v-model="model.amountFrom"
            :label="$t('AmountFrom')"
            type="number"
            min="0"
            step="0.01"
            hide-details="true"
          ></v-text-field>
        </v-col>
        <v-col cols="12" md="3" sm="6">
          <v-text-field
            v-model="model.amountTo"
            :label="$t('AmountTo')"
            type="number"
            min="0"
            step="0.01"
            hide-details="true"
          ></v-text-field>
        </v-col>
        <v-col cols="12" md="3" sm="6">
          <v-select
            :items="dictionaries.currencyEnum"
            item-text="description"
            item-value="code"
            v-model="model.currency"
            :label="$t('Currency')"
            hide-details="true"
            clearable
          ></v-select>
        </v-col>
        <v-col cols="12" md="3" sm="6">
          <v-select
            :items="dictionaries.specialTransactionTypeEnum"
            item-text="description"
            item-value="code"
            v-model="model.specialTransactionType"
            :label="$t('SpecialTransactionType')"
            hide-details="true"
            clearable
          ></v-select>
        </v-col>
        <v-col cols="12" md="3" sm="6">
          <v-select
            :items="dictionaries.jDealTypeEnum"
            item-text="description"
            item-value="code"
            v-model="model.jDealType"
            :label="$t('JDealType')"
            hide-details="true"
            clearable
          ></v-select>
        </v-col>
        <v-col cols="12" md="3" sm="6">
          <v-select
            :items="dictionaries.rejectionReasonEnum"
            item-text="description"
            item-value="code"
            v-model="model.rejectionReason"
            :label="$t('RejectionReason')"
            hide-details="true"
            clearable
          ></v-select>
        </v-col>
        <v-col cols="12" md="3" sm="6">
          <v-select
            :items="dictionaries.documentOriginEnum"
            item-text="description"
            item-value="code"
            v-model="model.documentOrigin"
            :label="$t('Origin')"
            hide-details="true"
            clearable
          ></v-select>
        </v-col>
        <v-col cols="12" md="9">
          <v-row>
            <v-col cols="12" md="4">
                <v-select
                  :items="dictionaries.propertyPresenceEnum"
                  item-text="description"
                  item-value="code"
                  v-model="model.hasInvoice"
                  :label="$t('HasInvoice')"
                  hide-details="true"
                  clearable
                ></v-select>
            </v-col>
            <v-col cols="12" md="4" class="mt-3">
                <v-switch v-model="model.hasMasavFile" hide-details>
                <template v-slot:label>
                  <small>{{$t('HasMasavFile')}}</small>
                </template>
              </v-switch>
            </v-col>
            <v-col cols="12" md="4" class="mt-3">
                <v-switch v-model="model.isPaymentRequest" hide-details>
                <template v-slot:label>
                  <small>{{$t('IsPaymentRequest')}}</small>
                </template>
              </v-switch>
            </v-col>
          </v-row>
        </v-col>
        <v-col cols="12" md="9">
          <integrations-filter v-model="model" md="4"></integrations-filter>
        </v-col>
        <v-col cols="12" md="3" sm="6">
          <v-select
            :items="dictionaries.transactionFinalizationStatusEnum"
            item-text="description"
            item-value="code"
            v-model="model.finalizationStatus"
            :label="$t('FinalizationStatus')"
            hide-details="true"
            clearable
          ></v-select>
        </v-col>
      </v-row>
      <consumer-filter v-model="model"></consumer-filter>
      <v-row>
        <v-col cols="12" class="d-flex justify-end">
          <v-btn color="success" class="mr-4" @click="apply()" :disabled="!formIsValid">{{$t('Apply')}}</v-btn>
        </v-col>
      </v-row>
    </v-form>
  </v-container>
</template>

<script>
import ValidationRules from "../../helpers/validation-rules";

export default {
  name: "TransactionsFilter",
  components: {
    MerchantTerminalFilter: () => import("../filtering/MerchantTerminalFilter"),
    TerminalTemplateFilter: () => import("../filtering/TerminalTemplateFilter"),
    ConsumerFilter: () => import("../filtering/ConsumerFilter"),
    IntegrationsFilter: () => import("../filtering/IntegrationsFilter"),
    DateFromToFilter: () => import("../filtering/DateFromToFilter"),
  },
  data() {
    return {
      model: { ...this.filterData },
      dictionaries: {},
      paymentTypesFiltered: [],
      formIsValid: true,
      vr: ValidationRules
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
      if(!this.$refs.form.validate()){
        return;
      }
      
      if(!this.model.statuses){
        this.model.statuses = null; //fix empty string
      }
      this.$emit("apply", this.model);
    }
  }
};
</script>