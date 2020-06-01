<template>
  <div>
    <v-overlay :value="loading">
      <v-progress-circular indeterminate size="64"></v-progress-circular>
    </v-overlay>

    <v-snackbar
      color="success"
      v-model="successSnack.showSnackbar"
      :timeout="5000"
      :top="true"
      :right="true"
      :multi-line="true"
      :vertical="true"
    >
      <h4 class="py-2">{{$t('CreatedSuccessfully')}}</h4>
      <p v-if="successSnack.snackbarMsg">{{successSnack.snackbarMsg}}</p>
      <v-btn color="accent" text @click="successSnack.showSnackbar = false">{{$t('Close')}}</v-btn>
    </v-snackbar>

    <v-snackbar
      color="ecError"
      v-model="errorSnack.showSnackbar"
      :timeout="5000"
      :top="true"
      :right="true"
      :multi-line="true"
      :vertical="true"
    >
      <h4 class="py-2">{{$t('AnErrorOccured')}}</h4>
      <p v-if="errorSnack.snackbarMsg">{{errorSnack.snackbarMsg}}</p>
      <p v-for="err in errorSnack.snackbarErrorsArray" :key="err.code">{{err.description}}</p>
      <v-btn color="accent" text @click="errorSnack.showSnackbar = false">{{$t('Close')}}</v-btn>
    </v-snackbar>

    <v-form ref="form" v-model="valid" lazy-validation>
      <v-container class="px-10" fluid>
        <v-row>
          <v-col cols="12" md="2" sm="6">
            <v-select
              :items="terminals"
              item-text="label"
              item-value="terminalID"
              v-model="model.terminalID"
              :label="$t('Terminal')"
            ></v-select>
          </v-col>

          <v-col cols="12" md="2" sm="6">
            <v-select
              :items="creditCardTokens"
              :item-text="'label'"
              :item-value="'value'"
              v-model="model.creditCardToken"
              :label="$t('CreditCardToken')"
            ></v-select>
          </v-col>

          <v-col cols="12" md="2" sm="6">
            <v-select
              :items="dictionaries.cardPresenceEnum"
              item-text="description"
              item-value="code"
              v-model="model.cardPresence"
              :label="$t('CardPresence')"
            ></v-select>
          </v-col>

          <v-col cols="12" md="2" v-if="!isRefund" sm="6">
            <v-select
              :items="dictionaries.transactionTypeEnum"
              item-text="description"
              item-value="code"
              v-model="model.transactionType"
              :label="$t('TransactionType')"
            ></v-select>
          </v-col>
          <v-col cols="12" md="2" v-if="isRefund" sm="6">
            <v-text-field
              v-bind:value="$t('Refund')"
              :label="$t('TransactionType')"
              :disabled="true"
            ></v-text-field>
          </v-col>

          <v-col cols="12" md="2" sm="6">
            <v-select
              :items="dictionaries.jDealTypeEnum"
              item-text="description"
              item-value="code"
              v-model="model.jDealType"
              :label="$t('JDealType')"
            ></v-select>
          </v-col>

          <v-col cols="12" md="2" sm="6">
            <v-select
              :items="dictionaries.currencyEnum"
              item-text="description"
              item-value="code"
              v-model="model.currency"
              :label="$t('Currency')"
            ></v-select>
          </v-col>
        </v-row>

        <v-row v-if="model.creditCardToken === null" class="py-2" no-gutters>
          <v-col cols="12">
            <credit-card-secure-details ref="ccSecureDetails" :data="model.creditCardSecureDetails"></credit-card-secure-details>
          </v-col>
        </v-row>

        <v-row v-if="isInstallmentTransaction" class="py-2" no-gutters>
          <v-col cols="12">
            <installment-details ref="instDetails" :data="model.installmentDetails"></installment-details>
          </v-col>
        </v-row>

        <v-row>
          <v-col cols="12" md="4" sm="4">
            <v-text-field
              v-model="model.dealDetails.dealReference"
              :counter="50"
              :rules="[vr.primitives.maxLength(50)]"
              :label="$t('DealReference')"
              @keydown.native.space.prevent
              required
            ></v-text-field>
          </v-col>
          <v-col cols="12" md="4" sm="4">
            <v-text-field
              v-model="model.dealDetails.consumerEmail"
              :label="$t('ConsumerEmail')"
              :rules="[vr.primitives.email]"
              @keydown.native.space.prevent
            ></v-text-field>
          </v-col>
          <v-col cols="12" md="4" sm="4">
            <v-text-field
              v-model="model.dealDetails.consumerPhone"
              :label="$t('ConsumerPhone')"
              :rules="[vr.primitives.maxLength(50)]"
              @keydown.native.space.prevent
            ></v-text-field>
          </v-col>
        </v-row>
        <v-row class="d-flex align-end">
          <v-col cols="12" md="6">
            <v-textarea
              v-model="model.dealDetails.dealDescription"
              :counter="1024"
              :rules="[vr.primitives.required,  vr.primitives.maxLength(1024)]"
            >
              <template v-slot:label>
                <div>{{$t('DealDescription')}}</div>
              </template>
            </v-textarea>
          </v-col>
          <v-spacer></v-spacer>
          <v-col cols="12" md="3">
            <v-text-field
              v-model="model.transactionAmount"
              :label="$t('Amount')"
              type="number"
              min="0.01"
              step="0.01"
              :rules="[vr.primitives.biggerThan(0)]"
              required
            ></v-text-field>
          </v-col>
        </v-row>
        <v-row>
          <v-col cols="12" class="d-flex justify-end">
            <v-btn color="ecGray" class="mr-4" :to="'/transactions/list'">Go back</v-btn>
            <v-btn color="success" class="mr-4" @click="save()">Save</v-btn>
          </v-col>
        </v-row>
      </v-container>
    </v-form>
  </div>
