<template>
  <ec-dialog :dialog.sync="visible">
    <template v-slot:title>{{$t('ResendInvoice')}}</template>
    <template>
      <div class="px-4 py-2">
        <v-form ref="form">
          <v-row>
            <v-col cols="12">
              <v-text-field
                v-model="email"
                :append-outer-icon="loading ? null : 'mdi-send'"
                @click:append-outer="resendInvoice"
                :label="$t('Email')"
                :rules="[vr.primitives.required, vr.primitives.email]"
                :loading="loading"
              ></v-text-field>
            </v-col>
          </v-row>
        </v-form>
      </div>
    </template>
  </ec-dialog>
</template>


<script>
import ValidationRules from "../../helpers/validation-rules";
export default {
  props: {
    show: {
      type: Boolean,
      default: false,
      required: true
    },
    invoice: {
      type: Object,
      required: true
    }
  },
  components: {
    EcDialog: () => import("../ec/EcDialog"),
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
  data() {
    return {
      email: null,
      vr: ValidationRules,
      loading: false
    };
  },
  mounted () {
      this.email = this.invoice.dealDetails.consumerEmail;
  },
  methods: {
    async resendInvoice() {
      if (!this.$refs.form.validate()) {
        return;
      }
      this.loading = true;
      let operationResult = await this.$api.invoicing.resendSingle({
          invoiceID: this.invoice.$invoiceID || this.invoice.invoiceID,
          email: this.email
      });

      if (!this.$apiSuccess(operationResult)) {
        this.$toasted.show(operationResult.message || this.$t('Error'), { type: 'error' });
      }
      this.$emit('ok');
      this.loading = false;
      this.visible = false;
    }
  }
};
</script>