<template>
  <v-form class="pt-0" ref="form" v-model="valid" lazy-validation>
    <v-row>
      <v-col cols="12" md="6" class="py-0">
        <terminal-select v-model="model.terminalID" clearable show-deleted></terminal-select>
      </v-col>
      <v-col cols="12" md="6" class="py-0">
        <v-select
          :items="dictionaries.currencyEnum"
          item-text="description"
          item-value="code"
          v-model="model.currency"
          required
          :label="$t('Currency')"
          disabled
          outlined
        ></v-select>
      </v-col>
      <v-col cols="12" md="6" class="pb-2" v-bind:class="{'pt-2': $vuetify.breakpoint.smAndDown, 'pt-7': $vuetify.breakpoint.mdAndUp}">
        <customer-dialog-invoker 
          :key="model.dealDetails.consumerID" 
          :terminal="true" 
          :customer-id="model.dealDetails.consumerID" 
          @update="processCustomer($event)"></customer-dialog-invoker>
      </v-col>
      <v-col cols="12" md="6" class="pb-2" v-bind:class="{'pt-2': $vuetify.breakpoint.smAndDown, 'pt-5': $vuetify.breakpoint.mdAndUp}">
        <payment-type v-model="model.paymentType" :exclude-types="['cash', 'cheque']"></payment-type>
      </v-col>
      <v-col
        cols="12"
        class="pb-2"
        v-bind:class="{'pt-2': $vuetify.breakpoint.smAndDown, 'pt-0': $vuetify.breakpoint.mdAndUp}"
        v-if="model.paymentType == appConstants.transaction.paymentTypes.card"
      >
        <v-flex class="d-flex justify-end">
          <v-btn
            color="success"
            x-small
            :disabled="!model.dealDetails.consumerID"
            @click="ctokenDialog = true;"
          >
            <v-icon left small>mdi-plus</v-icon>
            {{$t("AddToken")}}
          </v-btn>
        </v-flex>
        <card-token-form-dialog
          v-if="model.dealDetails.consumerID"
          :key="model.dealDetails.consumerID"
          :customer-id="model.dealDetails.consumerID"
          :show.sync="ctokenDialog"
          v-on:ok="createCardToken($event)"
          ref="ctokenDialogRef"
        ></card-token-form-dialog>
        <ec-dialog :dialog.sync="tokensDialog">
          <template v-slot:title>{{$t('SavedTokens')}}</template>
          <template>
            <ec-radio-group
              :data="customerTokens"
              valuekey="creditCardTokenID"
              item-disabled-key="expired"
              return-object
              :model.sync="token"
            >
              <template v-slot="{ item }">
                <card-token-string :token="item"></card-token-string>
              </template>
            </ec-radio-group>
          </template>
        </ec-dialog>
        <ec-dialog-invoker
          v-on:click="handleClick()"
          :clickable="model.dealDetails.consumerID"
          class="pt-2"
        >
          <template v-slot:prepend>
            <v-icon>mdi-credit-card-outline</v-icon>
          </template>
          <template v-slot:left>
            <div v-if="!token">
              <span
                v-if="customerTokens.length > 0"
              >{{$t("@ChooseFromSavedCount").replace("@count", customerTokens.length)}}</span>
              <span
                class="ecgray--text"
                v-if="!model.dealDetails.consumerID && customerTokens.length === 0"
              >{{$t("PleaseSelectCustomerFirst")}}</span>
              <span
                v-if="model.dealDetails.consumerID && customerTokens.length === 0"
              >{{$t("NoSavedCards")}}</span>
            </div>
            <div v-if="token">
              <span class="primary--text">{{token.cardNumber}}</span>
            </div>
          </template>
          <template v-slot:append>
            <re-icon>mdi-chevron-right</re-icon>
          </template>
        </ec-dialog-invoker>
      </v-col>
      <v-col
        cols="12"
        class="pb-2"
        v-bind:class="{'pt-2': $vuetify.breakpoint.smAndDown, 'pt-0': $vuetify.breakpoint.mdAndUp}"
        v-else-if="model.paymentType == appConstants.transaction.paymentTypes.bank">
        <bank-transfer-details-fields :data="model.bankTransferDetails" ref="bankTransferDetails"></bank-transfer-details-fields>
      </v-col>
      <v-col cols="12">
        <ec-dialog :dialog.sync="scheduleDialog" color="ecbg">
          <template v-slot:title>{{$t('BillingSchedule')}}</template>
          <template v-slot:right>
            <v-btn color="primary" @click="applySchedule()">{{$t('Apply')}}</v-btn>
          </template>
          <template>
            <billing-schedule-form
              ref="billingScheduleRef"
              class="px-4 py-4"
              :data="model.billingSchedule"
            ></billing-schedule-form>
          </template>
        </ec-dialog>
        <ec-dialog-invoker v-on:click="scheduleDialog = true;" class="py-2">
          <template v-slot:prepend>
            <v-icon>mdi-calendar</v-icon>
          </template>
          <template v-slot:left>
            <billing-schedule-string
              :schedule="model.billingSchedule"
              :key="billingScheduleJSON"
              replacement-text="SelectSchedule"
            ></billing-schedule-string>
          </template>
          <template v-slot:append>
            <re-icon>mdi-chevron-right</re-icon>
          </template>
        </ec-dialog-invoker>
      </v-col>
      <v-col cols="12" class="pt-0">
        <numpad-dialog-invoker
          :data="model" 
          @ok="processAmount($event)"></numpad-dialog-invoker>
      </v-col>
      <v-col cols="12" class="pt-0">
        <basket 
          v-if="model.dealDetails.items && model.dealDetails.items.length" 
          :key="model.dealDetails.items.length + model.transactionAmount" 
          embed 
          v-on:ok="processAmount($event)" 
          v-on:update="processAmount($event)" 
          :data="model"></basket>
      </v-col>
      <v-row class="px-2" v-if="false">
        <v-col cols="12" md="6">
          <v-text-field
            class="mt-4"
            v-model="model.transactionAmount"
            outlined
            :label="$t('Amount')"
            hide-details="true"
            @input="calculateTotal()"
            disabled
          ></v-text-field>
        </v-col>
        <v-col cols="12" md="6">
          <v-text-field
            class="mt-4"
            v-if="model"
            :value="model.netTotal"
            outlined
            readonly
            disabled
            :label="$t('NetAmount')"
            hide-details="true"
          ></v-text-field>
        </v-col>
        <v-col cols="12" md="6">
          <v-text-field
            class="mt-4"
            v-if="model"
            :value="model.vatTotal"
            outlined
            readonly
            disabled
            :label="$t('VAT')"
            hide-details="true"
          ></v-text-field>
        </v-col>
        <v-col cols="12" md="6">
          <v-text-field
            class="mt-4"
            :value="(model.vatRate * 100).toFixed(0)"
            readonly
            disabled
            outlined
            :label="$t('VATPercent')"
            hide-details="true"
          ></v-text-field>
        </v-col>
      </v-row>
      <deal-details
        class="px-2"
        ref="dealDetails"
        :data="model.dealDetails"
        :key="model.dealDetails ? model.dealDetails.consumerEmail : model.dealDetails"
      ></deal-details>
      <v-col cols="12">
        <v-switch
          v-if="$integrationAvailable(terminalStore, appConstants.terminal.integrations.invoicing)"
          v-model="model.issueInvoice"
          :label="$t('IssueDocument')"
          :disabled="issueInvoiceDisabled"
          class="pt-0 mt-0"></v-switch>
        <div v-if="model.issueInvoice">
          <invoice-details-fields ref="invoiceDetails" :data="model.invoiceDetails"></invoice-details-fields>
        </div>
      </v-col>
      <v-col cols="12" class="d-flex justify-end" v-if="!$vuetify.breakpoint.smAndDown">
        <v-btn class="mx-1" color="white" :to="{ name: 'BillingDeals' }">{{$t('Cancel')}}</v-btn>
        <v-btn color="primary" @click="ok()" :disabled="!valid">{{$t('OK')}}</v-btn>
      </v-col>
      <v-col cols="12" v-if="$vuetify.breakpoint.smAndDown">
        <v-btn block color="white" :to="{ name: 'BillingDeals' }">{{$t('Cancel')}}</v-btn>
        <v-spacer class="py-2"></v-spacer>
        <v-btn block color="primary" @click="ok()" :disabled="!valid">{{$t('OK')}}</v-btn>
      </v-col>
    </v-row>
  </v-form>