</template>

<script>
import ValidationRules from "../../helpers/validation-rules";
import CreditCardSecureDetails from "./CreditCardSecureDetailsForm";
import InstallmentDetails from "./InstallmentDetailsForm";

export default {
  components: { CreditCardSecureDetails, InstallmentDetails },
  name: "CreateTransactionForm",
  props: {
    isRefund: {
      type: Boolean,
      default: false
    }
  },
  methods: {
    async save() {
      if (!this.$refs.form.validate()) return;

      //retrieve data from child component
      this.model.creditCardSecureDetails = this.$refs.ccSecureDetails.model;

      if (this.isInstallmentTransaction)
        this.model.installmentDetails = this.$refs.instDetails.model;

      this.loading = true;
      this.snackbarErrorsArray = [];

      let result = { isError: false };

      if (this.isRefund) {
        result = await this.$api.transactions.refund(this.model);
      } else {
        switch (this.model.jDealType) {
          case "J4":
            result = await this.$api.transactions.createTransaction(this.model);
            break;
          case "J2":
            result = await this.$api.transactions.checkCreditCard(this.model);
            break;
          case "J5":
            result = await this.$api.transactions.blockCreditCard(this.model);
            break;
          default:
            result = false;
            console.error(`unknown JDeal type: ${this.model.jDealType}`);
        }
      }

      if (result.isError) {
        this.loading = false;
        this.errorSnack.showSnackbar = true;
        this.errorSnack.snackbarMsg = result.message || null;
        this.errorSnack.snackbarErrorsArray = result.errors || [];
      } else {
        this.successSnack.showSnackbar = true;
        this.successSnack.snackbarMsg = result.message || null;
        setTimeout(() => {
          this.$router.push("/admin/transactions/list");
        }, 3000);
      }
    }
  },
  computed: {
    isInstallmentTransaction() {
      return (
        this.model.transactionType === "installments" ||
        this.model.transactionType === "credit"
      );
    }
  },
  async mounted() {
    this.dictionaries = await this.$api.dictionaries.getTransactionDictionaries();
    this.model.transactionType = this.dictionaries.transactionTypeEnum[0].code;
    this.model.currency = this.dictionaries.currencyEnum[0].code;
    this.model.jDealType = this.dictionaries.jDealTypeEnum[0].code;
    this.model.cardPresence = this.dictionaries.cardPresenceEnum[0].code;
    this.terminals = (await this.$api.terminals.getTerminals()).data;
  },
  data() {
    return {
      //TODO: credit token, terminal id
      model: {
        terminalID: null,
        transactionType: null,
        jDealType: null,
        currency: null,
        cardPresence: null,
        creditCardToken: null,
        creditCardSecureDetails: {
          cardExpiration: {
            year: new Date().getFullYear() - 2000,
            month: new Date().getMonth() + 1
          },
          cardNumber: null,
          cvv: null,
          cardOwnerNationalID: null,
          cardOwnerName: null
        },
        transactionAmount: 0.0,
        dealDetails: {
          dealReference: null,
          consumerEmail: null,
          consumerPhone: null,
          dealDescription: null
        },
        installmentDetails: {
          numberOfPayments: 0,
          initialPaymentAmount: 0,
          installmentPaymentAmount: 0
        }
      },
      dictionaries: {},
      valid: true,
      terminals: [],
      //TODO: extend tokens
      creditCardTokens: [
        {
          label: this.$t("UseCreditCard"),
          value: null
        }
      ],
      vr: ValidationRules,
      errorSnack: {
        showSnackbar: false,
        snackbarMsg: null,
        snackbarErrors: []
      },
      successSnack: {
        showSnackbar: false,
        snackbarMsg: null
      },
      loading: false
    };
  }
};
</script>