<template>
  <v-overlay z-index="9" opacity="0.25" v-if="versionMismatchStore">
    <v-card class="text-center" rounded light>
      <v-card-title class="subtitle-1">{{$t("UpdateAvailable")}}</v-card-title>
      <v-card-text>{{$t("NewAppVersionAvailableClickToRefreshThePage")}}</v-card-text>
      <v-card-actions class="d-flex justify-center">
        <v-btn color="primary" icon large @click="reload()">
          <v-icon>mdi-refresh</v-icon>
        </v-btn>
      </v-card-actions>
    </v-card>
  </v-overlay>
</template>

<script>
import { mapState } from "vuex";

export default {
  computed: {
    ...mapState({
      versionMismatchStore: state => state.ui.versionMismatch
    })
  },
  methods: {
    reload() {
      location.reload(true);
    }
  }
};
</script>