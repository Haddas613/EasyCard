<template>
  <div>
    <v-row no-gutters>
      <v-col md="4" cols="12">
        <v-text-field
          v-model="model.label"
          :counter="50"
          :rules="[vr.primitives.maxLength(50)]"
          :label="$t('Label')"
          outlined
          required
        ></v-text-field>
      </v-col>
      <v-col md="4" cols="12">
        <v-text-field
          :value="$options.filters.ecdate(model.created ,'LLLL')"
          v-bind:class="{'px-1' : $vuetify.breakpoint.mdAndUp}"
          disabled
          :label="$t('Created')"
          outlined
          required
        ></v-text-field>
      </v-col>
      <v-col md="4" cols="12">
        <v-select
          :items="merchantDictionaries.terminalStatusEnum"
          item-text="description"
          item-value="code"
          v-model="model.status"
          :label="$t('Status')"
          outlined
          disabled
        ></v-select>
      </v-col>
    </v-row>
    <v-row no-gutters>
      <v-col cols="12" class="subtitle-2 black--text pb-3">
        {{$t("General")}}
        <v-divider class="pt-1"></v-divider>
      </v-col>
      <v-col cols="12" md="6">
        <v-text-field
          v-model="model.settings.defaultItemName"
          :counter="250"
          :rules="[vr.primitives.maxLength(250), vr.primitives.required]"
          :label="$t('DefaultItemName')"
          outlined
        ></v-text-field>
      </v-col>
      <v-col cols="12" md="6">
        <v-text-field
          v-model.number="model.settings.vatRatePercent"
          :label="$t('VATPercent')"
          :rules="[vr.primitives.required, vr.primitives.inRange(0, 99), vr.primitives.precision(2)]"
          required
          class="px-1"
          outlined
        ></v-text-field>
      </v-col>
      <v-col cols="12">
        <v-text-field
          v-model="model.settings.defaultChargeDescription"
          :counter="250"
          :rules="[vr.primitives.maxLength(250)]"
          :label="$t('DefaultChargeDescription')"
          outlined
        ></v-text-field>
      </v-col>
      <v-col cols="12">
        <v-text-field
          v-model="model.settings.defaultRefundDescription"
          :counter="250"
          :rules="[vr.primitives.maxLength(250)]"
          :label="$t('DefaultRefundDescription')"
          outlined
        ></v-text-field>
      </v-col>

      <v-col md="6" cols="12">
        <v-switch
          class="pt-0"
          v-model="model.settings.j5Allowed"
          :label="$t('J5Allowed')"
          disabled
          hide-details
        ></v-switch>
        <v-switch v-model="model.settings.j2Allowed" :label="$t('J2Allowed')" hide-details disabled></v-switch>
        <v-switch
          v-model="model.settings.enableCancellationOfUntransmittedTransactions"
          :label="$t('EnableCancellationOfUntransmittedTransactions')"
          hide-details
          disabled
        ></v-switch>
      </v-col>
      <v-col md="6" cols="12">
        <v-switch
          class="pt-0"
          v-model="model.settings.cvvRequired"
          :label="$t('CvvRequired')"
          hide-details
          disabled
        ></v-switch>
        <v-switch
          v-model="model.settings.nationalIDRequired"
          :label="$t('NationalIDRequired')"
          hide-details
          disabled
        ></v-switch>
      </v-col>
      <v-col cols="12">
        <v-spacer class="py-4"></v-spacer>
      </v-col>
      <v-col md="6" cols="12">
        <v-text-field
          v-model="model.settings.minInstallments"
          :label="$t('MinInstallments')"
          type="number"
          outlined
        ></v-text-field>
      </v-col>
      <v-col md="6" cols="12">
        <v-text-field
          v-model="model.settings.maxInstallments"
          v-bind:class="{'px-1' : $vuetify.breakpoint.mdAndUp}"
          :label="$t('MaxInstallments')"
          type="number"
          :rules="[vr.primitives.required]"
          outlined
        ></v-text-field>
      </v-col>
      <v-col md="6" cols="12">
        <v-text-field
          v-model="model.settings.minCreditInstallments"
          :label="$t('MinCreditInstallments')"
          type="number"
          outlined
        ></v-text-field>
      </v-col>
      <v-col md="6" cols="12">
        <v-text-field
          v-model="model.settings.maxCreditInstallments"
          v-bind:class="{'px-1' : $vuetify.breakpoint.mdAndUp}"
          :label="$t('MaxCreditInstallments')"
          type="number"
          :rules="[vr.primitives.required]"
          outlined
        ></v-text-field>
      </v-col>
    </v-row>
    <v-row>
      <v-col cols="12" class="subtitle-2 black--text pb-3">
        {{$t("Invoice")}}
        <v-divider class="pt-1"></v-divider>
      </v-col>
      <v-col cols="12" md="7">
        <v-text-field
          v-model="model.invoiceSettings.defaultInvoiceSubject"
          :counter="128"
          :rules="[vr.primitives.maxLength(128)]"
          :label="$t('DefaultInvoiceSubject')"
          outlined
        ></v-text-field>
      </v-col>
      <v-col cols="12" md="5">
        <v-select
          :items="dictionaries.invoiceTypeEnum"
          item-text="description"
          item-value="code"
          v-model="model.invoiceSettings.defaultInvoiceType"
          :label="$t('DefaultInvoiceType')"
          outlined
          clearable
        ></v-select>
      </v-col>
    </v-row>
    <v-row>
      <v-col cols="12" class="subtitle-2 black--text pb-3">
        {{$t("PaymentRequest")}}
        <v-divider class="pt-1"></v-divider>
      </v-col>
      <v-col cols="12" md="7">
        <v-text-field
          v-model="model.paymentRequestSettings.defaultRequestSubject"
          :counter="128"
          :rules="[vr.primitives.maxLength(128)]"
          :label="$t('DefaultRequestSubject')"
          outlined
        ></v-text-field>
      </v-col>
      <v-col cols="12" md="5">
        <v-text-field
          v-model="model.paymentRequestSettings.fromAddress"
          :rules="[vr.primitives.required, vr.primitives.email]"
          :label="$t('FromAddress')"
          outlined
        ></v-text-field>
      </v-col>
    </v-row>
    <v-row>
      <v-col cols="12" class="subtitle-2 black--text pb-3">
        {{$t("Billing")}}
        <v-divider class="pt-1"></v-divider>
      </v-col>
      <v-col cols="12" md="7">
        <v-textarea
          v-model="model.billingSettings.billingNotificationsEmailsRaw"
          :counter="512"
          :rules="[vr.primitives.maxLength(512)]"
          :label="$t('NotificationEmails')"
          outlined
          :hint="$t('SeparateEmailsWithCommas')"
          persistent-hint
          rows="3"
        ></v-textarea>
      </v-col>
      <v-col cols="12" md="5">
        <v-switch
          class="pt-0"
          v-model="model.billingSettings.createRecurrentPaymentsAutomatically"
          :label="$t('CreateRecurrentPaymentsAutomatically')"
          hide-details
        ></v-switch>
      </v-col>
    </v-row>
  </div>
