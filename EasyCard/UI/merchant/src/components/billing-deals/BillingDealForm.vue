<template>
  <v-form class="pt-0" ref="form" v-model="valid" lazy-validation>
    <v-row>
      <v-col cols="12" md="6" class="py-0">
        <v-select
          :items="terminals"
          item-text="label"
          item-value="terminalID"
          v-model="model.terminalID"
          outlined
          :label="$t('Terminal')"
          required
          disabled
        ></v-select>
      </v-col>
      <v-col cols="12" md="6" class="py-0">
        <v-select
          :items="dictionaries.currencyEnum"
          item-text="description"
          item-value="code"
          v-model="model.currency"
          required
          :label="$t('Currency')"
          outlined
        ></v-select>
      </v-col>
      <v-col cols="12" md="6" class="pb-2 pt-0">
        <ec-dialog :dialog.sync="customersDialog" color="ecbg">
          <template v-slot:title>{{$t('Customers')}}</template>
          <template>
            <div class="d-flex pb-2 justify-end">
              <v-btn
                color="red"
                class="white--text"
                :disabled="selectedCustomer == null"
                :block="$vuetify.breakpoint.smAndDown"
                @click="selectedCustomer = null; customersDialog = false;"
              >
                <v-icon left>mdi-delete</v-icon>
                {{$t("CancelSelection")}}
              </v-btn>
            </div>
            <customers-list
              :key="model.terminalID"
              :show-previously-charged="true"
              :filter-by-terminal="true"
              v-on:ok="processCustomer($event)"
            ></customers-list>
          </template>
        </ec-dialog>
        <ec-dialog-invoker
          v-on:click="customersDialog = true"
          v-bind:class="{'pt-2': $vuetify.breakpoint.smAndDown, 'pt-7': $vuetify.breakpoint.mdAndUp}"
        >
          <template v-slot:prepend>
            <v-icon>mdi-account</v-icon>
          </template>
          <template v-slot:left>
            <div v-if="!selectedCustomer">{{$t("ChooseCustomer")}}</div>
            <div v-if="selectedCustomer">
              <span class="primary--text">{{selectedCustomer.consumerName}}</span>
            </div>
          </template>
          <template v-slot:append>
            <re-icon>mdi-chevron-right</re-icon>
          </template>
        </ec-dialog-invoker>
      </v-col>
      <v-col
        cols="12"
        md="6"
        class="pb-2"
        v-bind:class="{'pt-2': $vuetify.breakpoint.smAndDown, 'pt-0': $vuetify.breakpoint.mdAndUp}"
      >
        <v-flex class="d-flex justify-end">
          <v-btn
            color="success"
            x-small
            :disabled="!model.dealDetails.consumerID"
            @click="ctokenDialog = true;"
          >
            <v-icon left small>mdi-plus</v-icon>
            {{$t("AddToken")}}
          </v-btn>
        </v-flex>
        <card-token-form-dialog
          v-if="model.dealDetails.consumerID"
          :key="model.dealDetails.consumerID"
          :customer-id="model.dealDetails.consumerID"
          :show.sync="ctokenDialog"
          v-on:ok="createCardToken($event)"
          ref="ctokenDialogRef"
        ></card-token-form-dialog>
        <ec-dialog :dialog.sync="tokensDialog">
          <template v-slot:title>{{$t('SavedTokens')}}</template>
          <template>
            <div class="d-flex px-2 justify-end">
              <v-btn
                color="red"
                class="white--text"
                :disabled="selectedToken == null"
                :block="$vuetify.breakpoint.smAndDown"
                @click="resetToken()"
              >
                <v-icon left>mdi-delete</v-icon>
                {{$t("CancelSelection")}}
              </v-btn>
            </div>
            <ec-radio-group
              :data="customerTokens"
              valuekey="creditCardTokenID"
              item-disabled-key="expired"
              return-object
              :model.sync="token"
            >
              <template v-slot="{ item }">
                <card-token-string :token="item"></card-token-string>
              </template>
            </ec-radio-group>
          </template>
        </ec-dialog>
        <ec-dialog-invoker
          v-on:click="handleClick()"
          :clickable="model.dealDetails.consumerID"
          class="pt-2"
        >
          <template v-slot:prepend>
            <v-icon>mdi-credit-card-outline</v-icon>
          </template>
          <template v-slot:left>
            <div v-if="!token">
              <span
                v-if="customerTokens.length > 0"
              >{{$t("@ChooseFromSavedCount").replace("@count", customerTokens.length)}}</span>
              <span
                class="ecgray--text"
                v-if="!model.dealDetails.consumerID && customerTokens.length === 0"
              >{{$t("PleaseSelectCustomerFirst")}}</span>
              <span
                v-if="model.dealDetails.consumerID && customerTokens.length === 0"
              >{{$t("NoSavedCards")}}</span>
            </div>
            <div v-if="token">
              <span class="primary--text">{{token.cardNumber}}</span>
            </div>
          </template>
          <template v-slot:append>
            <re-icon>mdi-chevron-right</re-icon>
          </template>
        </ec-dialog-invoker>
      </v-col>
      <v-col cols="12" class="pt-0">
        <ec-dialog :dialog.sync="scheduleDialog" color="ecbg">
          <template v-slot:title>{{$t('BillingSchedule')}}</template>
          <template v-slot:right>
            <v-btn color="primary" @click="applySchedule()">{{$t('Apply')}}</v-btn>
          </template>
          <template>
            <billing-schedule-form
              ref="billingScheduleRef"
              class="px-4 py-4"
              :data="model.billingSchedule"
            ></billing-schedule-form>
          </template>
        </ec-dialog>
        <ec-dialog-invoker v-on:click="scheduleDialog = true;" class="py-2">
          <template v-slot:prepend>
            <v-icon>mdi-calendar</v-icon>
          </template>
          <template v-slot:left>
            <billing-schedule-string
              :schedule="model.billingSchedule"
              replacement-text="SelectSchedule"
            ></billing-schedule-string>
          </template>
          <template v-slot:append>
            <re-icon>mdi-chevron-right</re-icon>
          </template>
        </ec-dialog-invoker>
      </v-col>
      <v-row class="px-2">
        <v-col cols="12" md="6">
          <v-text-field
            class="mt-4"
            v-model="model.transactionAmount"
            outlined
            :label="$t('Amount')"
            hide-details="true"
            @input="calculateTotal()"
          ></v-text-field>
        </v-col>
        <v-col cols="12" md="6">
          <v-text-field
            class="mt-4"
            v-if="model"
            :value="model.netTotal"
            outlined
            readonly
            disabled
            :label="$t('NetAmount')"
            hide-details="true"
          ></v-text-field>
        </v-col>
        <v-col cols="12" md="6">
          <v-text-field
            class="mt-4"
            v-if="model"
            :value="model.vatTotal"
            outlined
            readonly
            disabled
            :label="$t('VAT')"
            hide-details="true"
          ></v-text-field>
        </v-col>
        <v-col cols="12" md="6">
          <v-text-field
            class="mt-4"
            :value="(model.vatRate * 100).toFixed(0)"
            readonly
            disabled
            outlined
            :label="$t('VATPercent')"
            hide-details="true"
          ></v-text-field>
        </v-col>
      </v-row>
      <deal-details
        class="px-2"
        ref="dealDetails"
        :data="model.dealDetails"
        :key="model.dealDetails ? model.dealDetails.consumerEmail : model.dealDetails"
      ></deal-details>
      <v-col cols="12" class="d-flex justify-end" v-if="!$vuetify.breakpoint.smAndDown">
        <v-btn class="mx-1" color="white" :to="{ name: 'BillingDeals' }">{{$t('Cancel')}}</v-btn>
        <v-btn color="primary" @click="ok()" :disabled="!token">{{$t('Save')}}</v-btn>
      </v-col>
      <v-col cols="12" v-if="$vuetify.breakpoint.smAndDown">
        <v-btn block color="white" :to="{ name: 'BillingDeals' }">{{$t('Cancel')}}</v-btn>
        <v-spacer class="py-2"></v-spacer>
        <v-btn block color="primary" @click="ok()" :disabled="!token">{{$t('Save')}}</v-btn>
      </v-col>
    </v-row>
  </v-form>
