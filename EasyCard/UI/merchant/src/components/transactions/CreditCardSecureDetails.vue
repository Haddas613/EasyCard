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
    <v-card-text class="py-2">
      <v-form class="ec-form" ref="form" lazy-validation>
        <v-switch v-if="includeDevice"
          v-model="model.pinPad" :label="$t('UsePinPad')" :disabled="!availableDevices.length">
        </v-switch>
        <template v-if="model.pinPad">
            <v-row no-gutters>
              <v-col cols="12" v-if="availableDevices.length > 0">
                <v-select :items="availableDevices" v-model="selectedDevice" return-object :item-value="deviceValue" outlined>
                  <template v-slot:item="{ item }">
                    {{item.deviceID + '-' + item.deviceName}}
                  </template>
                  <template v-slot:selection="{ item }">
                    {{item.deviceID + '-' + item.deviceName}}
                  </template>
                </v-select>
              </v-col>
              <v-col cols="12">
                <v-text-field
                  v-model="model.cardOwnerNationalID"
                  :rules="[vr.special.israeliNationalId]"
                  :label="$t('NationalID')"
                  outlined
                ></v-text-field>
              </v-col>
            </v-row>
        </template>
        <template v-if="!model.pinPad">
          <ec-dialog-invoker
            v-on:click="handleClick()"
            v-if="customerTokens"
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
              ></credit-card-secure-details-fields>
              <v-checkbox v-model="model.saveCreditCard" :label="model.dealDetails.consumerID ? $t('SaveCard') : $t('SaveCardCreateNewCustomer')"></v-checkbox>
            </template>
            <v-text-field
              v-model="model.oKNumber"
              :label="$t('AuthorizationCodeOptional')"
              :rules="[vr.primitives.stringLength(1, 50)]">
            </v-text-field>
        </template>
      </v-form>
    </v-card-text>
    <v-card-actions class="px-4">
      <v-btn color="primary" bottom :x-large="true" block @click="ok()">
        {{$t(btnText)}}
        <ec-money :amount="model.transactionAmount" class="px-1" :currency="model.currency"></ec-money>
      </v-btn>
    </v-card-actions>
  </v-card>
</template>

<script>
import ValidationRules from "../../helpers/validation-rules";
import { mapState } from "vuex";
import appConstants from "../../helpers/app-constants";

export default {
  components: {
    EcMoney: () => import("../ec/EcMoney"),
    CreditCardSecureDetailsFields: () => import("./CreditCardSecureDetailsFields"),
    EcDialog: () => import("../../components/ec/EcDialog"),
    EcDialogInvoker: () => import("../../components/ec/EcDialogInvoker"),
    EcRadioGroup: () => import("../../components/inputs/EcRadioGroup"),
    ReIcon: () => import("../../components/misc/ResponsiveIcon"),
    CardTokenString: () => import("../../components/ctokens/CardTokenString")
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
    }
  },
  data() {
    return {
      model: { ...this.data },
      tokensDialog: false,
      customerTokens: null,
      selectedToken: null,
      selectedTokenObj: null,
      appConstants: appConstants,
      selectedPinPadDevice: null,
      availableDevices: [],
      vr: ValidationRules
    };
  },
  async mounted() {
    if (this.model.dealDetails.consumerID) {
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
  },
  methods: {
    ok() {
      let form = this.$refs.form.validate();
      if (!form) return;

      if (this.model.pinPad) {
        this.okDevice();
      } 
      else if (this.selectedToken) {
        this.okCardToken();
      } else {
        this.okCreditCard();
      }
    },
    resetToken() {
      this.token = null;
    },
    okDevice(){
      if(!this.cardOwnerNationalID)
      this.$emit("ok", {
        type: "device",
        data: {
          pinPad: true,
          pinPadDeviceID: this.selectedDevice.deviceID,
          oKNumber: this.model.oKNumber,
          cardOwnerNationalID: this.model.cardOwnerNationalID
        }
      });
    },
    okCardToken() {
      if (!this.selectedToken) return;

      this.$emit("ok", {
        type: "token",
        data: this.selectedToken,
        oKNumber: this.model.oKNumber
      });
    },
    okCreditCard() {
      let data = this.$refs.ccsecuredetailsform.getData();

      if (!data) {
        return;
      }

      this.$emit("ok", {
        type: "creditcard",
        data: {
          ...this.model.creditCardSecureDetails,
          ...data,
          saveCreditCard: this.model.saveCreditCard,
          oKNumber: this.model.oKNumber
        }
      });
    },
    handleClick() {
      this.tokensDialog = this.customerTokens && this.customerTokens.length > 0;
    },
    async checkPinPadAvailability(){
      if(this.$integrationAvailable(this.terminalStore, appConstants.terminal.integrations.pinpadProcessor)){
        this.availableDevices = await this.$api.terminals.getTerminalDevices(this.terminalStore.terminalID);
        this.selectedDevice = this.availableDevices[0];
      }
    },
    deviceValue(a){
      return `${a.deviceID}-${a.deviceName}`;
    }
  }
};
</script>