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
      <v-col cols="2">
        <v-app-bar-nav-icon @click.stop="drawerObj = !drawerObj" />
      </v-col>
      <v-col class="d-flex justify-space-around">
        <v-toolbar-title class="display-1 hidden-sm-and-down">easycard</v-toolbar-title>
      </v-col>
      <v-col cols="1">
        <lang-switcher class="pt-5 hidden-sm-and-down"></lang-switcher>
      </v-col>
      <v-col cols="1" class="d-flex justify-end">
        <v-switch class="pt-5 hidden-sm-and-down" :flat="true" :color="'accent'" label="RTL" v-model="$vuetify.rtl"></v-switch>
      </v-col>
    </v-row>
  </v-app-bar>
</template>
<script>
import LangSwitcher from "../../components/LanguageSwitcher"

export default {
  name: "MainAppBar",
  props: ["drawer"],
  components: {LangSwitcher},
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