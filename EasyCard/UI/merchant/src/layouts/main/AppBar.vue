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
      <v-row :align="'center'">
        <v-col cols="2" md="4" lg="4" xl="4">
          <template v-if="$vuetify.breakpoint.smAndDown">
            <v-app-bar-nav-icon
              @click.stop="drawerObj = !drawerObj"
              :color="(headerStore.altDisplay) ? '' : 'primary'"
              v-if="!showBackBtn && !showCloseBtn"
            />
          <a role="button" v-if="showBackBtn" @click="handleNavigation('backBtn')">
            <re-icon class="primary--text">
              mdi-chevron-left
            </re-icon>
          </a>
          <a role="button" v-if="showCloseBtn" @click="handleNavigation('closeBtn')">
            <v-icon class="primary--text">
              mdi-close
            </v-icon>
          </a>
          </template>
          <template v-if="$vuetify.breakpoint.mdAndUp">
            <v-app-bar-nav-icon
              @click.stop="drawerObj = !drawerObj"
            />
          </template>
        </v-col>
        <v-col cols="8" md="4" lg="4" xl="4" class="d-flex justify-space-around">
          <v-toolbar-title class="display-1 hidden-sm-and-down">easycard</v-toolbar-title>
          <v-toolbar-title class="subtitle-1 hidden-md-and-up" v-if="!headerStore.altDisplay">{{headerText}}</v-toolbar-title>
        </v-col>
        <v-col cols="2" md="4" lg="4" xl="4" class="d-flex justify-end">
          <template v-if="$vuetify.breakpoint.mdAndUp">
            <actions-bar :footer="false"></actions-bar>
          </template>
          <template v-if="$vuetify.breakpoint.smAndDown && tdMenuItems">
            <v-menu offset-y dark>
              <template v-slot:activator="{ on, attrs }">
                <v-icon v-bind="attrs" v-on="on">mdi-dots-vertical</v-icon>
              </template>
              <v-list class="py-0" color="grey darken-3">
                <v-list-item
                  v-for="item in tdMenuItems"
                  v-bind:key="item.type"
                  @click="item.fn()"
                >
                  <v-list-item-title>{{$t(item.text)}}</v-list-item-title>
                </v-list-item>
              </v-list>
            </v-menu>
          </template>
        </v-col>
      </v-row>
    </v-app-bar>
    <v-divider v-if="!headerStore.altDisplay"></v-divider>
  </div>
</template>
<script>
import LangSwitcher from "../../components/LanguageSwitcher";
import ActionsBar from "../../components/misc/ActionsBar";
import { mapState } from "vuex";
import ReIcon from "../../components/misc/ResponsiveIcon";

export default {
  name: "AppBar",
  props: {
    drawer: {
      type: Boolean,
      required: true
    }
  },
  components: { LangSwitcher, ActionsBar, ReIcon },
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
    }),
    headerText() {
      if (!this.headerStore || !this.headerStore.text) return null;
      
      return this.headerStore.text.translate
        ? this.$t(this.headerStore.text.value)
        : this.headerStore.text.value;
    },
    tdMenuItems() {
      if (
        !this.headerStore.threeDotMenu ||
        this.headerStore.threeDotMenu.length === 0
      )
        return null;

      return this.headerStore.threeDotMenu;
    },
    showBackBtn(){
      return this.$route.meta ? this.$route.meta.backBtn : false;
    },
    showCloseBtn(){
      return this.$route.meta ? this.$route.meta.closeBtn : false;
    }
  },
  methods: {
    handleNavigation(metaType) {
      let metaVal = this.$route.meta[metaType];
      if(!metaVal){
        console.error(`Unknown meta type: ${metaType}`)
        return;
      }else if(typeof(metaVal) == "string"){
        this.$router.push({name: metaVal});
      }else{
        this.$router.go(-1)
      }
    }
  },
};
</script>