<template>
  <v-card class="ec-card d-flex flex-column" fill-height>
    <ec-dialog :dialog.sync="tokensDialog">
      <template v-slot:title>{{$t('SavedTokens')}}</template>
      <template>
        <div class="d-flex px-2 justify-end">
          <v-btn
            color="red"
            class="white--text"
            :disabled="selectedToken == null"
            :block="$vuetify.breakpoint.smAndDown"
            outlined
            @click="resetToken()"
          >
          <v-icon left>mdi-delete</v-icon>
          {{$t("Remove")}}
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
    <v-card-text class="py-0">
      <v-form class="ec-form" ref="form" lazy-validation>
        <v-row no-gutters>
          <v-col 
            cols="12"
            class="d-flex justify-center"
            v-if="includeDevice && availableDevices.length">
            <v-switch
              dense
              hide-details
              v-model="model.pinPad"
              :label="$t('UsePinPad')"
              :disabled="!availableDevices.length || model.useBit"
              @change="model.useBit = false">
            </v-switch>
          </v-col>
        </v-row>
        <template v-if="model.useBit">
          <bit-payment-component></bit-payment-component>
        </template>
        <template v-if="model.pinPad && availableDevices.length > 0">
          <v-select :items="availableDevices" v-model="selectedDevice" return-object :item-value="deviceValue" outlined>
            <template v-slot:item="{ item }">
              {{item.deviceID + '-' + item.deviceName}}
            </template>
            <template v-slot:selection="{ item }">
              {{item.deviceID + '-' + item.deviceName}}
            </template>
          </v-select>
        </template>
        <template v-else-if="!model.pinPad">
          <ec-dialog-invoker
            v-on:click="handleClick()"
            v-if="$featureEnabled(terminalStore, $appConstants.terminal.features.CreditCardTokens) && customerTokens"
            :clickable="(customerTokens.length > 0)"
            class="py-2"
          >
            <template v-slot:prepend>
              <v-icon>mdi-credit-card-outline</v-icon>
            </template>
            <template v-slot:left >
              <div v-if="!token">
                <span v-if="customerTokens.length > 0" >{{$t("@ChooseFromSavedCount").replace("@count", customerTokens.length)}}</span>
                <span v-if="customerTokens.length === 0">{{$t("NoSavedCards")}}</span>
              </div>
              <div v-if="token">
                <span class="primary--text">
                  <card-token-string :token="token"></card-token-string>
                </span>
              </div>
            </template>
            <template v-slot:append>
              <re-icon>mdi-chevron-right</re-icon>
            </template>
          </ec-dialog-invoker>
            <template v-if="!token">
              <credit-card-secure-details-fields
                :data="model.creditCardSecureDetails"
                ref="ccsecuredetailsform"
                :tokens="customerTokens"
                :show-customer-select-btn="includeCustomer"
                v-on:select-customer="onCustomerSelect()"
              ></credit-card-secure-details-fields>
              <v-row no-gutters>
                <v-col cols="5" md="3" v-if="$featureEnabled(terminalStore, $appConstants.terminal.features.CreditCardTokens)">
                  <v-checkbox v-model="model.saveCreditCard" :label="$t('SaveCard')"></v-checkbox>
                </v-col>
                <v-col cols="12" :md="$featureEnabled(terminalStore, $appConstants.terminal.features.CreditCardTokens) ? 9 : 12">
                  <v-text-field
                    class="mt-2"
                    v-model="model.oKNumber"
                    :label="$t('AuthorizationCode')"
                    :rules="[vr.primitives.stringLength(1, 50)]">
                  </v-text-field>
                </v-col>
              </v-row>
            </template>
            <v-text-field
              v-else
              v-model="model.oKNumber"
              :label="$t('AuthorizationCodeOptional')"
              :rules="[vr.primitives.stringLength(1, 50)]">
            </v-text-field>
        </template>
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
          :total-amount="data.transactionAmount"
          :key="model.transactionType"
          :transaction-type="model.transactionType"
        ></installment-details>
      </v-form>
    </v-card-text>
    <v-card-actions class="px-4" v-if="btnText">
      <v-btn color="primary" bottom :x-large="true" block @click="ok()">
        {{$t(btnText)}}
        <ec-money :amount="data.transactionAmount" class="px-1" :currency="data.currency"></ec-money>
      </v-btn>
    </v-card-actions>
  </v-card>
</template>

<script>
import ValidationRules from "../../helpers/validation-rules";
import { mapState } from "vuex";

