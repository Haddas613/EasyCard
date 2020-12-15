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
              class="px-1"
              outlined
            ></v-text-field>
          </v-col>
        </v-row>
      </v-form>
      <div class="d-flex px-2 pt-4 justify-end">
        <v-btn
          color="primary"
          class="white--text"
          :block="$vuetify.breakpoint.smAndDown"
          @click="ok()"
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
        merchantID: this.merchantId
      },
      valid: true,
      vr: ValidationRules
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
  methods: {
    async ok() {
      if (!this.valid) {
        return;
      }
      let operaionResult = await this.$api.terminals.createTerminal(this.model);
      if (operaionResult.status === "success") {
        this.visible = false;
        this.$emit("ok");
      }
    }
  }
};
</script>