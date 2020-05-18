<template>
  <v-form ref="form" v-model="valid" lazy-validation>
    <v-container class="px-10" fluid>
      <v-row>
        <v-col cols="4">
          <v-text-field
            v-model="model.terminalID"
            :label="$t('_Forms.Terminal')"
            required
            type="'number'"
            :disabled="true"
          ></v-text-field>
        </v-col>
        <v-col cols="4">
          <v-select :items="[]" v-model="model.transactionType" :label="$t('_Forms.TransactionType')"></v-select>
        </v-col>
        <v-col cols="4">
          <v-select :items="[]" v-model="model.currency" :label="$t('_Forms.Currency')"></v-select>
        </v-col>
      </v-row>

      <v-row>
        <v-col cols="4">
          <v-select
            :items="creditCardTokens"
            :item-text="'label'"
            :item-value="'value'"
            v-model="model.creditCardToken"
            :label="$t('_Forms.CreditCardToken')"
          ></v-select>
        </v-col>
        <v-col cols="8" v-if="model.creditCardToken === null">
          <v-card outlined>
            <v-container fluid class="px-2">
              <v-row>
                <v-col cols="4">
                  <v-text-field
                    v-model="model.creditCardSecureDetails.cardNumber"
                    :label="$t('_Forms.CardNumber')"
                    required
                    :rules="[vr.primitives.required, v => vr.primitives.stringLength(v, 10, 19)]"
                    type="'text'"
                  ></v-text-field>
                </v-col>
                <v-col cols="4">
                  <v-text-field
                    v-model="model.creditCardSecureDetails.cardOwnerName"
                    :label="$t('_Forms.OwnerName')"
                    :rules="[v => vr.primitives.stringLength(v, 2, 50)]"
                    required
                    type="'text'"
                  ></v-text-field>
                </v-col>
                <v-col cols="4">
                  <v-text-field
                    v-model="model.creditCardSecureDetails.cardOwnerNationalID"
                    :label="$t('_Forms.NationalID')"
                    :rules="[vr.special.israeliNationalId]"
                    required
                    type="'text'"
                  ></v-text-field>
                </v-col>
              </v-row>
              <v-row>
                <v-col cols="4">
                  <v-text-field
                    v-model="model.creditCardSecureDetails.cardExpiration.year"
                    :label="$t('_Forms.ExpirationYear')"
                    type="number"
                    v-bind:min="todayDate.getFullYear()"
                    :rules="[vr.primitives.required, v => vr.primitives.inRangeFlat(v, todayDate.getFullYear(), creditCardExpToDate.getFullYear())]"
                    required
                  ></v-text-field>
                </v-col>
                <v-col cols="4">
                  <v-text-field
                    v-model="model.creditCardSecureDetails.cardExpiration.month"
                    :label="$t('_Forms.ExpirationMonth')"
                    type="number"
                    min="1"
                    max="12"
                    :rules="[...vr.complex.month, v => vr.primitives.expired(v, todayDate.getMonth() + 1)]"
                    required
                  ></v-text-field>
                </v-col>
                <v-col cols="2">
                  <v-text-field
                    v-model="model.creditCardSecureDetails.cvv"
                    :label="$t('_Forms.CVV')"
                    type="text"
                    :counter="3"
                    :rules="vr.complex.cvv"
                    required
                  ></v-text-field>
                </v-col>
              </v-row>
            </v-container>
          </v-card>
        </v-col>
      </v-row>

      <v-row>
        <v-col cols="4">
          <v-text-field
            v-model="model.dealDetails.dealReference"
            :counter="50"
            :rules="[vr.primitives.maxLength(model.dealDetails.dealReference, 50)]"
            :label="$t('_Forms.DealReference')"
            required
          ></v-text-field>
        </v-col>
        <v-col cols="4">
          <v-text-field
            v-model="model.dealDetails.consumerEmail"
            :label="$t('_Forms.ConsumerEmail')"
            :rules="[vr.primitives.email]"
          ></v-text-field>
        </v-col>
        <v-col cols="4">
          <v-text-field
            v-model="model.dealDetails.consumerPhone"
            :label="$t('_Forms.ConsumerPhone')"
            :rules="[vr.primitives.maxLength(model.dealDetails.consumerPhone, 50)]"
          ></v-text-field>
        </v-col>
      </v-row>
      <v-row class="d-flex align-end">
        <v-col cols="6">
          <v-textarea
            v-model="model.dealDetails.dealDescription"
            :counter="1024"
            :rules="[vr.primitives.maxLength(model.dealDetails.dealDescription, 1024)]"
          >
            <template v-slot:label>
              <div>
                {{$t('_Forms.DealDescription')}}
                <small>(optional)</small>
              </div>
            </template>
          </v-textarea>
        </v-col>
        <v-spacer></v-spacer>
        <v-col cols="3">
          <v-text-field
            v-model="model.transactionAmount"
            :label="$t('_Forms.Amount')"
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

export default {
  name: "CreateTransactionForm",
  methods: {
    save() {
      if (!this.$refs.form.validate()) 
        return;

      this.$emit('save', this.model)
    }
  },
  data() {
    return {
      //TODO: all dictionaries, terminal id
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
          label: this.$t("_Forms.UseCreditCard"),
          value: null
        }
      ],
      todayDate: new Date(),
      creditCardExpToDate: (() => {
        let d = new Date();
        return new Date(d.getFullYear() + 10, d.getMonth(), d.getDay());
      })(),
      vr: ValidationRules
    };
  }
};
</script>

<style lang="scss" scoped>
</style>