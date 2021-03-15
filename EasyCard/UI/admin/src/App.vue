<template>
  <div class="scroll-contain">
    <v-overlay :value="requestsCountStore > 0" z-index="10">
      <v-progress-circular indeterminate size="64"></v-progress-circular>
    </v-overlay>
    <router-view />
  </div>
</template>

<script>
import { mapState } from "vuex";
import i18n from "./i18n";

export default {
  data() {
    return {
      requestsCount: 0,
    };
  },
  computed: {
    ...mapState({
      requestsCountStore: state => state.ui.requestsCount
    })
  },
  async beforeMount () {
    if(!!this.$oidc && await this.$oidc.isAuthenticated()) {
      await this.$store.dispatch('settings/getDefaultSettings', { api: this.$api, lodash: this.lodash });
    }
  },
};
</script>