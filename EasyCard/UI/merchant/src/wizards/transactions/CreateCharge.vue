<template>
  <div>
    <navbar
      v-on:back="goBack()"
      v-on:close="$router.push('/admin/dashboard')"
      v-on:skip="step = step + 1"
      :skippable="steps[step].skippable"
      :closeable="steps[step].closeable"
      :title="navTitle"
    ></navbar>
    <v-stepper v-model="step">
      <v-stepper-items>
        <v-stepper-content step="1" class="py-0 px-0">
          <numpad :btntext="$t('Charge')" v-on:ok="amount=$event; step=2"></numpad>
        </v-stepper-content>

        <v-stepper-content step="2" class="py-0 px-0">
          <customers-list :show-previously-charged="true" v-on:ok="model.customer=$event; step=3"></customers-list>
        </v-stepper-content>

        <v-stepper-content step="3" class="py-0 px-0">
          <create-charge-form></create-charge-form>
        </v-stepper-content>
      </v-stepper-items>
    </v-stepper>
  </div>
</template>

<script>
import Navbar from "../../components/wizard/NavBar";
import Numpad from "../../components/misc/Numpad";
import CustomersList from "../../components/customers/CustomersList";
import CreateChargeForm from "../../components/transactions/CreateChargeForm";

export default {
  components: {
    Navbar,
    Numpad,
    CustomersList,
    CreateChargeForm
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
          title: "Charge"
        },
        2: {
          title: "ChooseCustomer",
          skippable: true
        },
        3: {
          title: "Charge",
          closeable: true
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
    }
  }
};
</script>

<style lang="scss" scoped>

</style>