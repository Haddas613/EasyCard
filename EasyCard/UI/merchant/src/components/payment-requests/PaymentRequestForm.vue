<template>
  <v-card class="ec-card d-flex flex-column">
    <v-card-text class="py-2">
      <v-form class="ec-form" ref="form" lazy-validation>
        <invoice-details-form ref="invoiceDetails" :data="model.invoiceDetails"></invoice-details-form>

        <installment-details
          ref="instDetails"
          :data="model.installmentDetails"
          v-if="isInstallmentTransaction"
        ></installment-details>

        <v-text-field
          v-model="model.dealDetails.dealReference"
          :counter="50"
          :rules="[vr.primitives.maxLength(50)]"
          :label="$t('DealReference')"
          @keydown.native.space.prevent
          outlined
          required
        ></v-text-field>
        <v-text-field
          v-model="model.dealDetails.consumerEmail"
          :label="$t('ConsumerEmail')"
          :rules="[vr.primitives.required, vr.primitives.email]"
          outlined
          @keydown.native.space.prevent
        ></v-text-field>
        <v-text-field
          v-model="model.dealDetails.consumerPhone"
          :label="$t('ConsumerPhone')"
          :rules="[vr.primitives.maxLength(50)]"
          outlined
          @keydown.native.space.prevent
        ></v-text-field>
        <v-textarea
          v-model="model.dealDetails.dealDescription"
          :counter="1024"
          outlined
          rows="3"
          :rules="[vr.primitives.required,  vr.primitives.maxLength(1024)]"
        >
          <template v-slot:label>
            <div>{{$t('DealDescription')}}</div>
          </template>
        </v-textarea>
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
    InvoiceDetailsForm: () => import("../invoicing/InvoiceDetailsForm"),
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
      messageDialog: false
    };
  },
  computed: {
    isInstallmentTransaction() {
      return false;//TODO
      return (
        this.model.invoiceDetails.invoiceType === "installments" ||
        this.model.invoiceDetails.invoiceType === "credit"
      );
    },
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
      if (!this.model.invoiceDetails.invoiceType) {
        this.$set(this.model.invoiceDetails, 'invoiceType', this.dictionaries.invoiceTypeEnum[0]);
      }
      // this.model.cardPresence = this.dictionaries.cardPresenceEnum[1].code;
    }
  },
  methods: {
    ok() {
      if (!this.$refs.form.validate()) return;

      let result = { ...this.model };
      if (this.$refs.instDetails) {
        result.installmentDetails = this.$refs.instDetails.model;
      }else{
        result.installmentDetails = null;
      }

      result.invoiceDetails = this.$refs.invoiceDetails.getData();
      if(result.invoiceDetails) this.$emit("ok", result);
    }
  }
};
</script>

<style lang="scss" scoped>
</style>