<template>
  <div>
    <v-card outlined class="mb-2">
      <v-card-title class="pb-0 mb-0 subtitle-2 black--text">{{$t("Terminal")}}</v-card-title>
      <v-card-text>
        <v-row no-gutters>
          <v-col cols="12">
            <v-divider class="py-2"></v-divider>
          </v-col>
          <v-col md="4" cols="12">
            <v-text-field
              v-model="model.label"
              :counter="50"
              :rules="[vr.primitives.required, vr.primitives.maxLength(50)]"
              :label="$t('Label')"
              required
            ></v-text-field>
          </v-col>
          <v-col md="4" cols="12">
            <v-text-field
              :value="$options.filters.ecdate(model.created ,'LLLL')"
              v-bind:class="{'px-1' : $vuetify.breakpoint.mdAndUp}"
              disabled
              :label="$t('Created')"
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
              disabled
            ></v-select>
          </v-col>
        </v-row>
      </v-card-text>
    </v-card>

    <v-card outlined class="mb-2">
      <v-card-title class="pb-0 mb-0 subtitle-2 black--text">{{$t("General")}}</v-card-title>
      <v-card-text>
        <v-row no-gutters>
          <v-col cols="12">
            <v-divider class="py-2"></v-divider>
          </v-col>
          <v-col cols="12" md="4">
            <v-text-field
              v-model="model.settings.defaultItemName"
              :counter="250"
              :rules="[vr.primitives.maxLength(250), vr.primitives.required]"
              :label="$t('DefaultItemName')"
            ></v-text-field>
          </v-col>
          <v-col cols="12" md="4" class="px-1">
            <v-text-field
              v-model="model.settings.defaultChargeDescription"
              :counter="250"
              :rules="[vr.primitives.maxLength(250)]"
              :label="$t('DefaultChargeDescription')"
            ></v-text-field>
          </v-col>
          <v-col cols="12" md="4">
            <v-text-field
              v-model="model.settings.defaultRefundDescription"
              :counter="250"
              :rules="[vr.primitives.maxLength(250)]"
              :label="$t('DefaultRefundDescription')"
            ></v-text-field>
          </v-col>
          <v-col cols="12" md="6">
            <v-text-field
              class="w99"
              :value="(model.settings.vatRate * 100).toFixed(0)"
              :label="$t('VATPercent')"
              :rules="[vr.primitives.required, vr.primitives.inRange(0, 99), vr.primitives.precision(0)]"
              required
              disabled
              hide-details
            ></v-text-field>
          </v-col>
          <v-col cols="12" md="6">
            <v-switch
              class="pt-0 mt-0 pb-2"
              v-model="model.settings.vatExempt"
              :label="$t('VATExempt')"
              :hint="$t('WhenEnabledVATWillBeIgnored')"
              persistent-hint
            ></v-switch>
          </v-col>
          <v-col md="6" cols="12">
            <v-switch
              class="pt-0"
              v-model="model.settings.j5Allowed"
              :label="$t('J5Allowed')"
              hide-details
            ></v-switch>
            <v-switch v-model="model.settings.j2Allowed" :label="$t('J2Allowed')" hide-details></v-switch>
            <v-switch
              v-model="model.settings.enableCancellationOfUntransmittedTransactions"
              :label="$t('EnableCancellationOfUntransmittedTransactions')"
              hide-details
            ></v-switch>
            <v-switch
              v-model="model.settings.useQuickChargeByDefault"
              :label="$t('UseQuickChargeByDefault')"
              hide-details
            ></v-switch>
            <v-switch
              v-model="model.settings.doNotCreateSaveTokenInitialDeal"
              :label="$t('DoNotCreateSaveTokenInitialDeal')"
              hide-details
            ></v-switch>
          </v-col>
          <v-col md="6" cols="12">
            <v-switch
              class="pt-0"
              v-model="model.settings.cvvRequired"
              :label="$t('CvvRequired')"
              hide-details
            ></v-switch>
            <v-switch
              v-model="model.settings.nationalIDRequired"
              :label="$t('NationalIDRequired')"
              hide-details
            ></v-switch>
            <v-switch
              v-model="model.settings.sendTransactionSlipEmailToMerchant"
              :label="$t('SendTransactionSlipEmailToMerchant')"
              hide-details
            ></v-switch>
          </v-col>
          <v-col cols="12">
            <v-spacer class="py-4"></v-spacer>
          </v-col>
          <v-col md="3" cols="12">
            <v-text-field
              v-model="model.settings.minInstallments"
              :label="$t('MinInstallments')"
              :rules="[vr.primitives.required, vr.primitives.lessThan(36, true)]"
              type="number"
            ></v-text-field>
          </v-col>
          <v-col md="3" cols="12">
            <v-text-field
              v-model="model.settings.maxInstallments"
              v-bind:class="{'px-1' : $vuetify.breakpoint.mdAndUp}"
              :label="$t('MaxInstallments')"
              type="number"
              :rules="[vr.primitives.required, vr.primitives.lessThan(36, true)]"
            ></v-text-field>
          </v-col>
          <v-col md="3" cols="12">
            <v-text-field
              v-model="model.settings.minCreditInstallments"
              :label="$t('MinCreditInstallments')"
              type="number"
              :rules="[vr.primitives.required, vr.primitives.lessThan(36, true)]"
            ></v-text-field>
          </v-col>
          <v-col md="3" cols="12">
            <v-text-field
              v-model="model.settings.maxCreditInstallments"
              v-bind:class="{'px-1' : $vuetify.breakpoint.mdAndUp}"
              :label="$t('MaxCreditInstallments')"
              type="number"
              :rules="[vr.primitives.required, vr.primitives.lessThan(36, true)]"
            ></v-text-field>
          </v-col>
        </v-row>
      </v-card-text>
    </v-card>

    <v-card outlined class="mb-2">
      <v-card-title class="pb-0 mb-0 subtitle-2 black--text">{{$t("Invoice")}}</v-card-title>
      <v-card-text>
        <v-row no-gutters>
          <v-col cols="12">
            <v-divider class="py-2"></v-divider>
          </v-col>
          <v-col cols="12" md="5">
            <v-text-field
              v-model="model.invoiceSettings.defaultInvoiceSubject"
              :counter="128"
              :rules="[vr.primitives.maxLength(128)]"
              :label="$t('DefaultInvoiceSubject')"
            ></v-text-field>
          </v-col>
          <v-spacer></v-spacer>
          <v-col cols="12" md="5">
            <v-select
              :items="dictionaries.invoiceTypeEnum"
              item-text="description"
              item-value="code"
              v-model="model.invoiceSettings.defaultInvoiceType"
              :label="$t('DefaultInvoiceType')"
              clearable
            ></v-select>
          </v-col>
          <v-col cols="12" md="5">
            <v-select
              :items="dictionaries.invoiceTypeEnum"
              item-text="description"
              item-value="code"
              v-model="model.invoiceSettings.defaultRefundInvoiceType"
              :label="$t('DefaultRefundInvoiceType')"
              clearable
            ></v-select>
          </v-col>
          <v-spacer></v-spacer>
          <v-col cols="12" md="5">
            <v-select
              :items="dictionaries.invoiceTypeEnum"
              item-text="description"
              item-value="code"
              v-model="model.invoiceSettings.defaultCreditInvoiceType"
              :label="$t('DefaultCreditInvoiceType')"
              clearable
            ></v-select>
          </v-col>
        </v-row>
      </v-card-text>
    </v-card>

    <v-card outlined class="mb-2">
      <v-card-title class="pb-0 mb-0 subtitle-2 black--text">{{$t("PaymentRequest")}}</v-card-title>
      <v-card-text>
        <v-row no-gutters>
          <v-col cols="12">
            <v-divider class="py-2"></v-divider>
          </v-col>
          <v-col cols="12" md="6">
            <v-text-field
              v-model="model.paymentRequestSettings.defaultRequestSubject"
              :counter="128"
              :rules="[vr.primitives.maxLength(128)]"
              :label="$t('DefaultRequestSubject')"
            ></v-text-field>
          </v-col>
          <v-col cols="12" md="6">
            <v-text-field
              v-bind:class="{'px-4' : $vuetify.breakpoint.mdAndUp}"
              v-model="model.paymentRequestSettings.defaultRefundRequestSubject"
              :counter="128"
              :rules="[vr.primitives.maxLength(128)]"
              :label="$t('DefaultRefundRequestSubject')"
            ></v-text-field>
          </v-col>
          <v-col cols="12" md="6">
            <v-text-field
              v-model="model.paymentRequestSettings.fromAddress"
              :rules="[vr.primitives.required, vr.primitives.email]"
              :label="$t('FromAddress')"
            ></v-text-field>
          </v-col>
          <v-col cols="12" md="6" v-if="$featureEnabled(model, appConstants.terminal.features.SmsNotification)">
            <v-text-field
              v-bind:class="{'px-4' : $vuetify.breakpoint.mdAndUp}"
              v-model="model.paymentRequestSettings.fromPhoneNumber"
              :rules="[vr.primitives.numeric()]"
              :label="$t('FromPhoneNumber')"
            ></v-text-field>
          </v-col>
          <v-col cols="12" md="5">
            <terminal-merchant-logo-input v-model="model"></terminal-merchant-logo-input>
          </v-col>
          <v-spacer></v-spacer>
          <v-col cols="12" md="5">
            <div class="d-flex justify-items-center" v-if="model.paymentRequestSettings.merchantLogo">
              <img class="mt-1"  v-bind:src="getBlobUrl(model.paymentRequestSettings.merchantLogo)" height="48">
              <v-btn class="mt-2" icon color="error" @click="deleteMerchantLogo()">
                <v-icon>mdi-delete</v-icon>
              </v-btn>
            </div>
          </v-col>
        </v-row>
      </v-card-text>
    </v-card>

    <v-card outlined class="mb-2" v-if="$featureEnabled(model, appConstants.terminal.features.Billing)">
      <v-card-title class="pb-0 mb-0 subtitle-2 black--text">{{$t("Billing")}}</v-card-title>
      <v-card-text>
        <v-row no-gutters>
          <v-col cols="12">
            <v-divider class="py-2"></v-divider>
          </v-col>
          <v-col cols="12" md="7">
            <v-textarea
              v-model="model.billingSettings.billingNotificationsEmailsRaw"
              :counter="512"
              :rules="[vr.primitives.maxLength(512)]"
              :label="$t('NotificationEmails')"
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
      </v-card-text>
    </v-card>
    <v-card outlined class="mb-2" v-if="$featureEnabled(model, appConstants.terminal.features.Checkout)">
      <v-card-title class="pb-0 mb-0 subtitle-2 black--text">{{$t("SharedApiKey")}}</v-card-title>
      <v-card-text>
        <v-row no-gutters>
          <v-col cols="12">
            <v-btn
              color="success"
              :outlined="!showSharedApiKey"
              small
              @click="showSharedApiKey = !showSharedApiKey;"
            >{{$t("ShowSharedKey")}}</v-btn>
            <v-btn class="mx-1" color="primary" small @click="resetSharedKey()">{{$t("ResetSharedKey")}}</v-btn>
          </v-col>
          <v-col cols="12" md="12" class="pt-4">
            <v-text-field
              v-if="showSharedApiKey"
              :value="model.sharedApiKey"
              :label="$t('SharedApiKey')"
              readonly
            ></v-text-field>
          </v-col>
        </v-row>
      </v-card-text>
    </v-card>

    <v-card outlined class="mb-2" v-if="$featureEnabled(model, appConstants.terminal.features.Checkout)">
      <v-card-title class="pb-0 mb-0 subtitle-2 black--text">{{$t("PrivateApiKey")}}</v-card-title>
      <v-card-text>
        <v-row no-gutters>
          <v-col cols="12" md="12">
            <v-btn color="primary" small @click="resetPrivateKey()">{{$t("ResetPrivateKey")}}</v-btn>
            <v-btn color="success" class="mx-1" :disabled="privateApiKey" small @click="showPrivateKey()">{{$t("ShowPrivateKey")}}</v-btn>
          </v-col>
          <v-col cols="12" md="12" class="mt-4">
            <v-text-field
              v-if="privateApiKey"
              :value="privateApiKey"
              :label="$t('PrivateApiKey')"
              readonly
            ></v-text-field>
          </v-col>
        </v-row>
      </v-card-text>
    </v-card>

    <v-card outlined class="mb-2" v-if="$featureEnabled(model, appConstants.terminal.features.Checkout)">
      <v-card-title class="pb-0 mb-0 subtitle-2 black--text">{{$t("Checkout")}}</v-card-title>
      <v-card-text>
        <v-row no-gutters>
          <v-col cols="12">
            <v-divider class="py-2"></v-divider>
          </v-col>
          <v-col cols="12" md="5">
            <terminal-merchant-style-input v-model="model"></terminal-merchant-style-input>
          </v-col>
          <v-spacer></v-spacer>
          <v-col cols="12" md="5">
            <div v-if="model.checkoutSettings.customCssReference" class="px-4 mt-5">
              <a class="body-1" v-bind:href="getBlobUrl(model.checkoutSettings.customCssReference)">
                {{$t("LinkToCSS")}}
              </a>
              <v-btn class="pb-1" icon color="error" @click="deleteCustomCSS()">
                <v-icon>mdi-delete</v-icon>
              </v-btn>
            </div>
          </v-col>
          <v-col cols="12" md="5">
            <v-switch
              class="pt-0"
              v-model="model.checkoutSettings.issueInvoice"
              :label="$t('IssueInvoice')"
              hide-details
            ></v-switch>
          </v-col>
        </v-row>
      </v-card-text>
    </v-card>

    <v-card outlined class="mb-2" v-if="$featureEnabled(model, appConstants.terminal.features.Checkout)">
      <v-card-title class="pb-0 mb-0 subtitle-2 black--text">{{$t("CheckoutRedirectUrls")}}</v-card-title>
      <v-card-text>
        <v-row no-gutters>
          <v-col cols="12">
            <v-divider class="py-2"></v-divider>
          </v-col>
          <v-col cols="12" md="6" class="d-flex justify-start">
            <v-switch
              class="pt-0"
              v-model="model.checkoutSettings.legacyRedirectResponse"
              :label="$t('ExtendedResponseForCheckoutTransaction')"
              hide-details
            ></v-switch>
          </v-col>
          <v-col cols="12" md="6" class="d-flex justify-end">
            <v-btn color="success" small @click="addRedirectUrl()">
              <v-icon left class="body-1">mdi-plus-circle</v-icon>
              {{$t('Add')}}
            </v-btn>
          </v-col>
          <v-col cols="12" class="pt-6">
            <ec-list
              :items="model.checkoutSettings.redirectUrls"
              v-if="model.checkoutSettings.redirectUrls && model.checkoutSettings.redirectUrls.length > 0"
              stretch
              dense
            >
              <template v-slot:left="{ index }">
                <v-col cols="12">
                  <v-text-field
                    v-model="model.checkoutSettings.redirectUrls[index]"
                    :label="$t('@RedirectURLNumber').replace('@number', index + 1)"
                    :rules="[vr.primitives.required]"
                  ></v-text-field>
                </v-col>
              </template>
              <template v-slot:append="{ item }">
                <v-btn class="mt-2" icon @click="deleteRedirectUrl(item)">
                  <v-icon class="red--text">mdi-delete</v-icon>
                </v-btn>
              </template>
            </ec-list>
          </v-col>
        </v-row>
      </v-card-text>
    </v-card>

    <v-card outlined class="mb-2">
      <v-card-title class="pb-0 mb-0 subtitle-2 black--text">{{$t("Transmission")}}</v-card-title>
      <v-card-text>
        <v-row no-gutters>
          <v-col cols="12">
            <v-divider class="py-2"></v-divider>
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
      </v-card-text>
    </v-card>

    <v-card outlined class="mb-2" v-if="$featureEnabled(model, appConstants.terminal.features.Billing)">
      <v-card-text>
        <v-row>
          <v-col cols="12" class="subtitle-2 black--text pb-3">
            {{$t("BankAccountDetails")}}
            <v-divider class="pt-1"></v-divider>
          </v-col>
          <v-col cols="12" md="4">
            <bank-select 
              v-model="model.bankDetails.bank"
              v-bind:class="{'px-1' : $vuetify.breakpoint.mdAndUp}"
              required
            ></bank-select>
          </v-col>
          <v-col cols="12" md="4">
            <v-text-field
              :label="$t('BankBranch')"
              :counter="7"
              outlined
              v-model="model.bankDetails.bankBranch"
              max="7"
              type="number"
              :rules="[vr.primitives.required, vr.primitives.numeric()]"
              v-bind:class="{'px-1' : $vuetify.breakpoint.mdAndUp}"
            ></v-text-field>
          </v-col>
          <v-col cols="12" md="4">
            <v-text-field
              :label="$t('BankAccount')"
              :counter="12"
              outlined
              v-model="model.bankDetails.bankAccount"
              max="12"
              type="number"
              :rules="[vr.primitives.required, vr.primitives.numeric(), vr.primitives.stringLength(6, 12)]"
              v-bind:class="{'px-1' : $vuetify.breakpoint.mdAndUp}"
            ></v-text-field>
          </v-col>
        </v-row>
      </v-card-text>
    </v-card>
  </div>
