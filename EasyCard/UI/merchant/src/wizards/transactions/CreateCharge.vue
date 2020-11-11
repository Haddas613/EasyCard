<template>
  <v-flex fluid fill-height>
    <navbar
      v-on:back="goBack()"
      v-on:close="$router.push('/admin/dashboard')"
      v-on:skip="step = step + 1"
      v-on:terminal-changed="terminalChanged()"
      :skippable="steps[step].skippable"
      :closeable="steps[step].closeable"
      :completed="steps[step].completed"
      :canchangeterminal="steps[step].canChangeTerminal"
      :tdmenuitems="threeDotMenuItems"
      :title="navTitle"
    >
      <template v-if="$refs.numpadRef && steps[step].showItemsCount" v-slot:title>
        <v-btn :disabled="!$refs.numpadRef.model.items.length" color="ecgray" small @click="$refs.numpadRef.ok();">
          {{$t("@ItemsQuantity").replace("@quantity", $refs.numpadRef.model.items.length)}}
        </v-btn>
      </template>
    </navbar>
    <v-stepper class="ec-stepper" v-model="step">
      <v-stepper-items>
        <v-stepper-content step="1" class="py-0 px-0">
          <numpad btn-text="Charge" v-on:ok="processAmount($event)" ref="numpadRef"></numpad>
        </v-stepper-content>

        <v-stepper-content step="2" class="py-0 px-0">
          <basket v-if="step === 2" btn-text="Charge" v-on:ok="processAmount($event)" :data="model"></basket>
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
            btn-text="Charge"
          ></credit-card-secure-details>
        </v-stepper-content>

        <v-stepper-content step="5" class="py-0 px-0">
          <additional-settings-form :data="model" :issue-document="true" v-on:ok="processAdditionalSettings($event)"></additional-settings-form>
        </v-stepper-content>

        <v-stepper-content step="6" class="py-0 px-0">
          <transaction-success
            :amount="model.transactionAmount"
            v-if="success"
            :customer="customer"
          ></transaction-success>
          <transaction-error :errors="errors" v-if="!success"></transaction-error>
        </v-stepper-content>
      </v-stepper-items>
    </v-stepper>
  </v-flex>
</template>

<script>
import { mapState } from "vuex";

export default {
  components: {
    Navbar: () => import("../../components/wizard/NavBar"),
    Numpad: () => import("../../components/misc/Numpad"),
    Basket: () => import("../../components/misc/Basket"),
    CustomersList: () => import("../../components/customers/CustomersList"),
    CreditCardSecureDetails: () => import("../../components/transactions/CreditCardSecureDetails"),
    TransactionSuccess: () => import("../../components/transactions/TransactionSuccess"),
    TransactionError: () => import("../../components/transactions/TransactionError"),
    AdditionalSettingsForm: () => import("../../components/transactions/AdditionalSettingsForm")
  },
  props: ["customerid"],
  data() {
    return {
      customer: null,
      skipCustomerStep: false,
      creditCardRefreshState: null,
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
          dealDescription: null
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
      errors: []
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
        this.model.dealDetails.consumerID = data.consumerID;
        this.model.creditCardSecureDetails.cardOwnerName = this.creditCardRefreshState =
          data.consumerName;
        this.model.creditCardSecureDetails.cardOwnerNationalID =
          data.consumerNationalID;
      }
    }
  },
  methods: {
    goBack() {
      if (this.step === 1) this.$router.push("/admin/dashboard");
      else this.step--;
    },
    terminalChanged() {
      this.skipCustomerStep = false;
      this.customer = null;
      this.model.dealDetails.consumerEmail = null;
      this.model.dealDetails.consumerPhone = null;
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
    processAmount(data) {
      this.model.transactionAmount = data.amount;
      this.model.note = data.note;
      this.model.items = data.items;
      // if (this.skipCustomerStep) this.step += 2;
      // else this.step++;

      this.step++;
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

      let result = await this.$api.transactions.processTransaction(this.model);

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

<style lang="scss" scoped>
</style>