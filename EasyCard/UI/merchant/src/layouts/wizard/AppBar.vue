<template>
  <v-app-bar
    :clipped-left="$vuetify.breakpoint.lgAndUp"
    :clipped-right="$vuetify.breakpoint.lgAndUp"
    app
    color="white"
    light
    :flat="true"
  >
    <v-row :align="'center'">
      <v-col class="d-flex justify-start px-1" cols="2">
        <v-app-bar-nav-icon color="primary" @click.stop="drawerObj = !drawerObj" />
      </v-col>
      <v-col class="d-flex justify-space-around">
        <v-toolbar-title class="subtitle-2">{{terminalName}}</v-toolbar-title>
      </v-col>
      <v-col cols="2" class="d-flex justify-end">
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
import { mapState } from 'vuex'

export default {
  name: "AppBar",
  props: {
    drawer: {
      type: Boolean,
      required: true
    }
  },
  components: {LangSwitcher, ActionsBar},
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
      terminal: state => state.settings.terminal
    }),
    terminalName(){
      return this.terminal ? this.terminal.name : this.$t('TerminalNotSelected')
    }
  }
};
</script>