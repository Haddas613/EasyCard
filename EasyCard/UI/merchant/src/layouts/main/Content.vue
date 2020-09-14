<template>
  <v-main class="ecbg">
    <v-container fluid ma-0 pa-0 center>
      <v-row no-gutters>
        <v-col cols="2" class="hidden-sm-and-down" v-bind:class="{'d-none' : drawer}">
          <v-spacer></v-spacer>
        </v-col>
        <v-col class="d-flex justify-space-around">
          <v-flex class="flex-column">
            <v-app-bar flat color="white" v-if="$vuetify.breakpoint.mdAndUp && !headerStore.altDisplay">
              <ec-header-content :drawer.sync="drawerObj"></ec-header-content>
            </v-app-bar>
            <router-view />
          </v-flex>
        </v-col>
        <v-col cols="2" class="hidden-sm-and-down" v-bind:class="{'d-none' : drawer}">
          <v-spacer></v-spacer>
        </v-col>
      </v-row>
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