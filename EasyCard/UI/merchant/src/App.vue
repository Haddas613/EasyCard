<template>
  <div>
    <v-overlay :value="requestsCountStore > 0" z-index="10">
      <v-progress-circular indeterminate size="64"></v-progress-circular>
    </v-overlay>
    <router-view />
  </div>
</template>

<script>
import { mapState } from "vuex";
import auth from "./auth";

export default {
  data() {
    return {
      requestsCount: 0,
      auth: auth
    };
  },
  computed: {
    ...mapState({
      requestsCountStore: state => state.ui.requestsCount
    })
  },
  async beforeMount () {
    if(!auth.isAuthenticated) return;
    await this.$store.dispatch('settings/getDefaultSettings',{ api: this.$api });
  },
  watch: {
    /**requests are watched with delay so overlay is not shown immediately
     * but after a 1s delay
     */
    // requestsCountStore(newValue, oldValue) {
    //   //decrement
    //   // console.log(newValue, oldValue)
    //   if (newValue < oldValue) {
    //     this.requestsCount--;
    //     console.log(newValue, oldValue, this.requestsCount)
    //   }
    //   //increment
    //   else{
    //     setTimeout(
    //       (() => {
    //         this.requestsCount++;
    //         console.log(newValue, oldValue, this.requestsCount)
    //       }).bind(this),
    //       1000
    //     );
    //   }
    // }
  }
};
</script>

<style lang="scss" scoped>
</style>