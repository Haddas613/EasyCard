<template>
  <v-app-bar
    :clipped-left="$vuetify.breakpoint.lgAndUp"
    :clipped-right="$vuetify.breakpoint.lgAndUp"
    app
    color="white"
    light
    :flat="true"
    class="appbar-white"
  >
    <v-row :align="'center'">
      <v-col class="d-flex justify-start px-1" cols="2" md="3" lg="4">
        <v-app-bar-nav-icon color="primary" @click.stop="drawerObj = !drawerObj" />
      </v-col>
      <v-col class="d-flex justify-space-around">
        <v-toolbar-title class="subtitle-2" @click="dialog = true">{{terminalName}}</v-toolbar-title>
      </v-col>
      <v-col cols="2" md="3" lg="4" class="d-flex justify-end">
        <v-menu offset-y>
          <template v-slot:activator="{ on, attrs }">
            <v-icon  v-bind="attrs" v-on="on">mdi-dots-vertical</v-icon>
          </template>
          <v-list>
            <v-list-item>
              <v-list-item-title>Additional Settings</v-list-item-title>
            </v-list-item>
          </v-list>
        </v-menu>
      </v-col>
    </v-row>
    <v-dialog v-model="dialog" fullscreen>
      <v-card>
        <v-card-title class="headline px-2">
          <v-icon @click="dialog = false" class="px-2">mdi-arrow-left</v-icon>
          {{$t('Terminal')}}
        </v-card-title>
        <v-divider></v-divider>
        <v-card-text class="px-0">
          <v-list class="py-2">
            <v-radio-group
              mandatory
              @change="setTerminal($event)"
              v-bind:value="terminal.terminalID"
            >
              <template v-for="(t, index) in terminals">
                <v-list-item v-bind:key="t.terminalID">
                  <v-list-item-content>{{t.label}}</v-list-item-content>
                  <v-list-item-action>
                    <v-radio :value="t.terminalID" color="black"></v-radio>
                  </v-list-item-action>
                </v-list-item>
                <v-divider v-if="index + 1 < terminals.length" :key="index"></v-divider>
              </template>
            </v-radio-group>
          </v-list>
        </v-card-text>
      </v-card>
    </v-dialog>
  </v-app-bar>
</template>
<script>
import LangSwitcher from "../../components/LanguageSwitcher";
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
  data() {
    return {
      dialog: false,
      terminals: []
    };
  },
  components: { LangSwitcher, ActionsBar },
  computed: {
    drawerObj: {
      get: function() {
        return this.drawer;
      },
      set: function(nv) {
        this.$emit("update:drawer", nv);
      }
    },
    terminal: {
      get: function() {
        return this.terminalStore;
      },
      set: function(nv) {
        this.$store.commit("settings/changeTerminal", {
          vm: this,
          newTerminal: nv
        });
      }
    },
    ...mapState({
      terminalStore: state => state.settings.terminal
    }),
    terminalName() {
      return this.terminal
        ? this.terminal.label
        : this.$t("TerminalNotSelected");
    }
  },
  async mounted() {
    let terminals = await this.$api.terminals.getTerminals();
    this.terminals = terminals ? terminals.data : [];

    //validate if stored terminal is still accessible. Clear it otherwise
    if (this.terminals.length > 0 && this.terminal) {
      let exists = this.lodash.some(
        this.terminals,
        t => t.terminalID === this.terminal.terminalID
      );
      if (!exists) this.terminal = null;
    } else {
      this.terminal = null;
    }
  },
  methods: {
    setTerminal(val) {
      if (this.terminals.length > 0) {
        let terminal = this.lodash.find(
          this.terminals,
          t => t.terminalID === val
        );
        if (terminal) this.terminal = terminal;
      }
    }
  }
};
</script>
