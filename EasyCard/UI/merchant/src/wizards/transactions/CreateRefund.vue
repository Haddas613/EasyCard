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
      <v-btn v-if="$refs.numpadRef" :disabled="!$refs.numpadRef.model.items.length" color="ecgray" small @click="processToBasket()">
        {{$t("@ItemsQuantity").replace("@quantity", $refs.numpadRef.model.items.length)}}
      </v-btn>
    </template>
    </navbar>
    <v-stepper class="ec-stepper" v-model="step">
      <v-stepper-items>
        <v-stepper-content step="1" class="py-0 px-0">
          <numpad v-if="step === 1" btn-text="Refund" v-on:ok="processAmount($event, true);" ref="numpadRef" :items="model.dealDetails.items"></numpad>
        </v-stepper-content>

        <v-stepper-content step="2" class="py-0 px-0">
          <basket v-if="step === 2" btn-text="Total" v-on:ok="processAmount($event)" v-on:update="updateAmount($event)" :items="model.dealDetails.items"></basket>
        </v-stepper-content>

        <v-stepper-content step="3" class="py-0 px-0">
          <customers-list
            :key="terminal.terminalID"
            :show-previously-charged="true"
            :filter-by-terminal="true"
            v-on:ok="processCustomer($event)"
          ></customers-list>
        </v-stepper-content>

        <v-stepper-content step="4" class="py-0 px-0">
          <credit-card-secure-details
            :key="creditCardRefreshState"
            :data="model"
            v-on:ok="processCreditCard($event)"
            ref="ccSecureDetails"
            btn-text="Refund"
          ></credit-card-secure-details>
        </v-stepper-content>

        <v-stepper-content step="5" class="py-0 px-0">
          <additional-settings-form :key="model.transactionAmount" :data="model" v-on:ok="processAdditionalSettings($event)" :invoice-type="invoiceType"></additional-settings-form>
        </v-stepper-content>

        <v-stepper-content step="6" class="py-0 px-0">
           <wizard-result :errors="errors" v-if="result">
            <template v-if="customer">
              <v-icon class="success--text font-weight-thin" size="170">mdi-check-circle-outline</v-icon>
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
              <div class="pt-4" v-if="result.innerResponse">
                <p v-if="result.innerResponse.status == 'error'">
                  {{result.innerResponse.message}}
                </p>
                <router-link v-else class="primary--text" link
                  :to="{ name: 'Invoice', params: { id: result.innerResponse.entityReference } }"
                >{{$t("GoToInvoice")}}</router-link>
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
import appConstants from "../../helpers/app-constants";

export default {
  components: {
    Navbar: () => import("../../components/wizard/NavBar"),
    Numpad: () => import("../../components/misc/Numpad"),
    Basket: () => import("../../components/misc/Basket"),
    EcMoney: () => import("../../components/ec/EcMoney"),
    CustomersList: () => import("../../components/customers/CustomersList"),
    CreditCardSecureDetails: () => import("../../components/transactions/CreditCardSecureDetails"),
    WizardResult: () => import("../../components/wizard/WizardResult"),
    AdditionalSettingsForm: () => import("../../components/transactions/AdditionalSettingsForm")
  },
  props: ["customerid"],
  data() {
    return {
      customer: null,
      skipCustomerStep: false,
      creditCardRefreshState: null,
      invoiceType: null,
      model: {
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
      result: null
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
        this.model.creditCardSecureDetails.cardOwnerName = this.creditCardRefreshState =
          data.consumerName;
        this.model.creditCardSecureDetails.cardOwnerNationalID =
          data.consumerNationalID;
      }
    }
    this.model.dealDetails.dealDescription = this.terminal.settings.defaultRefundDescription;
    this.invoiceType = this.terminal.invoiceSettings.defaultRefundInvoiceType || appConstants.invoicing.defaultRefundInvoiceType;
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
      this.creditCardRefreshState = null;
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
      this.model.dealDetails.consumerEmail = data.consumerEmail;
      this.model.dealDetails.consumerPhone = data.consumerPhone;
      this.model.dealDetails.consumerAddress = data.consumerAddress;
      this.model.dealDetails.consumerID = data.consumerID;
      if (!this.model.creditCardSecureDetails) {
        this.$set(this.model, "creditCardSecureDetails", {
          cardOwnerName: data.consumerName,
          cardOwnerNationalID: data.consumerNationalID
        });
      } else {
        this.model.creditCardSecureDetails.cardOwnerName = this.creditCardRefreshState =
          data.consumerName;
        this.model.creditCardSecureDetails.cardOwnerNationalID =
          data.consumerNationalID;
      }
      this.model.creditCardToken = null;
      this.$refs.ccSecureDetails.resetToken();
      this.creditCardRefreshState = data.consumerName;
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
      this.model.dealDetails.items = data.items;
    },
    processCreditCard(data) {
      if (data.type === "creditcard") {
        data = data.data;
        this.model.saveCreditCard = data.saveCreditCard || false;
        this.model.creditCardSecureDetails = data;
        this.model.creditCardToken = null;
        if (data.cardReaderInput) {
          this.model.cardPresence = "regular";
        } else {
          this.model.cardPresence = "cardNotPresent";
        }
      } else if (data.type === "token") {
        this.model.creditCardSecureDetails = null;
        this.model.creditCardToken = data.data;
        this.model.saveCreditCard = false;
      }
      this.step++;
    },
    async processAdditionalSettings(data) {
      this.model.dealDetails = data.dealDetails;
      this.model.transactionType = data.transactionType;
      this.model.currency = data.currency;
      this.model.jDealType = data.jDealType;
      this.model.installmentDetails = data.installmentDetails;
      this.model.terminalID = this.terminal.terminalID;
      this.model.invoiceDetails = data.invoiceDetails;
      this.model.issueInvoice = !!this.model.invoiceDetails;

      let result = await this.$api.transactions.refund(this.model);
      this.result = result;

      //assuming current step is one before the last
      let lastStep = this.steps[this.step + 1];

      if (!result || result.status === "error") {
        this.success = false;
        lastStep.title = "Error";
        lastStep.completed = false;
        lastStep.closeable = true;
        if (result && result.errors && result.errors.length > 0) {
          this.errors = result.errors;
        } else {
          this.errors = [{ description: result.message }];
        }
      } else {
        this.success = true;
        lastStep.title = "Success";
        lastStep.completed = true;
        lastStep.closeable = false;
        this.errors = [];
      }
      if (this.customer) {
        this.$store.commit("payment/addLastChargedCustomer", {
          customerID: this.customer.consumerID,
          terminalID: this.model.terminalID
        });
      }

      this.step++;
    }
  }
};
</script>