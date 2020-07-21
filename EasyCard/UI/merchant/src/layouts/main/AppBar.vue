<template>
  <div>
    <v-app-bar
    :clipped-left="$vuetify.breakpoint.lgAndUp"
    :clipped-right="$vuetify.breakpoint.lgAndUp"
    app
    :color="$vuetify.breakpoint.mdAndUp ? 'primary' : 'white'"
    :dark="$vuetify.breakpoint.mdAndUp"
    :flat="true"
  >
    <v-row :align="'center'">
      <v-col cols="2" md="4" lg="4" xl="3">
        <v-app-bar-nav-icon @click.stop="drawerObj = !drawerObj" :color="$vuetify.breakpoint.mdAndUp ? '' : 'primary'"/>
      </v-col>
      <v-col cols="8" md="4" lg="4" xl="3" class="d-flex justify-space-around">
        <v-toolbar-title class="display-1 hidden-sm-and-down">easycard</v-toolbar-title>
        <v-toolbar-title class="subtitle-1 hidden-md-and-up">
          {{headerText}}
        </v-toolbar-title>
      </v-col>
      <v-col cols="2" md="4" lg="4" xl="3" class="d-flex justify-end">
        <template v-if="$vuetify.breakpoint.mdAndUp">
          <actions-bar :footer="false"></actions-bar>
        </template>
      </v-col>
    </v-row>
  </v-app-bar>
  <v-divider></v-divider>
  </div>
</template>
<script>
import LangSwitcher from "../../components/LanguageSwitcher"
import ActionsBar from "../../components/misc/ActionsBar";
import { mapState } from "vuex";

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
      headerStore: state => state.ui.headerText,
    }),
    headerText(){
      if(!this.headerStore || !this.headerStore.text)
        return null;

      return this.headerStore.translate ? this.$t(this.headerStore.text) : this.headerStore.text;
    }
  }
};
</script>