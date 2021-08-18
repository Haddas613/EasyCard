<template>
  <v-app id="inspire" v-bind:dir="$vuetify.rtl ? 'rtl' : 'ltr'" class="ecbg">
    <app-version-overlay></app-version-overlay>
    <idle-confirmation-overlay></idle-confirmation-overlay>
    <v-overlay :value="requestsCountStore > 0" z-index="10">
      <v-progress-circular indeterminate size="64"></v-progress-circular>
    </v-overlay>
    <v-overlay z-index="10" v-if="!available">
      <v-card class="text-center" rounded light>
        <v-card-title class="subtitle-1">
          {{$t("SystemIsNotReadyToUse")}}
          <v-icon
            class="pb-1"
            v-bind:class="{'pr-2': $vuetify.rtl, 'pl-2': !$vuetify.rtl}"
            small
            color="error darken-2"
          >mdi-cancel</v-icon>
        </v-card-title>
        <v-card-text>{{$t("NoAvailableTerminals")}}</v-card-text>
      </v-card>
    </v-overlay>
    <div class="scroll-contain">
      <router-view v-if="renderReady" />
    </div>
  </v-app>
</template>

<script>
import { mapState } from "vuex";
import i18n from "./i18n";

export default {
  components: {
    NotificationsHub: () => import("./components/misc/NotificationsHub"),
    AppVersionOverlay: () => import("./components/misc/AppVersionOverlay"),
    IdleConfirmationOverlay: () =>
      import("./components/misc/IdleConfirmationOverlay")
  },
  data() {
    return {
      requestsCount: 0,
      renderReady: false,
      available: true
    };
  },
  computed: {
    ...mapState({
      requestsCountStore: state => state.ui.requestsCount,
      idlingStore: state => state.idling
    })
  },
  async beforeMount() {
    if (!!this.$oidc && (await this.$oidc.isAuthenticated())) {
      this.available = await this.$store.dispatch(
        "settings/getDefaultSettings",
        { api: this.$api, lodash: this.lodash }
      );
      this.renderReady = this.available;
    }
    if (this.renderReady) {
      await this.$store.dispatch("idling/start");
    }
  }
};
</script>
<style lang="scss" scoped>
.ecbg{
  background-color: var(--v-ecbg-base) !important;
}
</style>