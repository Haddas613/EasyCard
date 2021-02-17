<template>
  <v-row :align="'center'">
    <v-col cols="2" md="4" lg="4" xl="4">
      <template>
        <v-app-bar-nav-icon
          @click.stop="drawerObj = !drawerObj"
          :color="(headerStore.altDisplay) ? '' : 'primary'"
          v-if="$vuetify.breakpoint.smAndDown && !showBackBtn && !showCloseBtn"
        />
        <a role="button" v-if="showBackBtn" @click="handleNavigation('backBtn')">
          <re-icon class="primary--text">mdi-chevron-left</re-icon>
        </a>
        <a role="button" v-if="showCloseBtn" @click="handleNavigation('closeBtn')">
          <v-icon class="primary--text">mdi-close</v-icon>
        </a>
      </template>
    </v-col>
    <v-col cols="8" md="4" lg="4" xl="4" class="d-flex justify-space-around">
      <v-toolbar-title v-bind:class="{'subtitle-1': $vuetify.breakpoint.smAndDown}" v-if="!headerStore.altDisplay">{{headerText}}</v-toolbar-title>
      <v-toolbar-title v-bind:class="{'subtitle-1': $vuetify.breakpoint.smAndDown}" v-else>{{terminalName}}</v-toolbar-title>
    </v-col>
    <v-col cols="2" md="4" lg="4" xl="4" class="d-flex justify-end">
      <template v-if="tdMenuItems">
        <v-menu offset-y dark>
          <template v-slot:activator="{ on, attrs }">
            <v-icon v-bind="attrs" v-on="on">mdi-dots-vertical</v-icon>
          </template>
          <v-list class="py-0" color="grey darken-3">
            <v-list-item v-for="item in tdMenuItems" v-bind:key="item.type" @click="item.fn()">
              <v-list-item-title>{{$t(item.text)}}</v-list-item-title>
            </v-list-item>
          </v-list>
        </v-menu>
      </template>
    </v-col>
  </v-row>
</template>

<script>
import LangSwitcher from "../../components/LanguageSwitcher";
import ActionsBar from "../../components/misc/ActionsBar";
import { mapState } from "vuex";
import ReIcon from "../../components/misc/ResponsiveIcon";

export default {
  name: "EcHeaderContent",
  components: { LangSwitcher, ActionsBar, ReIcon },
  props: {
    drawer: {
      type: Boolean,
      required: true
    }
  },
  computed: {
    ...mapState({
      headerStore: state => state.ui.header,
      terminalStore: state => state.settings.terminal
    }),
    drawerObj: {
      get: function() {
        return this.drawer;
      },
      set: function(nv) {
        this.$emit("update:drawer", nv);
      }
    },
    headerText() {
      if (!this.headerStore || !this.headerStore.text) return null;

      return this.headerStore.text.translate
        ? this.$t(this.headerStore.text.value)
        : this.headerStore.text.value;
    },
    terminalName() {
      return this.terminalStore.label
        ? this.terminalStore.label
        : this.$t("TerminalNotSelected");
    },
    tdMenuItems() {
      if (
        !this.headerStore.threeDotMenu ||
        this.headerStore.threeDotMenu.length === 0
      )
        return null;

      return this.headerStore.threeDotMenu;
    },
    showBackBtn() {
      return this.$route.meta ? this.$route.meta.backBtn : false;
    },
    showCloseBtn() {
      return this.$route.meta ? this.$route.meta.closeBtn : false;
    }
  },
  methods: {
    handleNavigation(metaType) {
      let metaVal = this.$route.meta[metaType];
      if (!metaVal) {
        console.error(`Unknown meta type: ${metaType}`);
        return;
      } else if (typeof metaVal == "string") {
        this.$router.push({ name: metaVal });
      } else {
        this.$router.go(-1);
      }
    }
  }
};
</script>