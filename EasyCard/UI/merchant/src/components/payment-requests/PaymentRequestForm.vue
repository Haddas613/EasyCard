<template>
  <v-card class="ec-card d-flex flex-column">
    <v-card-text class="py-2">
      <v-form class="ec-form" ref="form" lazy-validation>

        <v-menu
          ref="dueDateMenu"
          v-model="dueDateMenu"
          :close-on-content-click="false"
          :return-value.sync="model.dueDate"
          offset-y
          min-width="290px"
        >
          <template v-slot:activator="{ on }">
            <v-text-field
              v-model="model.dueDate"
              :label="$t('DueDate')"
              readonly
              outlined
              v-on="on"
            ></v-text-field>
          </template>
          <v-date-picker v-model="model.dueDate" :min="minDate" no-title scrollable color="primary">
            <v-spacer></v-spacer>
            <v-btn text color="primary" @click="$refs.dueDateMenu.save(model.dueDate)">{{$t("Ok")}}</v-btn>
          </v-date-picker>
        </v-menu>
        
        <v-switch 
          v-model="isInstallmentTransaction" 
          :label="$t('InstallmentTransaction')" 
          class="pt-0 mt-0" 
          v-bind:class="{'pb-2': !isInstallmentTransaction}"
          hide-details="true"></v-switch>

        <installment-details
          ref="instDetails"
          :data="model.installmentDetails"
          v-if="isInstallmentTransaction"
          :total-amount="model.paymentRequestAmount"
          hide-title
        ></installment-details>

        <v-text-field
          v-if="!model.dealDetails.consumerID"
          v-model="model.consumerName"
          :label="$t('CustomerName')"
          :rules="[vr.primitives.required, vr.primitives.stringLength(3, 50)]"
          background-color="white"
          type="text"
          outlined
        ></v-text-field>
        <deal-details
          ref="dealDetails"
          :data="model.dealDetails"
          :key="model.dealDetails ? model.dealDetails.consumerEmail : model.dealDetails"
        ></deal-details>

        <invoice-details-fields ref="invoiceDetails" :data="model.invoiceDetails"></invoice-details-fields>
      </v-form>
    </v-card-text>
    <v-card-actions class="px-2">
      <v-btn color="primary" bottom :x-large="true" block @click="ok()">{{$t('Confirm')}}</v-btn>
    </v-card-actions>
  </v-card>
</template>

<script>
import ValidationRules from "../../helpers/validation-rules";
import { mapState } from "vuex";

export default {
  components: {
    InstallmentDetails: () => import("../transactions/InstallmentDetailsForm"),
    DealDetails: () => import("../transactions/DealDetailsFields"),
    InvoiceDetailsFields: () => import("../invoicing/InvoiceDetailsFields"),
    ReIcon: () => import("../../components/misc/ResponsiveIcon"),
    EcDialog: () => import("../../components/ec/EcDialog"),
    EcDialogInvoker: () => import("../../components/ec/EcDialogInvoker"),
    EcRadioGroup: () => import("../../components/inputs/EcRadioGroup")
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
      dictionaries: {},
      model: { 
        ...this.data, 
        invoiceDetails: this.data.invoiceDetails || {} 
      },
      vr: ValidationRules,
      messageDialog: false,
      dueDateMenu: false,
      minDate: new Date().toISOString(),
      isInstallmentTransaction: false,
    };
  },
  computed: {
    ...mapState({
      currencyStore: state => state.settings.currency
    })
  },
  async mounted() {
    let dictionaries = await this.$api.dictionaries.getTransactionDictionaries();
    if (dictionaries) {
      this.dictionaries = dictionaries;

      if (!this.model.currency) {
        this.model.currency =
          this.currencyStore.code || this.dictionaries.currencyEnum[0].code;
      }
      // this.model.cardPresence = this.dictionaries.cardPresenceEnum[1].code;
    }
  },
  methods: {
    ok() {
      if (!this.$refs.form.validate()) return;
      
      let result = { ...this.model };
      if (this.isInstallmentTransaction) {
        result.installmentDetails = this.$refs.instDetails.getData();
      }else{
        result.installmentDetails = null;
      }

      result.invoiceDetails = this.$refs.invoiceDetails.getData();
      result.dealDetails = this.$refs.dealDetails.getData();
      if(result.invoiceDetails) this.$emit("ok", result);
    }
  }
};
</script>

<style lang="scss" scoped>
</style>