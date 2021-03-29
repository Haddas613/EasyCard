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
          :value="(model.settings.vatRate * 100).toFixed(0)"
          :label="$t('VATPercent')"
          :rules="[vr.primitives.required, vr.primitives.inRange(0, 99), vr.primitives.precision(2)]"
          required
          outlined
          disabled
        ></v-text-field>
      </v-col>
      <v-col cols="12" md="6">
        <v-switch
          class="pt-0 mt-2 px-2"
          v-model="model.settings.vatExempt"
          :label="$t('VATExempt')"
          :hint="$t('WhenEnabledVATWillBeIgnored')"
          persistent-hint
        ></v-switch>
      </v-col>
      <v-col cols="12">
        <v-text-field
          v-model="model.settings.defaultItemName"
          :counter="250"
          :rules="[vr.primitives.maxLength(250), vr.primitives.required]"
          :label="$t('DefaultItemName')"
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
        {{$t("SharedApiKey")}}
        <v-divider class="pt-1"></v-divider>
      </v-col>
      <v-col cols="12" md="12">
        <v-btn
          color="success"
          :outlined="!showSharedApiKey"
          small
          @click="showSharedApiKey = !showSharedApiKey;"
        >{{$t("ShowSharedKey")}}</v-btn>
        <v-btn class="mx-1" color="primary" small @click="resetSharedKey()">{{$t("ResetSharedKey")}}</v-btn>
      </v-col>
      <v-col cols="12" md="12">
        <v-text-field
          v-if="showSharedApiKey"
          :value="model.sharedApiKey"
          :label="$t('SharedApiKey')"
          readonly
          outlined
        ></v-text-field>
      </v-col>
    </v-row>
    <v-row>
      <v-col cols="12" class="subtitle-2 black--text pb-3">
        {{$t("PrivateApiKey")}}
        <v-divider class="pt-1"></v-divider>
      </v-col>
      <v-col cols="12" md="12">
        <v-btn color="primary" small @click="resetPrivateKey()">{{$t("ResetPrivateKey")}}</v-btn>
      </v-col>
      <v-col cols="12" md="12">
        <v-text-field
          v-if="privateApiKey"
          :value="privateApiKey"
          :label="$t('PrivateApiKey')"
          readonly
          outlined
        ></v-text-field>
      </v-col>
    </v-row>
    <v-row>
      <v-col cols="12" class="subtitle-2 black--text pb-3">
        {{$t("Invoice")}}
        <v-divider class="pt-1"></v-divider>
      </v-col>
      <v-col cols="12" md="6">
        <v-text-field
          v-model="model.invoiceSettings.defaultInvoiceSubject"
          :counter="128"
          :rules="[vr.primitives.maxLength(128)]"
          :label="$t('DefaultInvoiceSubject')"
          outlined
        ></v-text-field>
      </v-col>
      <v-col cols="12" md="6">
        <v-select
          :items="dictionaries.invoiceTypeEnum"
          item-text="description"
          item-value="code"
          v-model="model.invoiceSettings.defaultInvoiceType"
          :label="$t('DefaultInvoiceType')"
          outlined
        ></v-select>
      </v-col>
      <v-col cols="12" md="6">
        <v-select
          :items="dictionaries.invoiceTypeEnum"
          item-text="description"
          item-value="code"
          v-model="model.invoiceSettings.defaultRefundInvoiceType"
          :label="$t('DefaultRefundInvoiceType')"
          outlined
        ></v-select>
      </v-col>
      <v-col cols="12" md="6">
        <v-select
          :items="dictionaries.invoiceTypeEnum"
          item-text="description"
          item-value="code"
          v-model="model.invoiceSettings.defaultCreditInvoiceType"
          :label="$t('DefaultCreditInvoiceType')"
          outlined
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
      <v-col cols="12" md="7">
        <v-text-field
          v-model="model.paymentRequestSettings.merchantLogo"
          :label="$t('MerchantLogoURL')"
          outlined
        ></v-text-field>
      </v-col>
      <v-col cols="12" md="5">
        <img class="mt-1" v-if="model.paymentRequestSettings.merchantLogo" v-bind:src="model.paymentRequestSettings.merchantLogo" height="48">
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
    <v-row>
      <v-col cols="12" class="subtitle-2 black--text pb-3">
        {{$t("Checkout")}}
        <v-divider class="pt-1"></v-divider>
      </v-col>
      <v-col cols="12" md="7">
        <v-text-field
          v-model="model.checkoutSettings.customCssReference"
          :counter="512"
          :rules="[vr.primitives.maxLength(512)]"
          :label="$t('CustomCSSURL')"
          outlined
          persistent-hint
        ></v-text-field>
      </v-col>
    </v-row>
    <v-row>
      <v-col cols="12" class="subtitle-2 black--text">
        {{$t("CheckoutRedirectUrls")}}
        <v-divider class="pt-1"></v-divider>
      </v-col>
      <v-col cols="12" class="d-flex justify-end py-0">
          <v-btn color="success" small @click="addRedirectUrl()">
            <v-icon left class="body-1">mdi-plus-circle</v-icon>
            {{$t('Add')}}
          </v-btn>
      </v-col>
      <v-col cols="12">
        <ec-list color="ecbg"
            :items="model.checkoutSettings.redirectUrls" 
            v-if="model.checkoutSettings.redirectUrls && model.checkoutSettings.redirectUrls.length > 0"
            stretch 
            dense>
          <template v-slot:left="{ index }">
            <v-col cols="12">
              <v-text-field 
                v-model="model.checkoutSettings.redirectUrls[index]" 
                outlined 
                :label="$t('@RedirectURLNumber').replace('@number', index + 1)"
                :rules="[vr.primitives.required]"></v-text-field>
            </v-col>
          </template>
          <template v-slot:append="{ item }">
            <v-btn class="mb-8" icon @click="deleteRedirectUrl(item)">
              <v-icon class="red--text">mdi-delete</v-icon>
            </v-btn>
          </template>
        </ec-list>
      </v-col>
    </v-row>
    <v-row>
      <v-col cols="12" class="subtitle-2 black--text pb-3">
        {{$t("Transmission")}}
        <v-divider class="pt-1"></v-divider>
      </v-col>
      <v-col cols="12" md="7">
        <v-select
          :items="merchantDictionaries.terminalTransmissionScheduleEnum"
          item-text="description"
          item-value="code"
          v-model="model.settings.transmissionSchedule"
          :label="$t('TransmissionTime')"
          outlined
        ></v-select>
      </v-col>
    </v-row>
  </div>