</template>

<script>
import ValidationRules from "../../helpers/validation-rules";

export default {
  props: {
    data: {
      type: Object,
      default: () => null,
      required: true
    }
  },
  data() {
    return {
      vr: ValidationRules,
      model: { ...this.data },
      dictionaries: {},
      merchantDictionaries: {}
    };
  },
  async mounted() {
    this.dictionaries = await this.$api.dictionaries.getTransactionDictionaries();
    this.merchantDictionaries = await this.$api.dictionaries.getMerchantDictionaries();

    if(this.model.billingSettings.billingNotificationsEmails){
      this.model.billingSettings.billingNotificationsEmailsRaw = 
        this.model.billingSettings.billingNotificationsEmails.join(",");
    }

    this.model.settings.vatRatePercent = this.model.settings.vatRate * 100;

    if (!this.model.invoiceSettings.defaultInvoiceType) {
      this.$set(
        this.model.invoiceSettings,
        "invoiceType",
        this.dictionaries.invoiceTypeEnum.find(
          i => i.code == this.terminalStore.invoiceSettings.defaultInvoiceType
        )
      );
    } else if (
      typeof this.model.invoiceSettings.defaultInvoiceType === "string"
    ) {
      this.$set(
        this.model.invoiceSettings,
        "invoiceType",
        this.dictionaries.invoiceTypeEnum.find(
          i => i.code == this.model.invoiceSettings.defaultInvoiceType
        )
      );
    }
  },
  methods: {
    getData() {
      let result = { ...this.model };
      if(this.model.billingSettings.billingNotificationsEmailsRaw){
        result.billingSettings.billingNotificationsEmails = [];
        let split = result.billingSettings.billingNotificationsEmailsRaw.split(",");
        for (var s of split) {
          let trimmed = s.trim();
          if (trimmed && this.vr.primitives.email(trimmed) === true) {
            result.billingSettings.billingNotificationsEmails.push(trimmed);
          }
        }
        delete result.billingSettings.billingNotificationsEmailsRaw;
      }

      result.settings.vatRate = result.settings.vatRatePercent ? (result.settings.vatRatePercent / 100).toFixed(2) : 0;
      return result;
    },
  },
};
</script>