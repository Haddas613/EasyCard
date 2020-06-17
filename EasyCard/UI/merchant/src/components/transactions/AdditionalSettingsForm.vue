<template>
  <v-card class="ec-card d-flex flex-column">
    <v-card-text class="py-2">
      <v-form class="ec-form" ref="form">
        <v-select
          :items="dictionaries.jDealTypeEnum"
          item-text="description"
          item-value="code"
          v-model="model.jDealType"
          :label="$t('JDealType')"
        ></v-select>
        <v-select
          :items="dictionaries.transactionTypeEnum"
          item-text="description"
          item-value="code"
          v-model="model.transactionType"
          :label="$t('TransactionType')"
        ></v-select>
        <installment-details
          ref="instDetails"
          :data="model.installmentDetails"
          v-if="isInstallmentTransaction"
        ></installment-details>
      </v-form>
    </v-card-text>
    <v-btn
      color="primary"
      class="px-2"
      bottom
      :x-large="true"
      fixed
      block
      @click="ok()"
    >{{$t('Confirm')}}</v-btn>
  </v-card>
</template>

<script>
import InstallmentDetails from "./InstallmentDetailsForm";

export default {
  components: {
    InstallmentDetails
  },
  props: {
    data: {
      type: Object,
      default: null,
      required: true
    }
  },
  data() {
    return {
      dictionaries: {},
      model: { ...this.data }
    };
  },
  computed: {
    isInstallmentTransaction() {
      return (
        this.model.transactionType === "installments" ||
        this.model.transactionType === "credit"
      );
    }
  },
  async mounted() {
    this.dictionaries = await this.$api.dictionaries.getTransactionDictionaries();
    this.model.transactionType = this.dictionaries.transactionTypeEnum[0].code;
    this.model.jDealType = this.dictionaries.jDealTypeEnum[0].code;
  }
};
</script>

<style lang="scss" scoped>
</style>