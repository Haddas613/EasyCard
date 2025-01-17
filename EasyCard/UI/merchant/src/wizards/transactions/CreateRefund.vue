<template>
  <v-flex fluid fill-height>
    <navbar
      v-on:back="goBack()"
      v-on:close="$router.push({name: 'Dashboard'})"
      v-on:skip="step = step + 1"
      v-on:terminal-changed="terminalChanged()"
      :skippable="steps[step].skippable"
      :closeable="steps[step].closeable"
      :completed="steps[step].completed"
      :canchangeterminal="steps[step].canChangeTerminal"
      :tdmenuitems="threeDotMenuItems"
      :title="navTitle"
    >
    <template v-if="steps[step].showItemsCount" v-slot:title>
      <v-btn v-if="$refs.numpadRef" :disabled="!$refs.numpadRef.model.dealDetails.items.length" color="ecgray" small @click="processToBasket()">
        {{$t("@ItemsQuantity").replace("@quantity", $refs.numpadRef.model.dealDetails.items.length)}}
      </v-btn>
    </template>
    </navbar>
    <v-stepper class="ec-stepper" v-model="step">
      <v-stepper-items>
        <v-stepper-content step="1" class="py-0 px-0">
          <numpad v-if="step === 1" btn-text="Refund" v-on:ok="processAmount($event, true);" v-on:update="updateAmount($event)" ref="numpadRef" :data="model"></numpad>
        </v-stepper-content>

        <v-stepper-content step="2" class="py-0 px-0">
          <basket v-if="step === 2" btn-text="Total" v-on:ok="processAmount($event)" v-on:update="updateAmount($event)" :data="model"></basket>
        </v-stepper-content>

        <v-stepper-content step="3" class="py-0 px-0">
          <customers-list
            :key="terminal.terminalID"
            :show-previously-charged="true"
            :filter-by-terminal="true"
            v-on:ok="processCustomer($event)"
            support-create
          ></customers-list>
        </v-stepper-content>

        <v-stepper-content step="4" class="py-0 px-0">
          <credit-card-secure-details
            :key="model.key"
            :data="model"
            v-on:ok="processCreditCard($event)"
            include-device
            ref="ccSecureDetails"
            btn-text="Refund"
          ></credit-card-secure-details>
        </v-stepper-content>

        <v-stepper-content step="5" class="py-0 px-0">
          <additional-settings-form 
            :key="model.key"
            :data="model" 
            v-on:ok="processAdditionalSettings($event)" 
            ref="additionalSettingsForm"
            :invoice-type="invoiceType"
          ></additional-settings-form>
        </v-stepper-content>

        <v-stepper-content step="6" class="py-0 px-0">
           <wizard-result :errors="errors" v-if="result" :error="error">
            <v-icon class="success--text font-weight-thin" size="170">mdi-check-circle-outline</v-icon>
            <template v-if="customer">
              <p>{{customer.consumerName}}</p>
              <div class="pt-5">
                <p>{{$t("RefundedCustomer")}}</p>
                <p>{{customer.consumerEmail}} ● <ec-money :amount="model.transactionAmount" bold></ec-money></p>
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
                  :rules="[vr.primitives.stringLength(1, 50)]">
                </v-text-field>
                <v-btn color="primary" bottom :x-large="true" block @click="createRefund()">
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
        </v-stepper-content>
      </v-stepper-items>
    </v-stepper>
  </v-flex>
</template>

<script>
import { mapState } from "vuex";
import * as signalR from "@microsoft/signalr";
import ValidationRules from "../../helpers/validation-rules";

