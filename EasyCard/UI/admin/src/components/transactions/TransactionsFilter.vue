<template>
  <v-container fluid>
    <v-form v-model="formIsValid" ref="form">
      <v-row>
        <merchant-terminal-filter class="pt-3" v-model="model"></merchant-terminal-filter>
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
      </v-row>
      <v-row>
        <v-col cols="12" class="d-flex justify-end">
          <v-btn color="success" class="mr-4" @click="apply()" :disabled="!formIsValid">{{$t('Apply')}}</v-btn>
        </v-col>
      </v-row>
    </v-form>
  </v-container>
</template>

<script>
export default {
  name: "TransactionsFilter",
  components: {
    MerchantTerminalFilter: () => import("../filtering/MerchantTerminalFilter"),
  },
  data() {
    return {
      model: { ...this.filterData },
      dictionaries: {},
      formIsValid: true
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