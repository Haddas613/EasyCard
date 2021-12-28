<template>
  <ec-dialog :dialog.sync="visible">
    <template v-slot:title>{{$t('TransmitTransactionsByTerminal')}}</template>
    <template>
      <v-form class="mt-4" ref="form" lazy-validation>
        <v-select
          :items="terminals"
          item-text="label"
          item-value="terminalID"
          v-model="selectedTerminal"
          :label="$t('Terminal')"
          outlined
          :rules="[vr.primitives.required]"
        ></v-select>
      </v-form>
      <div class="d-flex justify-end">
        <v-btn color="success" @click="transmitTransactionsByTerminal()" :loading="loading">{{$t("OK")}}</v-btn>
      </div>
    </template>
  </ec-dialog>
</template>

<script>
import ValidationRules from "../../helpers/validation-rules";
import { mapState } from "vuex";

export default {
  props: {
    show: {
      type: Boolean,
      default: false,
      required: true
    }
  },
  components: {
    EcDialog: () => import("../../components/ec/EcDialog")
  },
  data() {
    return {
      terminals: [],
      selectedTerminal: null,
      vr: ValidationRules,
      loading: false
    };
  },
  async mounted() {
    let terminals = await this.$api.terminals.getTerminals();
    this.terminals = terminals ? terminals.data : [];
    this.selectedTerminal = this.terminalStore.terminalID;
  },
  computed: {
    ...mapState({
      terminalStore: state => state.settings.terminal,
    }),
    visible: {
      get: function() {
        return this.show;
      },
      set: function(val) {
        this.$emit("update:show", val);
      }
    }
  },
  methods: {
    async transmitTransactionsByTerminal() {
      if(!this.$refs.form.validate()){
        return;
      }

      let opResult = await this.$api.transmissions.transmitByTerminal(this.selectedTerminal);

    //   if(opResult.count > 0){
    //     this.$toasted.show(this.$t("@TransactionsQueuedSuccess").replace("@count", opResult.count), {
    //       type: "success"
    //     });
    //   }
    
      this.visible = false;
      this.$emit("ok");
    }
  }
};
</script>