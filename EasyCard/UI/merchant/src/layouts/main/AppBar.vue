<template>
  <div>
    <v-app-bar
      :clipped-left="$vuetify.breakpoint.lgAndUp"
      :clipped-right="$vuetify.breakpoint.lgAndUp"
      app
      :color="($vuetify.breakpoint.mdAndUp || headerStore.altDisplay) ? 'primary' : 'white'"
      :dark="($vuetify.breakpoint.mdAndUp || headerStore.altDisplay)"
      :flat="true"
    >
    <template v-if="$vuetify.breakpoint.mdAndUp">
       <v-row :align="'center'">
        <v-col cols="2" md="4" lg="4" xl="4">
          <v-app-bar-nav-icon @click.stop="drawerObj = !drawerObj" />
        </v-col>
        <v-col cols="8" md="4" lg="4" xl="4" class="d-flex justify-space-around">
          <v-toolbar-title class="display-1 hidden-sm-and-down">easycard</v-toolbar-title>
        </v-col>
        <v-col cols="2" md="4" lg="4" xl="4" class="d-flex justify-end">
          <actions-bar :footer="false"></actions-bar>
        </v-col>
      </v-row>
    </template>
    <template v-if="$vuetify.breakpoint.smAndDown">
      <ec-header-content :drawer.sync="drawerObj"></ec-header-content>
    </template>
    </v-app-bar>
    <v-divider v-if="!headerStore.altDisplay"></v-divider>
  </div>
</template>
<script>
import ActionsBar from "../../components/misc/ActionsBar";
import { mapState } from "vuex";
import EcHeaderContent from "../../layouts/shared/EcHeaderContent";

export default {
  name: "AppBar",
  props: {
    drawer: {
      type: Boolean,
      required: true
    }
  },
  components: { ActionsBar, EcHeaderContent },
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