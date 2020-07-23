<template>
  <div>
    <ec-dialog :dialog.sync="terminalDialog">
      <template v-slot:title>{{$t('Terminal')}}</template>
      <ec-radio-group :data="terminals" valuekey="terminalID" return-object :model.sync="terminal"></ec-radio-group>
    </ec-dialog>
    <v-app-bar app fixed flat color="white">
      <v-row :align="'center'" class="top-area">
        <v-col class="d-flex justify-start px-1" cols="2" md="3" lg="4">
          <v-spacer></v-spacer>
        </v-col>
        <v-col class="d-flex justify-space-around">
          <v-toolbar-title
            v-if="canchangeterminal"
            class="subtitle-1" v-bind:class="{'primary--text': terminal, 'error--text': !terminal}"
            @click="terminalDialog = true"
          >{{terminalName}}</v-toolbar-title>
          <v-toolbar-title
            v-if="!canchangeterminal"
            class="subtitle-1 ecgray--text"
          >{{terminalName}}</v-toolbar-title>
        </v-col>
        <v-col cols="2" md="3" lg="4" class="d-flex justify-end">
          <v-menu offset-y dark v-if="tdmenuitems && tdmenuitems.length > 0">
            <template v-slot:activator="{ on, attrs }">
              <v-icon v-bind="attrs" v-on="on">mdi-dots-vertical</v-icon>
            </template>
            <v-list class="py-0">
              <v-list-item v-for="item in tdmenuitems" v-bind:key="item.type" @click="$emit('td-menu-clicked', item.type)">
                <v-list-item-title>{{$t(item.text)}}</v-list-item-title>
              </v-list-item>
            </v-list>
          </v-menu>
          <v-spacer v-if="!tdmenuitems || tdmenuitems.length == 0"></v-spacer>
        </v-col>
      </v-row>
      <template v-slot:extension>
        <v-row :align="'center'">
          <v-col cols="2" class="d-flex px-1 justify-start" v-if="completed">
            <v-spacer></v-spacer>
          </v-col>
          <v-col cols="2" class="d-flex justify-start" v-if="!completed">
            <v-btn icon color="primary" @click="onClickBack()">
              <v-icon size="2rem" left>{{$vuetify.rtl ? 'mdi-chevron-right' : 'mdi-chevron-left'}}</v-icon>
            </v-btn>
          </v-col>
          <v-col class="d-flex justify-space-around">
            <v-toolbar-title class="subtitle-2 font-weight-bold">{{title}}</v-toolbar-title>
          </v-col>
          <v-col
            cols="2"
            class="d-flex justify-end"
            v-bind:class="{'px-2': $vuetify.rtl, 'px-1': !$vuetify.rtl}"
            v-if="skippable && !completed"
            @click="onClickSkip()"
          >
            <v-btn color="primary" class="text-none">{{$t('Skip')}}</v-btn>
          </v-col>
          <v-col
            cols="2"
            class="d-flex px-1 justify-end"
            v-if="closeable || completed"
            @click="onClickClose()"
          >
            <v-btn color="primary" icon>
              <v-icon icon size="2rem" color="primary">mdi-close</v-icon>
            </v-btn>
          </v-col>
          <v-col
            cols="2"
            class="d-flex px-1 justify-end"
            v-if="!skippable && (!closeable && !completed)"
          >
            <v-spacer></v-spacer>
          </v-col>
        </v-row>
      </template>
    </v-app-bar>
  </div>
</template>

<script>
import { mapState } from "vuex";

export default {
  components: {
    EcDialog: () => import("../../components/ec/EcDialog"),
    EcRadioGroup: () => import("../../components/inputs/EcRadioGroup")
  },
  props: {
    title: {
      type: String,
      default: null
    },
    skippable: {
      type: Boolean,
      default: false
    },
    closeable: {
      type: Boolean,
      default: false
    },
    completed: {
      type: Boolean,
      default: false
    },
    tdmenuitems: {
      type: Array,
      default: null,
      required: false
    },
    canchangeterminal: {
      type: Boolean,
      default: false
    }
  },
  data() {
    return {
      terminalDialog: false,
      terminals: []
    };
  },
  computed: {
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
    }
    else if(this.terminals.length > 0 && !this.terminal){
      this.terminal = this.terminals[0];
    } else {
      this.terminal = null;
    }
  },
  methods: {
    onClickBack() {
      this.$emit("back");
    },
    onClickSkip() {
      this.$emit("skip");
    },
    onClickClose() {
      this.$emit("close");
    },
  }
};
</script>

<style lang="scss" scoped>
.v-toolbar__content {
  padding-left: 0 !important;
  border-bottom: 1px solid var(--v-ecbg-darken1);
}
.top-area {
  border-bottom: 1px solid var(--v-ecbg-base);
}
.bottom-area {
  border-bottom: 4px solid var(--v-ecbg-base);
}
</style>