<template>
  <ec-dialog :dialog.sync="visible">
    <template v-slot:title>{{$t('SendTransactionSlipEmail')}}</template>
    <template>
      <div class="px-4 py-2">
        <v-form ref="form">
          <v-row>
            <v-col cols="12">
              <v-text-field
                v-model="email"
                :append-outer-icon="loading ? null : 'mdi-send'"
                @click:append-outer="sendEmail"
                :label="$t('Email')"
                :rules="[vr.primitives.required, vr.primitives.email]"
                :loading="loading"
              ></v-text-field>
            </v-col>
          </v-row>
        </v-form>
        <transaction-printout ref="printout" :transaction="transaction" visible></transaction-printout>
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
    transaction: {
      type: Object,
      required: true
    }
  },
  components: {
    EcDialog: () => import("../ec/EcDialog"),
    TransactionPrintout: () =>
      import("../../components/printouts/TransactionPrintout")
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
      this.email = this.transaction.dealDetails.consumerEmail;
  },
  methods: {
    async sendEmail() {
      if (!this.$refs.form.validate()) {
        return;
      }
      this.loading = true;
      let operationResult = await this.$api.transactions.sendTransactionSlipEmail({
          transactionID: this.transaction.$paymentTransactionID || this.transaction.paymentTransactionID,
          email: this.email
      });
      this.loading = false;
      this.visible = false;
    }
  }
};
</script>