export default {
  components: {
    EcMoney: () => import("../ec/EcMoney"),
    CreditCardSecureDetailsFields: () => import("./CreditCardSecureDetailsFields"),
    EcDialog: () => import("../../components/ec/EcDialog"),
    EcDialogInvoker: () => import("../../components/ec/EcDialogInvoker"),
    EcRadioGroup: () => import("../../components/inputs/EcRadioGroup"),
    ReIcon: () => import("../../components/misc/ResponsiveIcon"),
    CardTokenString: () => import("../../components/ctokens/CardTokenString"),
    InstallmentDetails: () => import("./InstallmentDetailsForm"),
    BitPaymentComponent: () => import("../integrations/BitPaymentComponent"),
  },
  props: {
    data: {
      type: Object,
      default: null,
      required: false
    },
    btnText: {
      type: String,
      default: 'OK',
      required: false
    },
    includeDevice: {
      type: Boolean,
      default: false
    },
    includeCustomer: {
      type: Boolean,
      default: false
    },
    allowBit: {
      type: Boolean,
      default: false
    }
  },
  data() {
    return {
      model: { 
        useBit: false,
        ...this.data
      },
      tokensDialog: false,
      customerTokens: null,
      selectedToken: null,
      selectedTokenObj: null,
      selectedPinPadDevice: null,
      availableDevices: [],
      vr: ValidationRules,
      dictionaries: {},
    };
  },
  async mounted() {
    let dictionaries = await this.$api.dictionaries.getTransactionDictionaries();
    if (dictionaries) {
      this.dictionaries = dictionaries;
      this.model.transactionType = this.dictionaries.transactionTypeEnum[0].code;
    }
    if (this.model.dealDetails.consumerID && this.$featureEnabled(this.terminalStore, this.$appConstants.terminal.features.CreditCardTokens)) {
      this.customerTokens =
        (
          await this.$api.cardTokens.getCustomerCardTokens(
            this.model.dealDetails.consumerID
          )
        ).data || [];
      this.selectedToken = this.model.creditCardToken;
    }else{
      this.model.saveCreditCard = false;
      this.selectedToken = this.selectedTokenObj = null;
    }
    await this.checkPinPadAvailability();
  },
  computed: {
    token: {
      get: function() {
        return this.selectedTokenObj;
      },
      set: function(nv) {
        this.selectedToken = nv ? nv.creditCardTokenID : null;
        this.selectedTokenObj = nv;
        this.tokensDialog = false;
      }
    },
    ...mapState({
      terminalStore: state => state.settings.terminal
    }),
    isInstallmentTransaction() {
      return (
        this.model.transactionType === "installments" ||
        this.model.transactionType === "credit"
      );
    }
  },
  methods: {
    ok() {
      let form = this.$refs.form.validate();
      if (!form) return;
      
      let result = null;

      if (this.model.pinPad) {
        result = this.okDevice();
      } 
      else if (this.selectedToken) {
        result = this.okCardToken();
      } else {
        result = this.okCreditCard();
      }

      if(result == null){
        return;
      }

      if (this.$refs.instDetails) {
        result.installmentDetails = this.$refs.instDetails.getData();
      }else{
        result.installmentDetails = null;
      }
      result.dealDetails = this.model.dealDetails;
      result.transactionType = this.model.transactionType;
      this.$emit("ok", result);

      return true;
    },
    resetToken() {
      this.token = null;
    },
    okDevice(){
      if(!this.cardOwnerNationalID)
      return {
        type: "device",
        oKNumber: this.model.oKNumber,
        data: {
          pinPad: true,
          pinPadDeviceID: this.selectedDevice.deviceID,
          cardOwnerNationalID: this.model.cardOwnerNationalID
        }
      };
    },
    okCardToken() {
      if (!this.selectedToken) return;

      return {
        type: "token",
        oKNumber: this.model.oKNumber,
        data: this.selectedToken
      };
    },
    okCreditCard() {
      let data = this.$refs.ccsecuredetailsform.getData();

      if (!data) {
        return;
      }

      return {
        type: "creditcard",
        oKNumber: this.model.oKNumber,
        data: {
          ...this.model.creditCardSecureDetails,
          ...data,
          saveCreditCard: this.model.saveCreditCard,
        }
      };
    },
    handleClick() {
      this.tokensDialog = this.customerTokens && this.customerTokens.length > 0;
    },
    async checkPinPadAvailability(){
      if(this.$integrationAvailable(this.terminalStore, this.$appConstants.terminal.integrations.pinpadProcessor)){
        this.availableDevices = await this.$api.terminals.getTerminalDevices(this.terminalStore.terminalID);
        this.selectedDevice = this.availableDevices[0];
      }
    },
    deviceValue(a){
      return `${a.deviceID}-${a.deviceName}`;
    },
    onCustomerSelect(){
      if(this.includeCustomer){
        this.$emit('select-customer');
      }
    }
  }
};
</script>