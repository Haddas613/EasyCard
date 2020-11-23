<template>
  <span>
    <template v-if="currency">{{amount | currency(currency)}}</template>
    <template v-else-if="currencyStore.code">{{amount | currency(currencyStore.code)}}</template>
    <template v-else>{{amount}}</template>
  </span>
</template>

<script>
import { mapState } from "vuex";

export default {
  props: {
    amount: {
      default: 0,
      required: true
    },
    currency: {
      type: String,
      required: false
    }
  },
  computed: {
    ...mapState({
      currencyStore: state => state.settings.currency
    })
  },
};
</script>

<style lang="scss" scoped>
</style>