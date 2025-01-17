<template>
  <ec-dialog :dialog.sync="visible">
    <template v-slot:title>{{$t('FilterPaymentRequests')}}</template>
    <template v-slot:right>
      <v-btn color="primary" @click="apply()" :disabled="!formIsValid">{{$t('Apply')}}</v-btn>
    </template>
    <template>
      <div class="d-flex px-4 py-2 justify-end">
        <v-btn color="primary" 
          :block="$vuetify.breakpoint.smAndDown" 
          outlined
          @click="model = {}">{{$t("Reset")}}</v-btn>
      </div>
      <div class="px-4 py-2">
        <v-form ref="form" v-model="formIsValid">
          <date-from-to-filter v-model="model"></date-from-to-filter>
          <v-row>
            <v-col cols="12" md="6" class="py-0">
              <v-text-field
                v-model="model.paymentRequestID"
                :label="$t('PaymentRequestID')"
                :rules="[vr.primitives.guid]"
                outlined
              ></v-text-field>
            </v-col>
            <v-col cols="12" md="6" class="py-0">
              <terminal-select v-model="model.terminalID" clearable show-deleted></terminal-select>
            </v-col>
            <v-col cols="12" md="6" class="py-0">
              <v-select
                :items="dictionaries.payReqQuickStatusFilterTypeEnum"
                item-text="description"
                item-value="code"
                v-model="model.quickStatus"
                :label="$t('PaymentRequestStatus')"
                outlined
                clearable
              ></v-select>
            </v-col>
            <v-col cols="12" md="6" class="py-0">
              <v-select
                :items="dictionaries.quickDateFilterTypeEnum"
                item-text="description"
                item-value="code"
                v-model="model.quickDateFilter"
                :label="$t('QuickDate')"
                outlined
                clearable
              ></v-select>
            </v-col>
          </v-row>
          <v-row>
            <v-col cols="12" md="6" class="py-0">
              <v-select
                :items="dictionaries.currencyEnum"
                item-text="description"
                item-value="code"
                v-model="model.currency"
                :label="$t('Currency')"
                outlined
                clearable
              ></v-select>
            </v-col>
            <v-col cols="12" md="6" class="py-0">
              <v-text-field
                v-model="model.paymentRequestAmount"
                :label="$t('Amount')"
                type="number"
                min="0"
                step="0.01"
                outlined
                clearable
              ></v-text-field>
            </v-col>
            <v-col cols="12" md="6" class="py-0">
              <v-text-field
                v-model="model.consumerExternalReference"
                :label="$t('CustomerExternalReference')"
                outlined
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
  components: {
    EcDialog: () => import("../../components/ec/EcDialog"),
    DateFromToFilter: () => import("../filtering/DateFromToFilter"),
  },
  props: {
    show: {
      type: Boolean,
      default: false,
      required: true
    },
    filter: {
      type: Object,
      default: null,
      required: true
    }
  },
  data() {
    return {
      model: { ...this.filter },
      dictionaries: {},
      formIsValid: true,
      vr: ValidationRules
    };
  },
  async mounted() {
    this.dictionaries = await this.$api.dictionaries.getTransactionDictionaries();
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
    apply() {
      if(!this.$refs.form.validate()){
        return;
      }

      this.$emit('ok', this.model);
      this.visible = false;
    }
  },
};
</script>