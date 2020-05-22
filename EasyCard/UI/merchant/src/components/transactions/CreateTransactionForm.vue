<template>
  <v-form ref="form" v-model="valid" lazy-validation>
    <v-container class="px-10" fluid>
      <v-row>
        <v-col cols="12" md="4">
          <v-text-field
            v-model="model.terminalID"
            :label="$t('Terminal')"
            required
            type="'number'"
            :disabled="true"
          ></v-text-field>
        </v-col>
        <v-col cols="12" md="4">
          <v-select :items="dictionaries.transactionTypes" item-text="description" item-value="code" v-model="model.transactionType" :label="$t('TransactionType')"></v-select>
        </v-col>
        <v-col cols="12" md="4">
          <v-select :items="dictionaries.currencies" item-text="description" item-value="code" v-model="model.currency" :label="$t('Currency')"></v-select>
        </v-col>
      </v-row>

      <v-row>
        <v-col cols="12" md="4">
          <v-select
            :items="creditCardTokens"
            :item-text="'label'"
            :item-value="'value'"
            v-model="model.creditCardToken"
            :label="$t('CreditCardToken')"
          ></v-select>
        </v-col>
        <v-col cols="12" md="8" v-if="model.creditCardToken === null">
          <credit-card-secure-details ref="ccSecureDetails" :data="model.creditCardSecureDetails"></credit-card-secure-details>
        </v-col>
      </v-row>

      <v-row>
        <v-col cols="12" md="4">
          <v-text-field
            v-model="model.dealDetails.dealReference"
            :counter="50"
            :rules="[vr.primitives.maxLength(50)]"
            :label="$t('DealReference')"
            required
          ></v-text-field>
        </v-col>
        <v-col cols="12" md="4">
          <v-text-field
            v-model="model.dealDetails.consumerEmail"
            :label="$t('ConsumerEmail')"
            :rules="[vr.primitives.email]"
          ></v-text-field>
        </v-col>
        <v-col cols="12" md="4">
          <v-text-field
            v-model="model.dealDetails.consumerPhone"
            :label="$t('ConsumerPhone')"
            :rules="[vr.primitives.maxLength(50)]"
          ></v-text-field>
        </v-col>
      </v-row>
      <v-row class="d-flex align-end">
        <v-col cols="12" md="3">
          <v-textarea
            v-model="model.dealDetails.dealDescription"
            :counter="1024"
            :rules="[vr.primitives.maxLength(1024)]"
          >
            <template v-slot:label>
              <div>
                {{$t('DealDescription')}}
                <small>(optional)</small>
              </div>
            </template>
          </v-textarea>
        </v-col>
        <v-spacer></v-spacer>
        <v-col cols="12" md="3">
          <v-text-field
            v-model="model.transactionAmount"
            :label="$t('Amount')"
            type="number"
            min="0"
            step="0.01"
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
</template>

<script>
import ValidationRules from "../../helpers/validation-rules";
import CreditCardSecureDetails from './CreditCardSecureDetailsForm';

export default {
  components: {
    CreditCardSecureDetails,
  },
  name: "CreateTransactionForm",
  methods: {
    save() {
      if (!this.$refs.form.validate()) 
        return;
        
      //retrieve data from child component
      this.model.creditCardSecureDetails = this.$refs.ccSecureDetails.model;
      this.$emit('save', this.model)
    }
  },
  async mounted () {
    this.dictionaries = await this.$api.dictionaries.getTransactionDictionaries();
    this.model.transactionType =  this.dictionaries.transactionTypes[0].code;
    this.model.currency =  this.dictionaries.currencies[0].code;
  },
  data() {
    return {
      //TODO: credit token, terminal id
      model: {
        terminalID: 0,
        transactionType: 0,
        currency: 0,
        cardPresence: 0,
        creditCardToken: null,
        creditCardSecureDetails: {
          cardExpiration: {
            year: new Date().getFullYear(),
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
        }
      },
      dictionaries: {},
      valid: true,
      //TODO: extend tokens
      creditCardTokens: [
        {
          label: this.$t("UseCreditCard"),
          value: null
        }
      ],
      vr: ValidationRules
    };
  }
};
</script>

<style lang="scss" scoped>
</style>