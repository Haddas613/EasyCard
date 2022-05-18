<template>
  <v-container fluid>
    <v-form ref="form" v-model="formIsValid">
      <v-row>
        <merchant-terminal-filter class="py-0" v-model="model"></merchant-terminal-filter>
        <!-- <v-col cols="12" md="6" class="py-0">
          <v-text-field
            v-model="model.merchantID"
            :label="$t('MerchantID')"
            :rules="[vr.primitives.guid]"
          ></v-text-field>
        </v-col>
        <v-col cols="12" md="6" class="py-0">
          <v-text-field
            v-model="model.terminalID"
            :label="$t('TerminalID')"
            :rules="[vr.primitives.guid]"
          ></v-text-field>
        </v-col> -->
        <v-col cols="12" md="4" class="py-0">
          <v-text-field
            v-model="model.invoiceID"
            :label="$t('InvoiceID')"
            :rules="[vr.primitives.guid]"
          ></v-text-field>
        </v-col>
        <v-col cols="12" md="4" class="py-0">
          <v-select
            :items="dictionaries.invoiceBillingTypeEnum"
            item-text="description"
            item-value="code"
            v-model="model.invoiceBillingType"
            :label="$t('InvoiceBillingType')"
            clearable
          ></v-select>
        </v-col>
        <v-col cols="12" md="4" class="py-0">
          <v-select
            :items="dictionaries.invoiceTypeEnum"
            item-text="description"
            item-value="code"
            v-model="model.invoiceTypeFilter"
            :label="$t('InvoiceType')"
            clearable
          ></v-select>
        </v-col>
        <v-col cols="12" md="4" class="py-0">
          <v-select
            :items="dictionaries.invoiceStatusEnum"
            item-text="description"
            item-value="code"
            v-model="model.status"
            :label="$t('Status')"
            clearable
          ></v-select>
        </v-col>
        <v-col cols="12" md="4" class="py-0">
          <v-text-field v-model="model.cardOwnerName" :label="$t('CardOwnerName')"></v-text-field>
        </v-col>
        <v-col cols="12" md="4" class="py-0">
          <v-select
            :items="dictionaries.currencyEnum"
            item-text="description"
            item-value="code"
            v-model="model.currency"
            :label="$t('Currency')"
            clearable
          ></v-select>
        </v-col>
        <v-col cols="12" md="4" class="py-0">
          <v-text-field
            v-model="model.invoiceAmount"
            :label="$t('Amount')"
            type="number"
            min="0"
            step="0.01"
            clearable
          ></v-text-field>
        </v-col>
        <v-col cols="12" md="4" sm="6" class="py-0">
          <v-text-field
            v-model="model.consumerExternalReference"
            :label="$t('CustomerExternalReference')"
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
        <date-from-to-filter class="px-3" v-model="model"></date-from-to-filter>
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
    };
  },
  async mounted() {
    this.dictionaries = await this.$api.dictionaries.getTransactionDictionaries();
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
        dateFrom: this.$formatDate(this.model.dateFrom),
        dateTo: this.$formatDate(this.model.dateTo)
      });
    }
  }
};
</script>