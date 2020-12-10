<template>
  <v-main class="ecbg">
    <v-container fluid center>
      <v-flex class="flex-column">
        <v-app-bar flat color="white" v-if="$vuetify.breakpoint.mdAndUp && !headerStore.altDisplay">
          <ec-header-content :drawer.sync="drawerObj"></ec-header-content>
        </v-app-bar>
        <router-view />
      </v-flex>
    </v-container>
  </v-main>
</template>
<script>
import EcHeaderContent from "../../layouts/shared/EcHeaderContent";
import { mapState } from "vuex";

export default {
  name: "MainContent",
  components: {
    EcHeaderContent
  },
  props: ["drawer"],
  computed: {
    drawerObj: {
      get: function() {
        return this.drawer;
      },
      set: function(nv) {
        this.$emit("update:drawer", nv);
      }
    },
    ...mapState({
      headerStore: state => state.ui.header
    }),
  }
};
</script>