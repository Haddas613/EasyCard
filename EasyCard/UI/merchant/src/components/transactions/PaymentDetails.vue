<template>
  <v-card class="ec-card d-flex flex-column" fill-height>
    <v-card-text class="py-2">
      <v-form class="ec-form" ref="form" lazy-validation>
        Work in progress
      </v-form>
    </v-card-text>
    <v-card-actions class="px-4">
      <v-btn color="primary" bottom :x-large="true" block @click="ok()">{{$t(btnText)}}</v-btn>
    </v-card-actions>
  </v-card>
</template>

<script>
import ValidationRules from "../../helpers/validation-rules";
import { mapState } from "vuex";
import appConstants from "../../helpers/app-constants";

export default {
  components: {
    EcDialog: () => import("../../components/ec/EcDialog"),
    EcDialogInvoker: () => import("../../components/ec/EcDialogInvoker"),
    EcRadioGroup: () => import("../../components/inputs/EcRadioGroup"),
    ReIcon: () => import("../../components/misc/ResponsiveIcon"),
  },
  props: {
    data: {
      type: Object,
      default: null,
      required: false
    },
    btnText: {
      type: String,
      default: 'OK',
      required: false
    }
  },
  data() {
    return {
      model: { ...this.data },
      appConstants: appConstants,
      vr: ValidationRules
    };
  },
  async mounted() {
    
  },
  computed: {
    ...mapState({
      terminalStore: state => state.settings.terminal
    }),
  },
  methods: {
    ok() {
      this.$emit("ok", null);
    }
  }
};
</script>