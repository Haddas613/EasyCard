<template>
  <span dir="ltr">{{amount | currency(currencyCode)}}</span>
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
  data() {
      return {
          currencyCode: null
      }
  },
  computed: {
    ...mapState({
      currencyStore: state => state.settings.currency
    })
  },
  beforeMount() {
    this.currencyCode = this.currency || this.currencyStore.code;
  }
};
</script>

<style lang="scss" scoped>
</style>