</template>

<script>
import ValidationRules from "../../helpers/validation-rules";
import { mapState } from "vuex";
import itemPricingService from "../../helpers/item-pricing";

export default {
  components: {
    DealDetails: () => import("../transactions/DealDetailsForm"),
    CustomersList: () => import("../customers/CustomersList"),
    BillingScheduleForm: () => import("./BillingScheduleForm"),
    BillingScheduleString: () => import("./BillingScheduleString"),
    EcDialog: () => import("../ec/EcDialog"),
    EcDialogInvoker: () => import("../ec/EcDialogInvoker"),
    EcRadioGroup: () => import("../inputs/EcRadioGroup"),
    ReIcon: () => import("../misc/ResponsiveIcon"),
    CardTokenFormDialog: () => import("../ctokens/CardTokenFormDialog"),
    CardTokenString: () => import("../ctokens/CardTokenString")
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
      model: { ...this.data },
      dictionaries: {},
      vr: ValidationRules,
      terminals: [],
      valid: true,
      tokensDialog: false,
      customerTokens: [],
      selectedToken: null,
      selectedCustomer: null,
      customersDialog: false,
      scheduleDialog: false,
      ctokenDialog: false
    };
  },
  computed: {
    ...mapState({
      terminalStore: state => state.settings.terminal,
      currencyStore: state => state.settings.currency
    }),
    totalAmount() {
      return (this.model.totalAmount = (
        this.model.transactionAmount * this.model.numberOfPayments
      ).toFixed(2));
    },
    token: {
      get: function() {
        return this.selectedToken;
      },
      set: function(nv) {
        this.model.creditCardToken = nv ? nv.creditCardTokenID : null;
        this.selectedToken = nv;
        this.tokensDialog = false;
      }
    }
  },
  methods: {
    async processCustomer(data) {
      this.selectedCustomer = data;
      this.model.dealDetails.consumerEmail = data.consumerEmail;
      this.model.dealDetails.consumerPhone = data.consumerPhone;
      this.model.dealDetails.consumerID = data.consumerID;
      await this.getCustomerTokens();
      this.customersDialog = false;
    },
    handleClick() {
      this.tokensDialog = this.customerTokens && this.customerTokens.length > 0;
    },
    ok() {
      if (!this.$refs.form.validate()) return;
      let result = { ...this.model };
      result.dealDetails = this.$refs.dealDetails.getData();
      //if this is edit and billing schedule has not been clicked, no need to validate
      if (!this.$refs.billingScheduleRef && this.model.billingDealID) {
        return this.$emit("ok", result);
      }
      if (
        !this.$refs.billingScheduleRef ||
        !this.$refs.billingScheduleRef.validate()
      ) {
        this.scheduleDialog = true;
        this.$toasted.show(this.$t("CheckScheduleSettings"), { type: "error" });
        return;
      }

      this.$emit("ok", result);
    },
    applySchedule() {
      if (!this.$refs.billingScheduleRef.validate()) {
        return;
      }
      this.scheduleDialog = false;
      this.model.billingSchedule = this.$refs.billingScheduleRef.model;
    },
    async createCardToken(data) {
      this.ctokenDialog = false;
      let result = await this.$api.cardTokens.createCardToken(data);
      //server errors will be displayed automatically
      if (!result) return;
      if (result.status === "success") {
        await this.getCustomerTokens();
        this.token = this.lodash.find(
          this.customerTokens,
          t => t.creditCardTokenID == result.entityReference
        );
      } else {
        this.$toasted.show(result.message, { type: "error" });
      }
      this.$refs.ctokenDialogRef.reset();
    },
    async getCustomerTokens() {
      this.customerTokens =
        (
          await this.$api.cardTokens.getCustomerCardTokens(
            this.model.dealDetails.consumerID
          )
        ).data || [];
    },
    calculateTotal(){
      itemPricingService.total.calculateWithoutItems(this.model, 'transactionAmount', { vatRate: this.terminalStore.settings.vatRate });
    }
  },
  async mounted() {
    this.terminals = (await this.$api.terminals.getTerminals()).data || [];
    this.dictionaries = await this.$api.dictionaries.getTransactionDictionaries();
    if (!this.model.terminalID) {
      this.model.terminalID =
        this.terminalStore.terminalID || this.terminals[0].terminalID;
    }
    if (!this.model.currency) {
      this.model.currency =
        this.currencyStore.code || this.dictionaries.currencyEnum[0].code;
    }
    if (this.model.dealDetails.consumerID) {
      this.selectedCustomer = await this.$api.consumers.getConsumer(
        this.model.dealDetails.consumerID
      );
      this.customerTokens =
        (
          await this.$api.cardTokens.getCustomerCardTokens(
            this.model.dealDetails.consumerID
          )
        ).data || [];

      if (this.model.creditCardToken) {
        this.selectedToken = this.lodash.find(
          this.customerTokens,
          t => t.creditCardTokenID === this.model.creditCardToken
        );
      }
    }
    this.calculateTotal();
  }
};
</script>

<style lang="css" scoped>
.pt-30px {
  padding-top: 30px !important;
}
</style>