<template>
  <v-form class="pt-0" ref="form" v-model="valid" lazy-validation>
    <v-row>
      <v-col cols="12" class="py-0 d-flex justify-center">
        <v-switch
          v-model="model.invoiceOnly"
          :label="$t('InvoiceOnly')"
          :disabled="!!model.billingDealID"
          @change="invoiceOnlySwitchChanged()"
          class="pt-0 mt-0"></v-switch>
      </v-col> 
      <v-col cols="12" md="6" class="py-0">
        <terminal-select v-model="model.terminalID" disabled></terminal-select>
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
          :terminal="model.terminalID" 
          :customer-id="model.dealDetails.consumerID" 
          @update="processCustomer($event)"
          full-customer-info
          ref="customerDialogInvoker"></customer-dialog-invoker>
      </v-col>
      <v-col cols="12" md="6" class="pb-2" v-bind:class="{'pt-2': $vuetify.breakpoint.smAndDown, 'pt-5': $vuetify.breakpoint.mdAndUp}">
        <payment-type 
          @change="onPaymentTypeChanged($event)"
          :key="model.invoiceOnly"
          :disabled="!!model.billingDealID"
          v-model="model.paymentType"
          :exclude-types="excludePaymentTypes"
        ></payment-type>
      </v-col>
      <template v-if="model.invoiceOnly">
        <invoice-credit-card-details-fields :data="model.paymentDetails[0]" ref="ccDetails" v-if="model.paymentType == $appConstants.transaction.paymentTypes.card"></invoice-credit-card-details-fields>
        <cheque-details-fields ref="chequeDetails" :data="model.paymentDetails[0]" v-else-if="model.paymentType == $appConstants.transaction.paymentTypes.cheque"></cheque-details-fields>
        <bank-transfer-details-fields :key="customerID" ref="bankDetails" :data="model.paymentDetails[0]" v-else-if="model.paymentType == $appConstants.transaction.paymentTypes.bank"></bank-transfer-details-fields>
      </template>
      <template v-else>
        <v-col
          cols="12"
          class="pb-2"
          v-bind:class="{'pt-2': $vuetify.breakpoint.smAndDown, 'pt-0': $vuetify.breakpoint.mdAndUp}"
          v-if="model.paymentType == $appConstants.transaction.paymentTypes.card"
        >
          <v-flex class="d-flex justify-end">
            <v-btn
              color="success"
              x-small
              :disabled="!model.dealDetails.consumerID"
              @click="ctokenDialog = true;"
              v-if="$featureEnabled(terminalStore, $appConstants.terminal.features.CreditCardTokens)"
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
            v-on:ok="onCreateCardToken($event)"
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
            :key="customerTokens.length"
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
          :key="customerID"
          cols="12"
          class="pb-2"
          v-bind:class="{'pt-2': $vuetify.breakpoint.smAndDown, 'pt-0': $vuetify.breakpoint.mdAndUp}"
          v-else-if="model.paymentType == $appConstants.transaction.paymentTypes.bank">
          <bank-details-fields :data="model.bankDetails" ref="bankDetails"></bank-details-fields>
        </v-col>
      </template>
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
        :data="model"
        :key="model.dealDetails.consumerName"
      ></deal-details>
      <v-col cols="12">
        <v-switch
          v-show="!model.invoiceOnly"
          v-if="$integrationAvailable(terminalStore, $appConstants.terminal.integrations.invoicing)"
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
    BankDetailsFields: () => import("../transactions/BankDetailsFields"),
    PaymentType: () => import("../transactions/PaymentType"),
    InvoiceCreditCardDetailsFields: () => import("../invoicing/InvoiceCreditCardDetailsFields"),
    ChequeDetailsFields: () => import("../invoicing/ChequeDetailsFields"),
    BankTransferDetailsFields: () => import("../invoicing/BankTransferDetailsFields"),
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
      issueInvoiceDisabled: false,
      customer: null,
      customerID: null,
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
    },
    excludePaymentTypes() {
      let types = this.model.invoiceOnly ? ['cash', 'invoice-only'] : ['cash', 'cheque', 'invoice-only'];

      if (this.terminalStore.bankDetails){
        let bankTypeAvailable = this.terminalStore.bankDetails.instituteName
          && this.terminalStore.bankDetails.instituteNum
          && this.terminalStore.bankDetails.instituteServiceNum;

        if (!bankTypeAvailable){
          types.push('bank');
        }
      } else {
        types.push('bank');
      }
      return types;
    }
  },
  methods: {
    async processCustomer(data) {
      let customerChanged = this.model.dealDetails.consumerID !== data.consumerID;
      if (customerChanged){
        this.token = null;
      }

      this.model.dealDetails = Object.assign(this.model.dealDetails, {
        consumerEmail: data.consumerEmail,
        consumerPhone: data.consumerPhone,
        consumerID: data.consumerID,
        consumerAddress: data.consumerAddress,
        consumerNationalID: data.consumerNationalID,
        consumerName: data.consumerName
      });
      this.customer = data;
      await this.getCustomerTokens();
      this.onPaymentTypeChanged(this.model.paymentType, customerChanged);
      this.customerID = this.customer.consumerID;
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

      if(!this.model.transactionAmount){
        this.$toasted.show(this.$t("SelectItems"), { type: "error" });
        return;
      }

      if (this.model.dealDetails.consumerID == null){
        this.$toasted.show(this.$t("PleaseSelectCustomerFirst"), { type: "error" });
        this.$refs.customerDialogInvoker.showDialog();
        return;
      }

      if(this.model.invoiceOnly){
        result.paymentDetails = [];
        let data = null;
        switch(this.model.paymentType){
          case this.$appConstants.transaction.paymentTypes.card:
            data = this.$refs.ccDetails.getData();
            if(data){
              result.paymentDetails.push({
                ...data,
                paymentType: this.$appConstants.transaction.paymentTypes.card
              });
            }
            break;
          case this.$appConstants.transaction.paymentTypes.cash:
            result.paymentDetails.push({
              paymentType: this.$appConstants.transaction.paymentTypes.cash
            });
            break;
          case this.$appConstants.transaction.paymentTypes.cheque:
            data = this.$refs.chequeDetails.getData();
            if(data){
              result.paymentDetails.push({
                ...data,
                paymentType: this.$appConstants.transaction.paymentTypes.cheque
              });
            }
            break;
          case this.$appConstants.transaction.paymentTypes.bank:
            data = this.$refs.bankDetails.getData();
            if(data){
              result.paymentDetails.push({
                ...data,
                paymentType: this.$appConstants.transaction.paymentTypes.bank
              });
            }
            break;
        }
      }else{
        if(this.model.paymentType == this.$appConstants.transaction.paymentTypes.bank){
          result.creditCardToken = null;
          result.bankDetails = this.$refs.bankDetails.getData();
        }
        else if(this.model.paymentType == this.$appConstants.transaction.paymentTypes.card){
          if(!this.token){
            this.$toasted.show(this.$t("PleaseSelectCardToken"), { type: "error" });
            if(this.customerTokens.length > 0){
              this.tokensDialog = true;
            }
            return;
          }
          result.bankDetails = null;
        }
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
    async onCreateCardToken(data) {
      this.ctokenDialog = false;
      await this.getCustomerTokens();
        this.token = this.lodash.find(
          this.customerTokens,
          t => t.creditCardTokenID == data.creditCardTokenID
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
      if(this.customerTokens.length === 1 && !this.model.billingDealID){
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
    invoiceOnlySwitchChanged(){
      if(this.model.invoiceOnly){
        this.token = null;
        this.model.paymentDetails = [];
        this.model.issueInvoice = true;
      }else{
        this.model.paymentDetails = null;
      }
    },
    onPaymentTypeChanged(pt, forcePaymentDetailsChange = false){
      if (pt == this.$appConstants.transaction.paymentTypes.bank){
        if((this.customer && this.customer.bankDetails) && (!this.model.paymentDetails || !this.model.paymentDetails.length)){
          if(this.model.invoiceOnly){
            this.model.paymentDetails = [{
              ...this.customer.bankDetails,
              paymentType: this.$appConstants.transaction.paymentTypes.bank
            }];
          }else {
            this.model.bankDetails = { ...this.customer.bankDetails };
          }
        } else if(forcePaymentDetailsChange){
          if(this.model.invoiceOnly){
            this.model.paymentDetails = [];
          }else {
            this.model.bankDetails = null;
          }
        }
      }
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
    if (this.model.dealDetails.consumerID && this.model.paymentType == this.$appConstants.transaction.paymentTypes.card) {
      this.customerTokens =
        (
          await this.$api.cardTokens.getCustomerCardTokens(
            this.model.dealDetails.consumerID
          )
        ).data || [];
      if (this.model.creditCardToken) {
        if(!this.model.cardExpired){
          this.selectedToken = this.lodash.find(
            this.customerTokens,
            t => t.creditCardTokenID === this.model.creditCardToken
          );
        }else{
          this.token = null;
          this.model.creditCardDetails = null;
        }
      }
    }
    if(this.model.currency != 'ILS'){
      this.issueInvoice = false;
      this.issueInvoiceDisabled = true;
    }
    else if (!this.model.billingDealID) {
      this.model.issueInvoice = !!this.$integrationAvailable(this.terminalStore, this.$appConstants.terminal.integrations.invoicing);
    }
    
    if(!this.model.vatRate){
      this.model.vatRate = this.terminalStore.settings.vatRate;
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