export default {
  components: {
    Navbar: () => import("../../components/wizard/NavBar"),
    Numpad: () => import("../../components/misc/Numpad"),
    Basket: () => import("../../components/misc/Basket"),
    EcMoney: () => import("../../components/ec/EcMoney"),
    CustomersList: () => import("../../components/customers/CustomersList"),
    CreditCardSecureDetails: () => import("../../components/transactions/CreditCardSecureDetails"),
    WizardResult: () => import("../../components/wizard/WizardResult"),
    AdditionalSettingsForm: () => import("../../components/transactions/AdditionalSettingsForm"),
    TransactionPrintout: () => import("../../components/printouts/TransactionPrintout"),
    TransactionSlipDialog: () => import("../../components/transactions/TransactionSlipDialog")
  },
  props: ["customerid"],
  data() {
    return {
      customer: null,
      skipCustomerStep: false,
      invoiceType: null,
      model: {
        key: "0",
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
        transactionAmount: 0.0,
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
        }
      },
      step: 1,
      steps: {
        1: {
          title: "Amount",
          canChangeTerminal: true,
          showItemsCount: true
        },
        2: {
          title: "Basket",
        },
        3: {
          title: "ChooseCustomer",
          skippable: true
        },
        4: {
          title: "PaymentInfo"
          // skippable: true
        },
        5: {
          title: "AdditionalSettings"
        },
        //Last step may be dynamically altered to represent error if transaction creation has failed.
        6: {
          title: "Success",
          completed: true
        }
      },
      threeDotMenuItems: null,
      success: true,
      errors: [],
      error: null,
      result: null,
      loading: false,
      transactionsHub: null,
      signalRToast: null,
      transaction: null,
      transactionSlipDialog: false,
      vr: ValidationRules
    };
  },
  computed: {
    navTitle() {
      return this.$t(this.steps[this.step].title);
    },
    terminal: {},
    ...mapState({
      terminal: state => state.settings.terminal
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
    this.model.dealDetails.dealDescription = this.terminal.settings.defaultRefundDescription;
    this.invoiceType = this.terminal.invoiceSettings.defaultRefundInvoiceType || this.$appConstants.invoicing.defaultRefundInvoiceType;
  },
  methods: {
    goBack() {
      if (this.step === 1) this.$router.push({ name: "Dashboard" });
      else this.step--;
    },
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
    },
    processCustomer(data) {
      this.skipCustomerStep = false;
      if(this.customer && data.consumerID === this.customer.consumerID){
        return this.step++;;
      }
      this.customer = data;
      this.model.dealDetails = Object.assign(this.model.dealDetails, data);
      
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
      this.step++;
    },
    processToBasket(){
      let data = this.$refs.numpadRef.getData();
      this.processAmount(data);
    },
    processAmount(data, skipBasket = false) {
      this.updateAmount(data);
      if (skipBasket) {this.step += 2 + (this.skipCustomerStep ? 1 : 0)}
      else this.step++;
    },
    updateAmount(data) {
      this.model.transactionAmount = data.totalAmount;
      this.model.netTotal = data.netTotal;
      this.model.vatTotal = data.vatTotal;
      this.model.vatRate = data.vatRate;
      this.model.note = data.note;
      this.model.dealDetails.items = data.dealDetails.items;
    },
    processCreditCard(data) {
      this.model.oKNumber = data.oKNumber;
      this.$set(this.model, 'installmentDetails', data.installmentDetails);
      this.model.transactionType = data.transactionType;

      if(data.dealDetails && this.model.dealDetails.consumerName){
        this.model.key = `${this.terminal.terminalID}-${this.model.dealDetails.consumerName}`;
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
      this.model.pinPad = !!this.model.pinPadDeviceID;
      this.step++;
    },
    async processAdditionalSettings(data) {
      this.model.dealDetails = data.dealDetails;
      this.model.currency = data.currency;
      this.model.jDealType = data.jDealType;
      this.model.terminalID = this.terminal.terminalID;
      this.model.invoiceDetails = data.invoiceDetails;
      this.model.issueInvoice = !!this.model.invoiceDetails;
      await this.createRefund();
    },

    async createRefund(){
      if (this.loading) return;
      try{
        if(this.model.pinPadDeviceID){
          await this.establishSignalRConnection();
        } else {
          this.disposeSignalRConnection();
        }

        this.loading = true;
        let result = await this.$api.transactions.refund(this.model);
        this.result = result;

        let lastStepKey = Object.keys(this.steps).reduce((l,r) => l > r ? l : r, 0);
        let lastStep = this.steps[lastStepKey];

        if (!result || result.status === "error") {
          this.success = false;
          lastStep.title = "Error";
          lastStep.completed = false;
          lastStep.closeable = true;
          this.error = result.message;
          if (result && result.errors && result.errors.length > 0) {
            this.errors = result.errors;
          }
        } else {
          this.success = true;
          lastStep.title = "Success";
          lastStep.completed = true;
          lastStep.closeable = false;
          this.errors = [];
          this.error = null;
          this.transaction = await this.$api.transactions.getTransaction(this.result.entityReference);
          if(this.model.pinPad){
            this.disposeSignalRConnection();
          }
        }
        if (this.customer) {
          this.$store.commit("payment/addLastChargedCustomer", {
            customerID: this.customer.consumerID,
            terminalID: this.model.terminalID
          });
        }

        this.step = lastStepKey;
      }finally{
        this.loading = false;  
      }
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
    }
  },
  async beforeDestroy () {
    await this.disposeSignalRConnection();
  }
};
</script>