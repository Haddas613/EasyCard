<template>
  <v-container fluid>
    <v-row>
      <v-col cols="12" md="4">
        <v-text-field v-model="model.paymentTransactionID" :label="$t('PaymentTransactionID')"></v-text-field>
      </v-col>
      <v-col cols="12" md="4">
        <v-text-field v-model="model.terminalID" :label="$t('Terminal')"></v-text-field>
      </v-col>
      <v-col cols="12" md="4">
        <v-text-field v-model="model.merchantID" :label="$t('Merchant')"></v-text-field>
      </v-col>
    </v-row>
    <v-row>
      <v-col cols="12" md="4">
        <v-select
          :items="dictionaries.transactionTypes"
          item-text="description"
          item-value="code"
          v-model="model.transactionType"
          :label="$t('TransactionType')"
        ></v-select>
      </v-col>
      <v-col cols="12" md="4">
        <v-select
          :multiple="true"
          :items="dictionaries.transactionStatuses"
          item-text="description"
          item-value="code"
          v-model="model.statuses"
          :label="$t('Status')"
          disabled
        ></v-select>
      </v-col>
      <v-col cols="12" md="4">
        <v-select
          :items="dictionaries.cardPresences"
          item-text="description"
          item-value="code"
          v-model="model.cardPresence"
          :label="$t('CardPresence')"
        ></v-select>
      </v-col>
    </v-row>
    <v-row>
      <v-col cols="12" md="4">
        <v-text-field
          v-model="model.amountFrom"
          :label="$t('AmountFrom')"
          type="number"
          min="0"
          step="0.01"
        ></v-text-field>
      </v-col>
      <v-col cols="12" md="4">
        <v-text-field
          v-model="model.amountTo"
          :label="$t('AmountTo')"
          type="number"
          min="0"
          step="0.01"
        ></v-text-field>
      </v-col>
      <v-col cols="12" md="4">
        <v-select
          :items="dictionaries.currencies"
          item-text="description"
          item-value="code"
          v-model="model.currency"
          :label="$t('Currency')"
        ></v-select>
      </v-col>
    </v-row>
    <v-row>
      <v-col cols="12" md="4">
        <v-select
          :items="dictionaries.specialTransactionTypes"
          item-text="description"
          item-value="code"
          v-model="model.specialTransactionType"
          :label="$t('SpecialTransactionType')"
        ></v-select>
      </v-col>
      <v-col cols="12" md="4">
        <v-select
          :items="dictionaries.jDealTypes"
          item-text="description"
          item-value="code"
          v-model="model.jDealType"
          :label="$t('JDealType')"
        ></v-select>
      </v-col>
      <v-col cols="12" md="4">
        <v-select
          :items="dictionaries.rejectionReasons"
          item-text="description"
          item-value="code"
          v-model="model.rejectionReason"
          :label="$t('CardPresence')"
        ></v-select>
      </v-col>
    </v-row>
    <v-row>
      <v-col cols="12" class="d-flex justify-end">
        <v-btn color="success" class="mr-4" @click="apply()">{{$t('Apply')}}</v-btn>
      </v-col>
    </v-row>
  </v-container>
</template>

<script>
export default {
  name: "TransactionsFilter",
  data() {
    return {
      model: { ...this.filterData },
      dictionaries: {}
    };
  },
  async mounted() {
    this.dictionaries = await this.$api.dictionaries.getTransactionDictionaries();
    const all = { code: null, description: this.$t("All") };
    this.dictionaries.transactionTypes = [all, ...this.dictionaries.transactionTypes];
    this.dictionaries.currencies = [all, ...this.dictionaries.currencies];
    this.dictionaries.cardPresences = [all, ...this.dictionaries.cardPresences];
    this.dictionaries.specialTransactionTypes = [all, ...this.dictionaries.specialTransactionTypes];
    this.dictionaries.jDealTypes = [all, ...this.dictionaries.jDealTypes];
    this.dictionaries.rejectionReasons = [all, ...this.dictionaries.rejectionReasons];
  },
  props: {
    filterData: {
      type: Object
    }
  },
  methods: {
    apply() {
      this.$emit("apply", this.model);
    }
  }
};
</script>