</template>

<script>
import ValidationRules from "../../helpers/validation-rules";
import { mapState } from "vuex";
import itemPricingService from "../../helpers/item-pricing";
import appConstants from "../../helpers/app-constants";

export default {
  components: {
    DealDetails: () => import("../transactions/DealDetailsFields"),
    BillingScheduleForm: () => import("./BillingScheduleForm"),
    BillingScheduleString: () => import("./BillingScheduleString"),
    EcDialog: () => import("../ec/EcDialog"),
    EcDialogInvoker: () => import("../ec/EcDialogInvoker"),
    EcRadioGroup: () => import("../inputs/EcRadioGroup"),
    ReIcon: () => import("../misc/ResponsiveIcon"),
    CardTokenFormDialog: () => import("../ctokens/CardTokenFormDialog"),
    CardTokenString: () => import("../ctokens/CardTokenString"),
    CustomerDialogInvoker: () => import("../dialog-invokers/CustomerDialogInvoker"),
    NumpadDialogInvoker: () => import("../dialog-invokers/NumpadDialogInvoker"),
    Basket: () => import("../misc/Basket"),
    InvoiceDetailsFields: () => import("../invoicing/InvoiceDetailsFields"),
    BankTransferDetailsFields: () => import("../transactions/BankTransferDetailsFields"),
    PaymentType: () => import("../transactions/PaymentType"),
    
  },
  props: {
    data: {
      type: Object,
      default: null,
      required: true
    }
  },
  data() {
    return {
      model: { ...this.data },
      dictionaries: {},
      vr: ValidationRules,
      terminals: [],
      valid: true,
      tokensDialog: false,
      customerTokens: [],
      selectedToken: null,
      scheduleDialog: false,
      ctokenDialog: false,
      billingScheduleJSON: JSON.stringify(this.data.billingSchedule),
      appConstants: appConstants,
      issueInvoiceDisabled: false
    };
  },
  computed: {
    ...mapState({
      terminalStore: state => state.settings.terminal,
      currencyStore: state => state.settings.currency
    }),
    totalAmount() {
      return (this.model.totalAmount = (
        this.model.transactionAmount * this.model.numberOfPayments
      ).toFixed(2));
    },
    token: {
      get: function() {
        return this.selectedToken;
      },
      set: function(nv) {
        this.model.creditCardToken = nv ? nv.creditCardTokenID : null;
        this.selectedToken = nv;
        this.tokensDialog = false;
      }
    }
  },
  methods: {
    async processCustomer(data) {
      this.model.dealDetails.consumerEmail = data.consumerEmail;
      this.model.dealDetails.consumerPhone = data.consumerPhone;
      this.model.dealDetails.consumerID = data.consumerID;
      this.model.dealDetails.consumerAddress = data.consumerAddress;
      await this.getCustomerTokens();
    },
    handleClick() {
      this.tokensDialog = this.customerTokens && this.customerTokens.length > 0;
    },
    ok() {
      if (!this.$refs.form.validate()) return;
      let result = { ...this.model };
      
      result.dealDetails = { ...this.$refs.dealDetails.getData() };
      result.dealDetails.items = this.model.dealDetails.items;
      
      if (result.issueInvoice) {
        result.invoiceDetails = this.$refs.invoiceDetails.getData();
      } else {
        result.invoiceDetails = null;
      }

      //if this is edit and billing schedule has not been clicked, no need to validate
      if (!this.$refs.billingScheduleRef && this.model.billingDealID) {
        return this.$emit("ok", result);
      }
      if (
        !this.$refs.billingScheduleRef ||
        !this.$refs.billingScheduleRef.validate()
      ) {
        this.scheduleDialog = true;
        this.$toasted.show(this.$t("CheckScheduleSettings"), { type: "error" });
        return;
      }

      if(!this.model.transactionAmount){
        this.$toasted.show(this.$t("SelectItems"), { type: "error" });
        return;
      }

      if(this.model.paymentType == appConstants.transaction.paymentTypes.bank){
        result.creditCardToken = null;
        result.bankDetails = this.$refs.bankTransferDetails.getData();
      }
      this.$emit("ok", result);
    },
    applySchedule() {
      if (!this.$refs.billingScheduleRef.validate()) {
        return;
      }
      this.scheduleDialog = false;
      this.model.billingSchedule = this.$refs.billingScheduleRef.model;
      this.billingScheduleJSON = JSON.stringify(this.model.billingSchedule);
    },
    async createCardToken(data) {
      this.ctokenDialog = false;
      let result = await this.$api.cardTokens.createCardToken(data);
      
      if (!this.$apiSuccess(result)) return;
      await this.getCustomerTokens();
      this.token = this.lodash.find(
        this.customerTokens,
        t => t.creditCardTokenID == result.entityReference
      );
      this.$refs.ctokenDialogRef.reset();
    },
    async getCustomerTokens() {
      this.customerTokens =
        (
          await this.$api.cardTokens.getCustomerCardTokens(
            this.model.dealDetails.consumerID
          )
        ).data || [];
      if(this.customerTokens.length === 1){
        this.token = this.customerTokens[0];
      }
    },
    calculateTotal(){
      itemPricingService.total.calculateWithoutItems(this.model, 'transactionAmount', { vatRate: this.terminalStore.settings.vatRate });
    },
    processAmount(data) {
      this.model.transactionAmount = data.totalAmount;
      this.model.netTotal = data.netTotal;
      this.model.vatTotal = data.vatTotal;
      this.model.vatRate = data.vatRate;
      this.model.note = data.note;
      this.model.dealDetails.items = data.dealDetails.items;
      // this.itemsRefreshKey = `${data.totalAmount}:${this.lodash.join(this.lodash.map(data.items, i => i.itemName))}`;
      this.calculateTotal();
    },
  },
  async mounted() {
    this.terminals = (await this.$api.terminals.getTerminals()).data || [];
    this.dictionaries = await this.$api.dictionaries.getTransactionDictionaries();
    if (!this.model.terminalID) {
      this.model.terminalID =
        this.terminalStore.terminalID || this.terminals[0].terminalID;
    }
    if (!this.model.currency) {
      this.model.currency =
        this.currencyStore.code || this.dictionaries.currencyEnum[0].code;
    }
    if (this.model.dealDetails.consumerID && this.model.paymentType == appConstants.transaction.paymentTypes.card) {
      this.customerTokens =
        (
          await this.$api.cardTokens.getCustomerCardTokens(
            this.model.dealDetails.consumerID
          )
        ).data || [];
      if (this.model.creditCardToken) {
        this.selectedToken = this.lodash.find(
          this.customerTokens,
          t => t.creditCardTokenID === this.model.creditCardToken
        );
        
      }
    }
    if(this.model.currency != 'ILS'){
      this.issueInvoice = false;
      this.issueInvoiceDisabled = true;
    }
    else if (this.model.issueInvoice) {
      this.model.invoiceDetails = this.$integrationAvailable(this.terminalStore, appConstants.terminal.integrations.invoicing);
    }
    this.calculateTotal();
  }
};
</script>

<style lang="css" scoped>
.pt-30px {
  padding-top: 30px !important;
}
</style>