<template>
  <v-card class="ec-card d-flex flex-column">
    <v-card-text class="py-2">
      <v-form class="ec-form" ref="form" lazy-validation>
        <v-select
          :items="dictionaries.jDealTypeEnum"
          item-text="description"
          item-value="code"
          v-model="model.jDealType"
          :label="$t('JDealType')"
          outlined
        ></v-select>
        <v-select
          :items="dictionaries.transactionTypeEnum"
          item-text="description"
          item-value="code"
          v-model="model.transactionType"
          :label="$t('TransactionType')"
          outlined
        ></v-select>

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
        <v-switch v-model="issueDocument" :label="$t('IssueDocument')" class="pt-0 mt-0"></v-switch>
        <div v-if="issueDocument">
          <ec-dialog-invoker v-on:click="invoiceTypeDialog = true" class="py-2">
            <template v-slot:left>
              <div class="font-weight-medium">{{$t("Type")}}</div>
            </template>
            <template v-slot:right>
              <div>{{model.invoiceDetails.invoiceType.description}}</div>
            </template>
            <template v-slot:append>
              <re-icon>mdi-chevron-right</re-icon>
            </template>
          </ec-dialog-invoker>
          <ec-dialog :dialog.sync="invoiceTypeDialog">
            <template v-slot:title>{{$t('InvoiceType')}}</template>
            <template>
              <ec-radio-group
                :data="dictionaries.invoiceTypeEnum"
                labelkey="description"
                valuekey="code"
                return-object
                :model.sync="model.invoiceDetails.invoiceType"
              ></ec-radio-group>
            </template>
          </ec-dialog>
          <v-text-field
            v-model="model.invoiceDetails.invoiceNumber"
            :label="$t('InvoiceNumber')"
            :rules="[vr.primitives.required, vr.primitives.maxLength(50)]"
            outlined
            @keydown.native.space.prevent
            required
          ></v-text-field>
          <v-text-field
            v-model="model.invoiceDetails.invoiceSubject"
            :label="$t('InvoiceSubject')"
            :rules="[vr.primitives.required, vr.primitives.maxLength(255)]"
            required
            outlined
          ></v-text-field>
          <v-textarea
            v-model="model.invoiceDetails.sendCCToRaw"
            outlined
            :hint="$t('SeparateEmailsWithCommas')"
            persistent-hint
            rows="3"
          >
            <template v-slot:label>
              <div>{{$t('AdditionalEmailToCC')}}</div>
            </template>
          </v-textarea>
        </div>
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
    InstallmentDetails: () => import("./InstallmentDetailsForm"),
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
      issueDocument: false,
      invoiceTypeDialog: false,
      messageDialog: false
    };
  },
  computed: {
    isInstallmentTransaction() {
      return (
        this.model.transactionType === "installments" ||
        this.model.transactionType === "credit"
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
      this.model.transactionType = this.dictionaries.transactionTypeEnum[0].code;

      if (!this.model.currency) {
        this.model.currency =
          this.currencyStore.code || this.dictionaries.currencyEnum[0].code;
      }
      if (!this.model.invoiceDetails.invoiceType) {
        this.$set(this.model.invoiceDetails, 'invoiceType', this.dictionaries.invoiceTypeEnum[0]);
      }

      this.model.jDealType = this.dictionaries.jDealTypeEnum[0].code;
      this.model.cardPresence = this.dictionaries.cardPresenceEnum[1].code;
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

      if (this.issueDocument) {
        if (result.invoiceDetails.sendCCToRaw) {
          result.invoiceDetails.sendCCTo = [];
          let split = result.invoiceDetails.sendCCToRaw.split(",");
          for (var s of split) {
            let trimmed = s.trim();
            if (trimmed && this.vr.primitives.email(trimmed) === true) {
              result.invoiceDetails.sendCCTo.push(trimmed);
            }
          }
          delete result.invoiceDetails.sendCCToRaw;
        }
        result.invoiceDetails.invoiceType =
            result.invoiceDetails.invoiceType.code;
      } else {
        result.invoiceDetails = null;
      }

      this.$emit("ok", result);
    }
  }
};
</script>

<style lang="scss" scoped>
</style>