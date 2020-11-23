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
    <template v-if="steps[step].showItemsCount" v-slot:title>
      <v-btn v-if="$refs.numpadRef" :disabled="!$refs.numpadRef.model.items.length" color="ecgray" small @click="processToBasket()">
        {{$t("@ItemsQuantity").replace("@quantity", $refs.numpadRef.model.items.length)}}
      </v-btn>
    </template>
    </navbar>
    <v-stepper class="ec-stepper" v-model="step">
      <v-stepper-items>
        <v-stepper-content step="1" class="py-0 px-0">
          <numpad v-if="step === 1" btn-text="Charge" v-on:ok="processAmount($event, true);" ref="numpadRef" :items="model.dealDetails.items"></numpad>
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
          <payment-request-form :key="model.paymentRequestAmount" :data="model" v-on:ok="processPaymentRequest($event)"></payment-request-form>
        </v-stepper-content>

        <v-stepper-content step="5" class="py-0 px-0">
          <wizard-result :errors="errors">
            <template v-if="customer">
              <v-icon class="success--text font-weight-thin" size="170">mdi-check-circle-outline</v-icon>
              <p>{{customer.consumerName}}</p>
              <div class="pt-5">
                <p>{{$t("PaymentRequestSentTo")}}</p>
                <p>{{customer.consumerEmail}}</p>
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

export default {
  components: {
    Navbar: () => import("../../components/wizard/NavBar"),
    Numpad: () => import("../../components/misc/Numpad"),
    Basket: () => import("../../components/misc/Basket"),
    CustomersList: () => import("../../components/customers/CustomersList"),
    WizardResult: () => import("../../components/wizard/WizardResult"),
    PaymentRequestForm: () =>
      import("../../components/payment-requests/PaymentRequestForm")
  },
  props: ["customerid"],
  data() {
    return {
      customer: null,
      skipCustomerStep: false,
      model: {
        terminalID: null,
        currency: null,
        invoiceType: null,
        paymentRequestAmount: 0.0,
        dueDate: null,
        dealDetails: {
          dealReference: null,
          consumerEmail: null,
          consumerPhone: null,
          consumerID: null,
          dealDescription: null
        },
        invoiceDetails: {
          invoiceNumber: null,
          invoiceType: null,
          invoiceSubject: null
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
          title: "PaymentRequestSettings"
        },
        //Last step may be dynamically altered to represent error if transaction creation has failed.
        5: {
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
    },
    processCustomer(data) {
      this.skipCustomerStep = false;
      if (this.customer && data.consumerID === this.customer.consumerID) {
        return this.step++;
      }
      this.customer = data;
      this.model.dealDetails.consumerEmail = data.consumerEmail;
      this.model.dealDetails.consumerPhone = data.consumerPhone;
      this.model.dealDetails.consumerID = data.consumerID;
      this.step++;
      console.log(this.model)
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
      this.model.paymentRequestAmount = data.totalAmount;
      this.model.netTotal = data.netTotal;
      this.model.vatTotal = data.vatTotal;
      this.model.vatRate = data.vatRate;
      this.model.note = data.note;
      this.model.dealDetails.items = data.items;
    },
    async processPaymentRequest(data) {
      this.model.dealDetails = data.dealDetails;
      this.model.currency = data.currency;
      this.model.installmentDetails = data.installmentDetails;
      this.model.invoiceDetails = data.invoiceDetails;
      this.model.terminalID = this.terminal.terminalID;
      this.model.dueDate = data.dueDate;

      let result = await this.$api.paymentRequests.createPaymentRequest(
        this.model
      );

      //assuming current step is one before the last
      let lastStep = this.steps[this.step + 1];

      if (!result || result.status === "error") {
        lastStep.title = "Error";
        lastStep.completed = false;
        lastStep.closeable = true;
        if (result && result.errors && result.errors.length > 0) {
          this.errors = result.errors;
        } else {
          this.errors = [{ description: result.message }];
        }
      } else {
        this.$store.commit("payment/addLastChargedCustomer", {
          customerID: this.customer.consumerID,
          terminalID: this.model.terminalID
        });
        return this.$router.push({
          name: "PaymentRequest",
          params: { id: result.entityReference }
        });
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