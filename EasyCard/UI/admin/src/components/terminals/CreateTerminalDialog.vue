<template>
  <ec-dialog :dialog.sync="visible">
    <template v-slot:title>{{$t('CreateTerminal')}}</template>
    <template>
      <v-form class="pt-2" ref="form" v-model="valid" lazy-validation>
        <v-row>
          <v-col cols="12" class="py-0">
            <v-text-field
              v-model="model.label"
              :counter="50"
              :rules="[vr.primitives.required, vr.primitives.stringLength(6, 50)]"
              :label="$t('BusinessName')"
              outlined
            ></v-text-field>
          </v-col>
          <v-col cols="12" class="py-0">
            <v-select
              :items="terminalTemplates"
              item-text="label"
              item-value="terminalTemplateID"
              v-model="model.terminalTemplateID"
              :label="$t('TerminalTemplate')"
              outlined
            ></v-select>
          </v-col>
        </v-row>
      </v-form>
      <div class="d-flex px-2 pt-4 justify-end">
        <v-btn
          color="primary"
          class="white--text"
          :block="$vuetify.breakpoint.smAndDown"
          @click.once="ok()"
        >{{$t("OK")}}</v-btn>
      </div>
    </template>
  </ec-dialog>
</template>

<script>
import ValidationRules from "../../helpers/validation-rules";
export default {
  props: {
    merchantId: {
      type: String,
      required: true
    },
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
      model: {
        label: null,
        merchantID: this.merchantId,
        terminalTemplateID: null
      },
      valid: true,
      vr: ValidationRules,
      terminalTemplates: [],
      terminalTemplatesFilter:{
        active: true
      }
    };
  },
  computed: {
    visible: {
      get: function() {
        return this.show;
      },
      set: function(val) {
        this.$emit("update:show", val);
      }
    }
  },
  async mounted () {
    let templates = (await this.$api.terminalTemplates.get(this.terminalTemplatesFilter)).data || [];

    if(templates && templates.length > 0){
      this.terminalTemplates = templates;
      this.model.terminalTemplateID = templates[0].terminalTemplateID;
    }
  },
  methods: {
    async ok() {
      if (!this.$refs.form.validate()) {
        return;
      }
      let operationResult = await this.$api.terminals.createTerminal(this.model);
      if (operationResult.status === "success") {
        this.visible = false;
        this.model.label = null;
        this.$emit("ok", operationResult.entityReference);
      }
    }
  }
};
</script>