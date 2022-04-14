<template>
  <ec-dialog :dialog.sync="visible">
    <template v-slot:title>{{ $t('Refund') }}</template>
    <template>
      <div class="px-4 py-2">
        <v-form ref="form" v-model="formIsValid">
          <v-row>
            <v-col cols="12">
              <v-text-field
                v-model.lazy="amount"
                :label="$t('Amount')"
                :rules="[
                  vr.primitives.biggerThan(0),
                  vr.primitives.precision(2),
                  vr.primitives.lessThan(refundMax, true),
                ]"
                required
                class="px-1"
                outlined
                :loading="loading"
              ></v-text-field>
            </v-col>
            <v-col cols="12" class="d-flex justify-end">
              <v-btn @click="visible = false">{{ $t('Cancel') }}</v-btn>
              <v-btn class="mx-1" color="primary" @click="refund()">{{ $t('Refund') }}</v-btn>
            </v-col>
          </v-row>
        </v-form>
      </div>
    </template>
  </ec-dialog>
</template>

<script>
import ValidationRules from '../../helpers/validation-rules';
export default {
  props: {
    show: {
      type: Boolean,
      default: false,
      required: true,
    },
    transaction: {
      type: Object,
      required: true,
    },
  },
  components: {
    EcDialog: () => import('../ec/EcDialog'),
  },
  computed: {
    visible: {
      get: function() {
        return this.show;
      },
      set: function(val) {
        this.$emit('update:show', val);
      },
    },
  },
  data() {
    return {
      amount: null,
      refundMax: 0,
      totalRefund: this.transaction.totalRefund || 0,
      formIsValid: false,
      vr: ValidationRules,
      loading: false,
    };
  },
  mounted() {
    this.init();
  },
  methods: {
    async refund() {
      if (this.loading || !this.$refs.form.validate()) {
        return;
      }
      this.loading = true;
      let operationResult = await this.$api.transactions.chargeback(
        this.transaction.$paymentTransactionID || this.transaction.paymentTransactionID,
        this.amount);

      this.loading = false;
      this.visible = false;

      if(!this.$apiSuccess(operationResult)){
        this.$toasted.show(operationResult.message || this.$t("Error"), { type: "error" });
        return;
      }
      this.totalRefund += Number(this.amount);
      this.init();
      this.$emit('refund', {
        totalRefund: this.totalRefund,
        transactionID: operationResult.entityReference,
      });
    },
    init() {
      this.amount = this.transaction.transactionAmount - this.totalRefund;
      this.refundMax = this.amount;
    },
  },
};
</script>
