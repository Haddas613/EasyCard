<template>
  <div class="scroll-contain">
    <v-overlay :value="requestsCountStore > 0" z-index="10">
      <v-progress-circular indeterminate size="64"></v-progress-circular>
    </v-overlay>
    <template v-if="renderReady">
      <notifications-hub />
      <router-view />
    </template>
  </div>
</template>

<script>

import { mapState } from "vuex";
import i18n from "./i18n";

export default {
  components: {
    NotificationsHub: () => import("./components/misc/NotificationsHub"),
  },
  data() {
    return {
      requestsCount: 0,
      renderReady: false
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
      this.renderReady = true;
    }
  },
};
</script>