</template>

<script>
import ValidationRules from "../../helpers/validation-rules";
import appConstants from "../../helpers/app-constants";

export default {
  components: {
    EcList: () => import("../ec/EcList"),
  },
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
      merchantDictionaries: {},
      privateApiKey: null,
      showSharedApiKey: false
    };
  },
  async mounted() {
    this.dictionaries = await this.$api.dictionaries.getTransactionDictionaries();
    this.merchantDictionaries = await this.$api.dictionaries.getMerchantDictionaries();

    if (this.model.billingSettings.billingNotificationsEmails) {
      this.model.billingSettings.billingNotificationsEmailsRaw = this.model.billingSettings.billingNotificationsEmails.join(
        ","
      );
    }

    this.model.settings.vatRatePercent = (
      this.model.settings.vatRate * 100
    ).toFixed(2);

    if (!this.model.invoiceSettings.defaultInvoiceType) {
      this.$set(this.model.invoiceSettings, "defaultInvoiceType",
        appConstants.invoicing.defaultInvoiceType
      );
    }
    if (!this.model.invoiceSettings.defaultRefundInvoiceType) {
      this.$set(this.model.invoiceSettings, "defaultRefundInvoiceType",
        appConstants.invoicing.defaultRefundInvoiceType
      );
    }
    if (!this.model.invoiceSettings.defaultCreditInvoiceType) {
      this.$set(this.model.invoiceSettings, "defaultCreditInvoiceType",
        appConstants.invoicing.defaultCreditInvoiceType
      );
    }
  },
  methods: {
    getData() {
      let result = { ...this.model };
      if (this.model.billingSettings.billingNotificationsEmailsRaw) {
        result.billingSettings.billingNotificationsEmails = [];
        let split = result.billingSettings.billingNotificationsEmailsRaw.split(
          ","
        );
        for (var s of split) {
          let trimmed = s.trim();
          if (trimmed && this.vr.primitives.email(trimmed) === true) {
            result.billingSettings.billingNotificationsEmails.push(trimmed);
          }
        }
        delete result.billingSettings.billingNotificationsEmailsRaw;
      }

      result.settings.vatRate = result.settings.vatRatePercent
        ? (result.settings.vatRatePercent / 100).toFixed(2)
        : 0;
        
      return result;
    },
    async resetPrivateKey() {
      if (!this.model.terminalID) {
        return;
      }
      let operation = await this.$api.terminals.resetPrivateApiKey(
        this.model.terminalID
      );

      if (operation.status === "success") {
        this.privateApiKey = operation.entityReference;
      }
    },
    async resetSharedKey() {
      if (!this.model.terminalID) {
        return;
      }
      let operation = await this.$api.terminals.resetSharedApiKey(
        this.model.terminalID
      );

      if (operation.status === "success") {
        this.showSharedKey = true;
        this.model.sharedApiKey = operation.entityReference;
        this.emitUpdate();
      }
    },
    emitUpdate() {
      this.$emit("update", this.model);
    },
    deleteRedirectUrl(item){
      let idx = this.model.checkoutSettings.redirectUrls.findIndex(i => i == item);
      if(idx === -1) { return ;}

      this.model.checkoutSettings.redirectUrls.splice(idx, 1);
    },
    addRedirectUrl(){
      if(!this.model.checkoutSettings.redirectUrls){
        this.model.checkoutSettings.redirectUrls = [];
      }else{
        let idx = this.model.checkoutSettings.redirectUrls.findIndex(i => !i);
        if(idx !== -1) { return ;}
      }

      this.model.checkoutSettings.redirectUrls.push("");
    }
  }
};
</script>