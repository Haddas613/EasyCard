<template>
  <ec-dialog :dialog.sync="visible">
    <template v-slot:title>{{$t('FilterPaymentRequests')}}</template>
    <template v-slot:right>
      <v-btn color="primary" @click="apply()">{{$t('Apply')}}</v-btn>
    </template>
    <template>
      <div class="d-flex px-4 py-2 justify-end">
        <v-btn color="primary" 
          :block="$vuetify.breakpoint.smAndDown" 
          outlined
          @click="model = {}">{{$t("Reset")}}</v-btn>
      </div>
      <div class="px-4 py-2">
        <v-row>
          <v-col cols="12" md="6" class="py-0">
            <v-text-field
              v-model="model.paymentRequestID"
              :label="$t('PaymentRequestID')"
              outlined
            ></v-text-field>
          </v-col>
          <v-col cols="12" md="6" class="py-0">
            <v-autocomplete
              :items="terminals"
              item-text="label"
              item-value="terminalID"
              v-model="model.terminalID"
              outlined
              :label="$t('Terminal')"
              clearable
            ></v-autocomplete>
          </v-col>
          <v-col cols="12" md="6" class="py-0">
            <v-select
              :items="dictionaries.paymentRequestTypeEnum"
              item-text="description"
              item-value="code"
              v-model="model.paymentRequestTypeFilter"
              :label="$t('PaymentRequestType')"
              outlined
              clearable
            ></v-select>
          </v-col>
          <v-col cols="12" md="6" class="py-0">
            <v-select
              :items="dictionaries.quickTimeFilterTypeEnum"
              item-text="description"
              item-value="code"
              v-model="model.quickTimeFilter"
              :label="$t('QuickTime')"
              outlined
              clearable
            ></v-select>
          </v-col>
          <v-col cols="12" md="6" class="py-0">
            <v-select
              :items="dictionaries.dateFilterTypeEnum"
              item-text="description"
              item-value="code"
              v-model="model.dateType"
              :label="$t('DateType')"
              outlined
              clearable
            ></v-select>
          </v-col>
           <v-col cols="12" md="6" class="py-0">
            <v-text-field
              v-model="model.consumerEmail"
              :label="$t('ConsumerEmail')"
              outlined
            ></v-text-field>
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
        </v-row>
      </div>
    </template>
  </ec-dialog>
</template>

<script>
export default {
  components: {
    EcDialog: () => import("../../components/ec/EcDialog")
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
      terminals: []
    };
  },
  async mounted() {
    this.dictionaries = await this.$api.dictionaries.getTransactionDictionaries();
    this.terminals = (await this.$api.terminals.getTerminals()).data || [];
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
      this.$emit('ok', this.model);
      this.visible = false;
    }
  },
};
</script>