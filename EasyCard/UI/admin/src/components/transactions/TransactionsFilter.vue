<template>
  <v-container fluid>
    <v-row>
      <v-col cols="12" md="3" sm="6">
        <v-text-field outlined v-model="model.terminalID" :label="$t('Terminal')"></v-text-field>
      </v-col>
      <v-col cols="12" md="3" sm="6">
        <v-text-field outlined v-model="model.merchantID" :label="$t('Merchant')"></v-text-field>
      </v-col>
       <v-col cols="12" md="3" sm="6">
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
      <v-col cols="12" md="3" sm="6">
        <v-select
          outlined
          :items="dictionaries.transactionTypeEnum"
          item-text="description"
          item-value="code"
          v-model="model.transactionType"
          :label="$t('TransactionType')"
          clearable
        ></v-select>
      </v-col>
      <v-col cols="12" md="3" sm="6">
        <v-select
          :items="dictionaries.quickStatusFilterTypeEnum"
          item-text="description"
          item-value="code"
          v-model="model.quickStatusFilter"
          :label="$t('Status')"
          outlined
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
          outlined
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
          outlined
        ></v-text-field>
      </v-col>
      <v-col cols="12" md="3" sm="6">
        <v-text-field
          v-model="model.amountTo"
          :label="$t('AmountTo')"
          type="number"
          min="0"
          step="0.01"
          outlined
        ></v-text-field>
      </v-col>
      <v-col cols="12" md="3" sm="6">
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
      <v-col cols="12" md="3" sm="6">
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
      <v-col cols="12" md="3" sm="6">
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
      <v-col cols="12" md="3" sm="6">
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
  },
  props: {
    filterData: {
      type: Object
    }
  },
  methods: {
    apply() {
      if(!this.model.statuses){
        this.model.statuses = null; //fix empty string
      }
      this.$emit("apply", this.model);
    }
  }
};
</script>