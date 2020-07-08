<template>
  <v-flex fluid fill-height>
    <navbar
      v-on:back="goBack()"
      v-on:close="$router.push('/admin/dashboard')"
      v-on:skip="step = step + 1"
      :skippable="steps[step].skippable"
      :closeable="steps[step].closeable"
      :completed="steps[step].completed"
      :title="navTitle"
    ></navbar>
    <v-overlay :value="loading">
      <v-progress-circular indeterminate size="64"></v-progress-circular>
    </v-overlay>
    {{model.cardPresence}}
    <v-stepper class="ec-stepper" v-model="step">
      <v-stepper-items>
        <v-stepper-content step="1" class="py-0 px-0">
          <terminal-select v-on:ok="processTerminal($event)"></terminal-select>
        </v-stepper-content>

        <v-stepper-content step="2" class="py-0 px-0">
          <numpad :btntext="$t('Charge')" v-on:ok="processAmount($event)"></numpad>
        </v-stepper-content>

        <v-stepper-content step="3" class="py-0 px-0">
          <customers-list :show-previously-charged="true" v-on:ok="model.customer=$event; step=4"></customers-list>
        </v-stepper-content>

        <v-stepper-content step="4" class="py-0 px-0">
          <credit-card-secure-details v-on:ok="processCreditCard($event)"></credit-card-secure-details>
        </v-stepper-content>

        <v-stepper-content step="5" class="py-0 px-0">
           <additional-settings-form :data="model" v-on:ok="processAdditionalSettings($event)"></additional-settings-form>
        </v-stepper-content>

        <v-stepper-content step="6" class="py-0 px-0">
           <transaction-success :amount="model.transactionAmount" v-if="success"></transaction-success>
           <transaction-error :errors="errors" v-if="!success"></transaction-error>
        </v-stepper-content>
      </v-stepper-items>
    </v-stepper>
  </v-flex>
</template>

<script>
import Navbar from "../../components/wizard/NavBar";
import Numpad from "../../components/misc/Numpad";
import CustomersList from "../../components/customers/CustomersList";
import CreateChargeForm from "../../components/transactions/CreateChargeForm";
import CreditCardSecureDetails from "../../components/transactions/CreditCardSecureDetails";
import TransactionSuccess from "../../components/transactions/TransactionSuccess";
import TransactionError from "../../components/transactions/TransactionError";
import TerminalSelect from "../../components/terminals/TerminalSelect";
import AdditionalSettingsForm from "../../components/transactions/AdditionalSettingsForm";

export default {
  components: {
    Navbar,
    Numpad,
    CustomersList,
    CreateChargeForm,
    CreditCardSecureDetails,
    TransactionSuccess,
    TransactionError,
    TerminalSelect,
    AdditionalSettingsForm
  },
  data() {
    return {
      model: {
        terminalID: null,
        transactionType: null,
        jDealType: null,
        currency: null,
        cardPresence: 'cardNotPresent',
        creditCardToken: null,
        creditCardSecureDetails: {
          cardExpiration: {
            year: new Date().getFullYear() - 2000,
            month: new Date().getMonth() + 1
          },
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
          dealDescription: null
        },
        installmentDetails: {
          numberOfPayments: 0,
          initialPaymentAmount: 0,
          installmentPaymentAmount: 0
        }
      },
      step: 1,
      steps: {
        1: {
          title: "Terminal",
        },
        2: {
          title: "Charge"
        },
        3: {
          title: "ChooseCustomer",
          skippable: true
        },
        4: {
          title: "Charge",
        },
        5: {
          title: "AdditionalSettings"
        },
        //Last step may be dynamically altered to represent error if transaction creation is failed.
        6: {
          title: "Success",
          completed: true
        }
      },
      success: true,
      errors: [],
      loading: false
    };
  },
  computed: {
    navTitle() {
      return this.$t(this.steps[this.step].title);
    }
  },
  methods: {
    goBack() {
      if (this.step === 1) this.$router.push("/admin/dashboard");
      else this.step--;
    },
    processAmount(data){
      this.model.transactionAmount = data.amount;
      this.model.note = data.note;
      this.model.items = data.items;
      this.step++;
    },
    processTerminal(data){
      this.model.terminalID = data;
      this.step++;
    },
    processCreditCard(data){
      this.model.creditCardSecureDetails = data;
      if(data.cardReaderInput){
        this.model.cardPresence = 'regular';
      }else{
        this.model.cardPresence = 'cardNotPresent';
      }
      this.step++;
    },
    async processAdditionalSettings(data){
      this.model.dealDetails = data.dealDetails;
      this.model.transactionType = data.transactionType;
      this.model.currency = data.currency;
      this.model.jDealType = data.jDealType;
      // this.model.cardPresence = data.cardPresence;

      let result = { isError: false };
      this.loading = true;

      switch (this.model.jDealType) {
        case "J4":
          result = await this.$api.transactions.createTransaction(this.model);
          break;
        case "J2":
          result = await this.$api.transactions.checkCreditCard(this.model);
          break;
        case "J5":
          result = await this.$api.transactions.blockCreditCard(this.model);
          break;
        default:
          result = false;
          console.error(`unknown JDeal type: ${this.model.jDealType}`);
      }

      this.loading = false;
      //assuming current step is one before the last
      let lastStep = this.steps[this.step + 1];

      if(!result || result.status === "error"){
        this.success = false;
        lastStep.title = "Error";
        lastStep.completed = false;
        lastStep.closeable = true;
        if(result.errors && result.errors.length > 0){
          this.errors = result.errors;
        }else{
          this.errors = [{description: result.message}]
        }
      }else{
        this.success = true;
        lastStep.title = "Success";
        lastStep.completed = true;
        lastStep.closeable = false;
        this.errors = [];
      }
      this.step++;
    }
  }
};
</script>

<style lang="scss" scoped>

</style>