<template>
  <v-main class="ecbg">
    <v-container fluid ma-0 pa-0 center>
      <v-row no-gutters>
        <v-col cols="2" class="hidden-sm-and-down" v-bind:class="{'d-none' : drawer}">
          <v-spacer></v-spacer>
        </v-col>
        <v-col class="d-flex justify-space-around">
          <v-flex class="flex-column" :key="keepAliveStateKey">
            <v-app-bar flat color="white" v-if="$vuetify.breakpoint.mdAndUp && !headerStore.altDisplay">
              <ec-header-content :drawer.sync="drawerObj"></ec-header-content>
            </v-app-bar>
            <keep-alive max="1" :include="keepAliveComponentsList">
              <router-view :key="$route.fullPath" />
            </keep-alive>
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
  data() {
    return {
      keepAliveComponentsList: [],
    }
  },
  props: ["drawer"],
  mounted () {
    this.keepAliveComponentsList = this.lodash.filter(this.$router.options.routes[1].children, r => (r.meta && r.meta.keepAlive)).map(r => r.meta.keepAlive);
  },
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
      headerStore: state => state.ui.header,
      keepAliveStateKey: state => state.ui.keepAliveRenderState,
    }),
  }
};
</script>