</template>

<script>
import ValidationRules from "../../helpers/validation-rules";
import appConstants from "../../helpers/app-constants";

export default {
  components: {
    EcList: () => import("../ec/EcList"),
    TerminalMerchantLogoInput: () => import("./TerminalMerchantLogoInput"),
    TerminalMerchantStyleInput: () => import("./TerminalMerchantStyleInput"),
    BankSelect: () => import("../bank/BankSelect")
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
      showSharedApiKey: false,
      appConstants: appConstants,
      changed: false
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

    this.watchModel();
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

      // TODO: should be implemented something like "touchet" ot bank details component
      if (result.bankDetails) {
        if (!result.bankDetails.bank && !result.bankDetails.bankBranch && !result.bankDetails.bankAccount) {
          result.bankDetails = null;
        }
      }  

      this.watchModel();
      return result;
    },
    async resetPrivateKey() {
      if (!this.model.terminalID) {
        return;
      }
      let operation = await this.$api.terminals.resetPrivateApiKey(
        this.model.terminalID
      );
      if (!this.$apiSuccess(operation)) return;

      this.privateApiKey = operation.entityReference;
    },
    async showPrivateKey(){
      if (!this.model.terminalID || this.privateApiKey) {
        return;
      }
      let operation = await this.$api.terminals.getPrivateApiKey(
        this.model.terminalID
      );
      if (!this.$apiSuccess(operation)) return;

      this.privateApiKey = operation.entityReference;
    },
    async resetSharedKey() {
      if (!this.model.terminalID) {
        return;
      }
      let operation = await this.$api.terminals.resetSharedApiKey(
        this.model.terminalID
      );
      if (!this.$apiSuccess(operation)) return;

      this.showSharedKey = true;
      this.model.sharedApiKey = operation.entityReference;
      this.emitUpdate();
    },
    emitUpdate() {
      this.$emit("update", this.model);
    },
    deleteRedirectUrl(item) {
      let idx = this.model.checkoutSettings.redirectUrls.findIndex(
        i => i == item
      );
      if (idx === -1) {
        return;
      }

      this.model.checkoutSettings.redirectUrls.splice(idx, 1);
    },
    addRedirectUrl() {
      if (!this.model.checkoutSettings.redirectUrls) {
        this.model.checkoutSettings.redirectUrls = [];
      } else {
        let idx = this.model.checkoutSettings.redirectUrls.findIndex(i => !i);
        if (idx !== -1) {
          return;
        }
      }

      this.model.checkoutSettings.redirectUrls.push("");
    },
    watchModel(){
      this.changed = false;
      let modelWatcher = this.$watch('model', (nv, ov) => {
        this.changed = true;
        modelWatcher(); //unwatch
      }, { deep: true})
    },
    async deleteCustomCSS(){
      let operation = await this.$api.terminals.deleteCustomCSS(this.data.terminalID);
      if(!this.$apiSuccess(operation)) return;
      this.model.checkoutSettings.customCssReference = null;
    },
    async deleteMerchantLogo(){
      let operation = await this.$api.terminals.deleteMerchantLogo(this.data.terminalID);
      if(!this.$apiSuccess(operation)) return;
       this.data.paymentRequestSettings.merchantLogo = null;
    },
    getBlobUrl(resource){
      return `${this.$cfg.VUE_APP_BLOB_BASE_ADDRESS}/${resource}`;
    }
  },
};
</script>

<style lang="scss" scoped>
.w99 {
  width: 99%;
}
</style>