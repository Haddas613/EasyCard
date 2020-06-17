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
    <v-stepper class="ec-stepper" v-model="step">
      <v-stepper-items>
        <v-stepper-content step="1" class="py-0 px-0">
          <terminal-select v-on:ok="model.terminalID = $event; step++"></terminal-select>
        </v-stepper-content>

        <v-stepper-content step="2" class="py-0 px-0">
          <numpad :btntext="$t('Charge')" v-on:ok="processAmount($event)"></numpad>
        </v-stepper-content>

        <v-stepper-content step="3" class="py-0 px-0">
          <customers-list :show-previously-charged="true" v-on:ok="model.customer=$event; step=4"></customers-list>
        </v-stepper-content>

        <v-stepper-content step="4" class="py-0 px-0">
          <credit-card-secure-details v-on:ok="step=5"></credit-card-secure-details>
        </v-stepper-content>

        <v-stepper-content step="5" class="py-0 px-0">
           <additional-settings-form></additional-settings-form>
        </v-stepper-content>

        <v-stepper-content step="6" class="py-0 px-0">
           <transaction-success :amount="model.amount"></transaction-success>
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
    TerminalSelect,
    AdditionalSettingsForm
  },
  data() {
    return {
      model:{
        amount: 0.0,
        customer: null
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
          skippable: true
        },
        5: {
          title: "AdditionalSettings",
          skippable: true
        },
        6: {
          title: "Success",
          completed: true
        }
      }
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
      this.model.amount = data.amount;
      this.model.note = data.note;
      this.model.items = data.items;
      this.step++;
    }
  }
};
</script>

<style lang="scss" scoped>

</style>