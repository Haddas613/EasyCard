<template>
  <ec-dialog :dialog.sync="visible">
    <template v-slot:title>{{$t('FilterInvoices')}}</template>
    <template v-slot:right>
      <v-btn color="primary" @click="apply()">{{$t('Apply')}}</v-btn>
    </template>
    <template>
      <div class="d-flex px-4 py-2 justify-end">
        <v-btn
          color="primary"
          :block="$vuetify.breakpoint.smAndDown"
          outlined
          @click="model = {}"
        >{{$t("Reset")}}</v-btn>
      </div>
      <div class="px-4 py-2">
        <v-row>
          <v-col cols="12" md="6" class="py-0">
            <v-text-field v-model="model.invoiceID" :label="$t('InvoiceID')" outlined></v-text-field>
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
              :items="dictionaries.invoiceTypeEnum"
              item-text="description"
              item-value="code"
              v-model="model.invoiceTypeFilter"
              :label="$t('InvoiceType')"
              outlined
              clearable
            ></v-select>
          </v-col>
          <v-col cols="12" md="6" class="py-0">
            <v-text-field v-model="model.consumerEmail" :label="$t('CustomerEmail')" outlined></v-text-field>
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
              v-model="model.invoiceAmount"
              :label="$t('Amount')"
              type="number"
              min="0"
              step="0.01"
              outlined
              clearable
            ></v-text-field>
          </v-col>
        </v-row>
        <v-row>
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
          <v-col cols="12" md="6" class="py-0">
            <v-menu
              ref="dateFromMenu"
              v-model="dateFromMenu"
              :close-on-content-click="false"
              :return-value.sync="model.dateFrom"
              offset-y
              min-width="290px"
            >
              <template v-slot:activator="{ on }">
                <v-text-field
                  v-model="model.dateFrom"
                  :label="$t('DateFrom')"
                  readonly
                  outlined
                  v-on="on"
                ></v-text-field>
              </template>
              <v-date-picker v-model="model.dateFrom" no-title scrollable>
                <v-spacer></v-spacer>
                <v-btn
                  text
                  color="primary"
                  @click="$refs.dateFromMenu.save(model.dateFrom)"
                >{{$t("Ok")}}</v-btn>
              </v-date-picker>
            </v-menu>
          </v-col>
          <v-col cols="12" md="6" class="py-0">
            <v-menu
              ref="dateToMenu"
              v-model="dateToMenu"
              :close-on-content-click="false"
              :return-value.sync="model.dateTo"
              offset-y
              min-width="290px"
            >
              <template v-slot:activator="{ on }">
                <v-text-field
                  v-model="model.dateTo"
                  :label="$t('DateTo')"
                  readonly
                  outlined
                  v-on="on"
                ></v-text-field>
              </template>
              <v-date-picker v-model="model.dateTo" no-title scrollable>
                <v-spacer></v-spacer>
                <v-btn
                  text
                  color="primary"
                  @click="$refs.dateToMenu.save(model.dateTo)"
                >{{$t("Ok")}}</v-btn>
              </v-date-picker>
            </v-menu>
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
      terminals: [],
      dateFromMenu: false,
      dateToMenu: false
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
      this.$emit("ok", this.model);
      this.visible = false;
    }
  }
};
</script>