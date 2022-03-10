<template>
  <v-flex fluid fill-height>
    <navbar
      v-on:back="goBack()"
      v-on:close="$router.push({name: 'Dashboard'})"
      v-on:terminal-changed="terminalChanged()"
      :canchangeterminal="!result"
      one-level
      closeable></navbar>
    <v-form v-if="!result" class="px-1 pt-4" ref="form" v-model="valid" lazy-validation :key="terminal.terminalID">
      <v-row no-gutters>
        <v-col cols="12" class="px-4">
          <v-text-field
            v-if="browser.os == 'Android OS'"
            class="centered-input amount-input"
            v-model.number="model.transactionAmount"
            type="number"
            inputmode="decimal"
            min="0"
            outlined
            :rules="[vr.primitives.numeric(true), vr.primitives.biggerThan(0), vr.primitives.precision(2)]"
            autofocus
            @input="adjustItemsAmountToTotalAmount()"
            @click="clearAmountInputIfRequired()"
            @focus="clearAmountInputIfRequired()"
          >
            <template v-slot:append>
              <span class="currency-icon">{{currency.description}}</span>
            </template>
          </v-text-field>
          <v-text-field
            v-else
            class="centered-input amount-input"
            v-model.number="model.transactionAmount"
            type="text"
            inputmode="decimal"
            min="0"
            outlined
            :rules="[vr.primitives.numeric(true), vr.primitives.biggerThan(0), vr.primitives.precision(2)]"
            autofocus
            v-input-decimal
            @input="adjustItemsAmountToTotalAmount()"
            @click="clearAmountInputIfRequired()"
            @focus="clearAmountInputIfRequired()"
          >
            <template v-slot:append>
              <span class="currency-icon">{{currency.description}}</span>
            </template>
          </v-text-field>
        </v-col>
        <v-col cols="12" class="pt-1">
          <numpad-dialog-invoker
            :amount="model.transactionAmount"
            items-only
            class="mx-4"
            :data="model"
            ref="numpadInvoker"
            @ok="processAmount($event)"
          ></numpad-dialog-invoker>
        </v-col>
        <v-col cols="12" class="pt-0">
          <basket
            v-if="model.dealDetails.items && model.dealDetails.items.length" 
            :key="(model.vatRate > 0 ? model.vatRate : 1) + model.dealDetails.items.length + model.totalAmount"
            embed
            v-on:update="processAmount($event)" 
            :data="model"></basket>
        </v-col>
        <v-col cols="12">
          <credit-card-secure-details
            :key="model.key"
            :data="model"
            v-on:ok="processCreditCard($event)"
            include-device
            include-customer
            v-on:select-customer="customersDialog = true"
            ref="ccSecureDetails"
            :btn-text="null"
            allow-bit
          ></credit-card-secure-details>
          <customer-dialog-invoker
            :show.sync="customersDialog"
            dialog-only
            :terminal="terminal.terminalID" 
            :customer-id="model.dealDetails.consumerID" 
            @update="processCustomer($event)"></customer-dialog-invoker>
        </v-col>
        <v-col cols="12" class="px-4">
          <v-text-field
            v-model="model.dealDetails.consumerEmail"
            :counter="50"
            :rules="[vr.primitives.email]"
            :label="$t('CustomerEmail')"
            outlined
          ></v-text-field>
          <div hidden>
            <additional-settings-form
              :data="model"
              :issue-document="true"
              ref="additionalSettingsForm"
            ></additional-settings-form>
          </div>
        </v-col>
      </v-row>
      <v-row no-gutters class="mx-2">
        <v-col cols="12">
          <v-btn color="primary" :disabled="!valid" bottom block @click="createTransaction()">
            {{$t("Charge")}}
            <ec-money :amount="model.transactionAmount" class="px-1" :currency="model.currency"></ec-money>
          </v-btn>
        </v-col>
      </v-row>
    </v-form>
    <wizard-result :errors="errors" v-if="result" :error="error">
      <template v-if="customer">
        <v-icon class="success--text font-weight-thin" size="170">mdi-check-circle-outline</v-icon>
        <p>{{customer.consumerName}}</p>
        <div class="pt-5">
          <p>{{$t("ChargedCustomer")}}</p>
          <p>{{customer.consumerEmail}} ● <ec-money :amount="model.transactionAmount" bold></ec-money></p>
        </div>
      </template>
      <template v-else>
        <v-icon class="success--text font-weight-thin" size="170">mdi-check-circle-outline</v-icon>
        <div class="pt-5">
          <p>{{$t("ChargedCustomer")}}</p>
          <p><ec-money :amount="model.transactionAmount" bold></ec-money></p>
        </div>
      </template>
      <template v-slot:link v-if="result.entityReference">
        <router-link class="primary--text" link
          :to="{ name: 'Transaction', params: { id: result.entityReference } }"
        >{{$t("GoToTransaction")}}</router-link>

        <template v-if="result.innerResponse">
          <p class="pt-4" v-if="result.innerResponse.status == 'error'">
            {{result.innerResponse.message}}
          </p>
          <p class="pt-4" v-else>
            <router-link class="primary--text" link
              :to="{ name: 'Invoice', params: { id: result.innerResponse.entityReference } }"
            >{{$t("GoToInvoice")}}</router-link>
          </p>
        </template>

        <v-flex class="text-center pt-2">
          <v-btn outlined color="success" link :to="{name: 'Dashboard'}">{{$t("Close")}}</v-btn>
        </v-flex>
      </template>
      <template v-slot:errors v-if="result.additionalData && result.additionalData.authorizationCodeRequired">
        <v-form class="my-4 ec-form" ref="form" lazy-validation>
          <!-- <p>{{result.additionalData.message}}</p> -->
          <v-text-field
            v-model="model.oKNumber"
            :label="$t('AuthorizationCode')"
            type="number"
            :rules="[vr.primitives.stringLength(1, 50)]">
          </v-text-field>
          <v-btn color="primary" bottom :x-large="true" block @click="retry()">
            {{$t("Retry")}}
            <ec-money :amount="model.transactionAmount" class="px-1" :currency="model.currency"></ec-money>
          </v-btn>
        </v-form>
      </template>
      <template v-slot:slip v-if="transaction">
        <transaction-printout ref="printout" :transaction="transaction"></transaction-printout>
        <transaction-slip-dialog ref="slipDialog" :transaction="transaction" :show.sync="transactionSlipDialog"></transaction-slip-dialog>
        <div class="mb-4">
          <v-btn small class="mx-1" @click="$refs.printout.print()">
            {{$t("Print")}}
            <v-icon class="px-1" small :right="$vuetify.ltr">
              mdi-printer
            </v-icon>
          </v-btn>
          <v-btn small color="primary" class="mx-1" @click="transactionSlipDialog = true">
            {{$t("Email")}}
            <v-icon small class="px-1" :right="$vuetify.ltr">
              mdi-email
            </v-icon>
          </v-btn>
        </div>
      </template>
    </wizard-result>
  </v-flex>
