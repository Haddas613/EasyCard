<template>
  <v-app-bar
    :clipped-left="$vuetify.breakpoint.lgAndUp"
    :clipped-right="$vuetify.breakpoint.lgAndUp"
    app
    color="primary"
    dark
    :flat="true"
  >
    <v-row :align="'center'">
      <v-col cols="4" md="4" lg="4" xl="3">
        <v-app-bar-nav-icon @click.stop="drawerObj = !drawerObj" />
      </v-col>
      <v-col class="d-flex justify-space-around">
        <v-toolbar-title class="display-1 hidden-xs-only">easycard</v-toolbar-title>
      </v-col>
      <v-col cols="5" md="4" lg="4" xl="3" class="d-flex justify-end">
        <template v-if="$vuetify.breakpoint.smAndUp">
          <actions-bar :footer="false"></actions-bar>
        </template>
      </v-col>
    </v-row>
  </v-app-bar>
</template>
<script>
import LangSwitcher from "../../components/LanguageSwitcher"
import ActionsBar from "../../components/misc/ActionsBar";

export default {
  name: "MainAppBar",
  props: ["drawer"],
  components: {LangSwitcher, ActionsBar},
  computed: {
    drawerObj: {
      get: function() {
        return this.drawer;
      },
      set: function(nv) {
        this.$emit("update:drawer", nv);
      }
    }
  }
};
</script>