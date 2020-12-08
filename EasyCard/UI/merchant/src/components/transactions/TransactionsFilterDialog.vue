<template>
  <ec-dialog :dialog.sync="visible">
    <template v-slot:title>{{$t('FilterTransactions')}}</template>
    <template v-slot:right>
      <v-btn color="primary" @click="apply()">{{$t('Apply')}}</v-btn>
    </template>
    <template>
      <div class="d-flex px-4 py-2 justify-end">
        <v-btn
          color="primary"
          :block="$vuetify.breakpoint.smAndDown"
          outlined
          @click="resetFilter()"
        >{{$t("Reset")}}</v-btn>
      </div>
      <div class="px-4 py-2">
        <v-row>
          <v-col cols="12" md="12" class="pb-2 pt-0">
            <ec-dialog :dialog.sync="customersDialog" color="ecbg">
              <template v-slot:title>{{$t('Customers')}}</template>
              <template>
                <div class="d-flex pb-2 justify-end">
                  <v-btn
                    color="red"
                    class="white--text"
                    :disabled="selectedCustomer == null"
                    :block="$vuetify.breakpoint.smAndDown"
                    @click="selectedCustomer = null; customersDialog = false;"
                  >
                    <v-icon left>mdi-delete</v-icon>
                    {{$t("CancelSelection")}}
                  </v-btn>
                </div>
                <customers-list
                  :key="model.terminalID"
                  :show-previously-charged="true"
                  :filter-by-terminal="model.terminalID"
                  v-on:ok="processCustomer($event)"
                ></customers-list>
              </template>
            </ec-dialog>
            <ec-dialog-invoker
              v-on:click="customersDialog = true"
              v-bind:class="{'pt-2': $vuetify.breakpoint.smAndDown, 'pt-7': $vuetify.breakpoint.mdAndUp}"
            >
              <template v-slot:prepend>
                <v-icon>mdi-account</v-icon>
              </template>
              <template v-slot:left>
                <div v-if="!selectedCustomer">{{$t("ChooseCustomer")}}</div>
                <div v-if="selectedCustomer">
                  <span class="primary--text">{{selectedCustomer.consumerName}}</span>
                </div>
              </template>
              <template v-slot:append>
                <re-icon>mdi-chevron-right</re-icon>
              </template>
            </ec-dialog-invoker>
          </v-col>
          <v-col cols="12" md="6" class="py-0">
            <v-text-field
              v-model="model.paymentTransactionID"
              :label="$t('PaymentTransactionID')"
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
              :items="dictionaries.transactionTypeEnum"
              item-text="description"
              item-value="code"
              v-model="model.transactionType"
              :label="$t('TransactionType')"
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
            <v-select
              :items="dictionaries.quickStatusFilterTypeEnum"
              item-text="description"
              item-value="code"
              v-model="model.quickStatusFilter"
              :label="$t('Status')"
              :disabled="model.notTransmitted"
              outlined
              clearable
              :persistent-hint="model.notTransmitted"
              :hint="$t('CantBeUsedInNotTransmittedMode')"
            ></v-select>
          </v-col>
          <v-col cols="12" md="6" class="py-0">
            <v-select
              :items="dictionaries.cardPresenceEnum"
              item-text="description"
              item-value="code"
              v-model="model.cardPresence"
              :label="$t('CardPresence')"
              outlined
              clearable
            ></v-select>
          </v-col>
        </v-row>
        <v-row>
          <v-col cols="12" md="6" class="py-0">
            <v-text-field
              v-model="model.amountFrom"
              :label="$t('AmountFrom')"
              type="number"
              min="0"
              step="0.01"
              outlined
            ></v-text-field>
          </v-col>
          <v-col cols="12" md="6" class="py-0">
            <v-text-field
              v-model="model.amountTo"
              :label="$t('AmountTo')"
              type="number"
              min="0"
              step="0.01"
              outlined
            ></v-text-field>
          </v-col>
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
            <v-select
              :items="dictionaries.specialTransactionTypeEnum"
              item-text="description"
              item-value="code"
              v-model="model.specialTransactionType"
              :label="$t('SpecialTransactionType')"
              outlined
              clearable
            ></v-select>
          </v-col>
          <v-col cols="12" md="6" class="py-0">
            <v-select
              :items="dictionaries.jDealTypeEnum"
              item-text="description"
              item-value="code"
              v-model="model.jDealType"
              :label="$t('JDealType')"
              outlined
              clearable
            ></v-select>
          </v-col>
          <v-col cols="12" md="6" class="py-0">
            <v-select
              :items="dictionaries.rejectionReasonEnum"
              item-text="description"
              item-value="code"
              v-model="model.rejectionReason"
              :label="$t('RejectionReason')"
              outlined
              clearable
            ></v-select>
          </v-col>
        </v-row>
      </div>
    </template>
  </ec-dialog>
</template>

<script>
export default {
  components: {
    EcRadioGroup: () => import("../../components/inputs/EcRadioGroup"),
    EcDialog: () => import("../../components/ec/EcDialog"),
    EcDialogInvoker: () => import("../../components/ec/EcDialogInvoker"),
    ReIcon: () => import("../../components/misc/ResponsiveIcon"),
    CustomersList: () => import("../../components/customers/CustomersList"),
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
      customersDialog: false,
      selectedCustomer: null,
      terminals: []
    };
  },
  async mounted() {
    this.dictionaries = await this.$api.dictionaries.getTransactionDictionaries();
    this.terminals = (await this.$api.terminals.getTerminals()).data || [];
    if(this.model.consumerID){
      this.processCustomer(await this.$api.consumers.getConsumer(this.model.consumerID))
    }
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
    },
    processCustomer(data) {
      this.selectedCustomer = data;
      this.model.terminalID = data.terminalID;
      this.model.consumerID = data.consumerID;
      this.customersDialog = false;
    },
    resetFilter(){
      this.model = {};
      this.selectedCustomer = null;
    }
  }
};
</script>