</template>

<script>
import { mapState } from "vuex";
import * as signalR from "@microsoft/signalr";
import ValidationRules from "../../helpers/validation-rules";
import { detect } from "detect-browser";

export default {
  components: {
    Navbar: () => import("../../components/wizard/NavBar"),
    Basket: () => import("../../components/misc/Basket"),
    EcMoney: () => import("../../components/ec/EcMoney"),
    CustomersList: () => import("../../components/customers/CustomersList"),
    CreditCardSecureDetails: () => import("../../components/transactions/CreditCardSecureDetails"),
    WizardResult: () => import("../../components/wizard/WizardResult"),
    AdditionalSettingsForm: () => import("../../components/transactions/AdditionalSettingsForm"),
    TransactionPrintout: () => import("../../components/printouts/TransactionPrintout"),
    TransactionSlipDialog: () => import("../../components/transactions/TransactionSlipDialog"),
    NumpadDialogInvoker: () => import("../../components/dialog-invokers/NumpadDialogInvoker"),
    CustomerDialogInvoker: () => import("../../components/dialog-invokers/CustomerDialogInvoker"),
  },
  props: ["customerid"],
  data() {
    return {
      vr: ValidationRules,
      valid: false,
      customer: null,
      model: {
        key: "0",
        pinPad: false,
        terminalID: null,
        transactionType: null,
        jDealType: null,
        currency: null,
        cardPresence: "cardNotPresent",
        creditCardToken: null,
        creditCardSecureDetails: {
          cardExpiration: null,
          cardNumber: null,
          cvv: null,
          cardOwnerNationalID: null,
          cardOwnerName: null
        },
        transactionAmount: 0,
        dealDetails: {
          dealReference: null,
          consumerEmail: null,
          consumerPhone: null,
          consumerID: null,
          dealDescription: null,
          items: []
        },
        invoiceDetails: null,
        installmentDetails: {
          numberOfPayments: 0,
          initialPaymentAmount: 0,
          installmentPaymentAmount: 0
        },
        vatTotal: null,
        netTotal: null,
        vatRate: null
      },
      customersDialog: false,
      success: true,
      errors: [],
      error: null,
      loading: false,
      result: null,
      transactionsHub: null,
      signalRToast: null,
      transaction: null,
      transactionSlipDialog: false,
      totalAmountTimeout: null,
      browser: detect()
    };
  },
  computed: {
    ...mapState({
      terminal: state => state.settings.terminal,
      currency: state => state.settings.currency
    })
  },
  async mounted() {
    if (this.customerid) {
      let data = await this.$api.consumers.getConsumer(this.customerid);
      if (data) {
        this.skipCustomerStep = true;
        this.customer = data;
        this.model.dealDetails.consumerEmail = data.consumerEmail;
        this.model.dealDetails.consumerPhone = data.consumerPhone;
        this.model.dealDetails.consumerAddress = data.consumerAddress;
        this.model.dealDetails.consumerID = data.consumerID;
        this.model.creditCardSecureDetails.cardOwnerName = data.consumerName;
        this.model.creditCardSecureDetails.cardOwnerNationalID =
          data.consumerNationalID;

        if(this.model.dealDetails){
          this.model.key = `${this.terminal.terminalID}-${this.model.dealDetails.consumerID}`;
        }
      }
    }
    
    this.model.dealDetails.dealDescription = this.terminal.settings.defaultChargeDescription;
  },
  methods: {
    terminalChanged() {
      this.skipCustomerStep = false;
      this.customer = null;
      this.model.dealDetails.consumerEmail = null;
      this.model.dealDetails.consumerPhone = null;
      this.model.dealDetails.consumerAddress = null;
      this.model.dealDetails.consumerID = null;
      if (this.model.creditCardSecureDetails) {
        this.model.creditCardSecureDetails.cardOwnerName = null;
        this.model.creditCardSecureDetails.cardOwnerNationalID = null;
      } else if (this.model.creditCardToken) {
        this.model.creditCardToken = null;
        this.$refs.ccSecureDetails.resetToken();
      }
      this.model.dealDetails.dealDescription = this.terminal.settings.defaultChargeDescription;
      this.model.vatRate = this.terminal.settings.vatExempt ? 0 : this.terminal.settings.vatRate;
    },
    processCustomer(data) {
      this.customer = data;
      this.model.dealDetails = !data ? null : Object.assign(this.model.dealDetails, {
        consumerEmail: data.consumerEmail,
        consumerPhone: data.consumerPhone,
        consumerID: data.consumerID,
        consumerAddress: data.consumerAddress,
        consumerNationalID: data.consumerNationalID,
        consumerName: data.consumerName
      });
      if(this.model.dealDetails){
        this.model.key = `${this.terminal.terminalID}-${this.model.dealDetails.consumerID}`;
      }
      if (!this.model.creditCardSecureDetails) {
        this.$set(this.model, "creditCardSecureDetails", {
          cardOwnerName: data.consumerName,
          cardOwnerNationalID: data.consumerNationalID
        });
      } else {
        this.model.creditCardSecureDetails.cardOwnerName = data.consumerName;
        this.model.creditCardSecureDetails.cardOwnerNationalID =
          data.consumerNationalID;
      }
      this.model.cardOwnerNationalID = data.consumerNationalID;
      this.model.creditCardToken = null;
      this.$refs.ccSecureDetails.resetToken();
      this.customersDialog = false;
    },
    // processAmount(data, skipBasket = false) {
    //   this.updateAmount(data);
    // },
    processAmount(data) {
      this.model.transactionAmount = data.totalAmount;
      this.model.netTotal = data.netTotal;
      this.model.vatTotal = data.vatTotal;
      this.model.vatRate = data.vatRate;
      this.model.note = data.note;
      this.model.dealDetails.items = data.dealDetails.items;
      this.$refs.numpadInvoker.refreshQuantity();
    },
    processCreditCard(data) {
      this.model.oKNumber = data.oKNumber;
      this.model.useBit = data.useBit;

      this.$set(this.model, 'installmentDetails', data.installmentDetails);
      this.model.transactionType = data.transactionType;

      if(data.dealDetails){
        this.model.dealDetails.consumerName = data.dealDetails.consumerName;
      }
      
      if (data.type === "creditcard") {
        data = data.data;
        this.model.saveCreditCard = data.saveCreditCard || false;
        this.model.creditCardSecureDetails = data;
        this.model.creditCardToken = null;
        this.model.pinPadDeviceID = null;
        this.model.cardOwnerNationalID = null;
        if (data.cardReaderInput) {
          this.model.cardPresence = "regular";
        } else {
          this.model.cardPresence = "cardNotPresent";
        }
        
        if (!this.model.dealDetails.consumerName) {
          this.model.dealDetails.consumerName = this.model.creditCardSecureDetails.cardOwnerName;
        }

      } else if (data.type === "token") {
        this.model.creditCardSecureDetails = null;
        this.model.creditCardToken = data.data;
        this.model.saveCreditCard = false;
        this.model.pinPadDeviceID = null;
        this.model.cardOwnerNationalID = null;
      } 
      else if (data.type === "device") {
        this.model.creditCardSecureDetails = null;
        this.model.creditCardToken = null;
        this.model.saveCreditCard = false;
        Object.assign(this.model, data.data);
      } 
      else if(data.useBit){
        this.model.creditCardSecureDetails = null;
        this.model.creditCardToken = null;
        this.model.saveCreditCard = false;
        this.model.bitPaymentInitiationId = data.data.bitPaymentInitiationId;
        this.model.bitTransactionSerialId = data.data.bitTransactionSerialId;
      }
    },
    async processAdditionalSettings(data) {
      this.model.dealDetails = data.dealDetails;
      this.model.currency = data.currency;
      this.model.jDealType = data.jDealType;
      this.model.terminalID = this.terminal.terminalID;
      this.model.invoiceDetails = data.invoiceDetails;
      this.model.issueInvoice = !!this.model.invoiceDetails;

      await this.createTransaction();
    },
    async retry(){
      await this.createTransaction(true);
    },
    async createTransaction(retry = false){
      if (this.loading || (!retry && !this.validate())) return;
      try {
        this.loading = true;
        if(this.model.pinPad){
          await this.establishSignalRConnection();
        }

        if(!retry){
          let asf = this.$refs.additionalSettingsForm.ok(true)
          if (!asf) {
            this.$toasted.show(this.$t("SomethingWentWrong"), { type: "error" });
            return;
          } else {
            this.processAdditionalSettings(asf)
          }
        }

        let result = await this.$api.transactions.processTransaction(this.model);
        
        if (!result || result.status === "error") {
          this.success = false;
          this.error = result.message;
          if (result.additionalData && result.additionalData.authorizationCodeRequired){
            this.result = result;
          }
          else if (result && result.errors && result.errors.length > 0) {
            result.errors.forEach(e => {
              this.$toasted.show(e.description, { type: "error" });
            })
            this.errors = result.errors;
          }else{
            this.$toasted.show(result.message, { type: "error" });
          }
        } 
        else {
          this.result = result;
          this.success = true;
          if(this.model.pinPad){
            this.disposeSignalRConnection();
          }
          this.errors = [];
          this.error = null;
          this.transaction = await this.$api.transactions.getTransaction(this.result.entityReference);
        }
        if (this.customer) {
          this.$store.commit("payment/addLastChargedCustomer", {
            customerID: this.customer.consumerID,
            terminalID: this.model.terminalID
          });
        }
      }finally{
        this.loading = false;
      }
    },
    validate(){
      if (!this.$refs.form.validate()) return false;

      let ccValid = this.$refs.ccSecureDetails.ok();
      
      if(!ccValid){
        return;
      }

      return true;
    },
    async establishSignalRConnection(){
      const options = {
        accessTokenFactory: () => {
          return this.$oidc.getAccessToken();
        },
        transport: 1
      };

      this.transactionsHub = new signalR.HubConnectionBuilder()
        .withUrl(
            `${this.$cfg.VUE_APP_TRANSACTIONS_API_BASE_ADDRESS}/hubs/transactions`,
            options
        )
        .withAutomaticReconnect()
        .configureLogging("Warning")
        .build();
        
      this.transactionsHub.on("TransactionStatusChanged", (payload) => {
        if(this.signalRToast){
          this.signalRToast.text(payload.statusString)
        }else{
          this.signalRToast = this.$toasted.show(payload.statusString, { type: "info", action: [] });
        }
      });

      await this.transactionsHub.start();
      this.model.connectionID = this.transactionsHub.connectionId;
    },
    async disposeSignalRConnection(){
      if(!this.transactionsHub){
        return;
      }
      if(this.signalRToast){
        this.signalRToast.goAway();
        this.signalRToast = null;
      }

      this.transactionsHub.stop();
    },
    adjustItemsAmountToTotalAmount(){
      if(this.totalAmountTimeout){
        clearTimeout(this.totalAmountTimeout);
      }
      if( this.model.transactionAmount < 0){
        return;
      }
      //this.$refs.numpadInvoker.recalculate();
      this.totalAmountTimeout = setTimeout(() => {
        this.$refs.numpadInvoker.recalculate(this.model.vatRate);
      }, 500);
    },
    clearAmountInputIfRequired(){
      if(this.model.transactionAmount === 0){
        this.model.transactionAmount = null;
      }
    }
  },
  async beforeDestroy () {
    await this.disposeSignalRConnection();
  },
};
</script>
<style lang="scss" scoped>
.currency-icon{
  font-size: 1.25rem;
}
.amount-input{
  -webkit-appearance: none;
  line-height: 2rem;
  font-size: 2rem;
}
/* Safari 11+ */
@media not all and (min-resolution:.001dpcm)
  { @supports (-webkit-appearance:none) and (stroke-color:transparent) {
  .amount-input{
    -webkit-appearance: none;
    line-height: 1rem !important;
    font-size: 1rem !important;
  }
}}
</style>