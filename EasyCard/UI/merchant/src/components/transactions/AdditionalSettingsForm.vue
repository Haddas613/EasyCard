<template>
  <v-card class="ec-card d-flex flex-column">
    <v-card-text class="py-2">
      <v-form class="ec-form" ref="form" lazy-validation>
        <!-- <v-select
          :items="dictionaries.jDealTypeEnum"
          item-text="description"
          item-value="code"
          v-model="model.jDealType"
          :label="$t('JDealType')"
          solo
        ></v-select> -->
        <v-select
          :items="dictionaries.currencyEnum"
          item-text="description"
          item-value="code"
          v-model="model.currency"
          :label="$t('Currency')"
          solo
        ></v-select>
        <v-select
          :items="dictionaries.transactionTypeEnum"
          item-text="description"
          item-value="code"
          v-model="model.transactionType"
          :label="$t('TransactionType')"
          solo
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
          solo
          required
        ></v-text-field>
        <v-text-field
          v-model="model.dealDetails.consumerEmail"
          :label="$t('ConsumerEmail')"
          :rules="[vr.primitives.email]"
          solo
          @keydown.native.space.prevent
        ></v-text-field>
        <v-text-field
          v-model="model.dealDetails.consumerPhone"
          :label="$t('ConsumerPhone')"
          :rules="[vr.primitives.maxLength(50)]"
          solo
          @keydown.native.space.prevent
        ></v-text-field>
        <v-textarea
          v-model="model.dealDetails.dealDescription"
          :counter="1024"
          solo
          :rules="[vr.primitives.required,  vr.primitives.maxLength(1024)]"
        >
          <template v-slot:label>
            <div>{{$t('DealDescription')}}</div>
          </template>
        </v-textarea>
      </v-form>
    </v-card-text>
    <v-btn color="primary" class="px-2" bottom :x-large="true" block @click="ok()">{{$t('Confirm')}}</v-btn>
  </v-card>
</template>

<script>
import InstallmentDetails from "./InstallmentDetailsForm";
import ValidationRules from "../../helpers/validation-rules";

export default {
  components: {
    InstallmentDetails
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
      model: { ...this.data },
      vr: ValidationRules
    };
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
    let dictionaries = await this.$api.dictionaries.getTransactionDictionaries();
    if (dictionaries) {
      this.dictionaries = dictionaries;
      this.model.transactionType = this.dictionaries.transactionTypeEnum[0].code;
      this.model.currency = this.dictionaries.currencyEnum[0].code;
      this.model.jDealType = this.dictionaries.jDealTypeEnum[0].code;
      this.model.cardPresence = this.dictionaries.cardPresenceEnum[1].code;
    }
  },
  methods: {
    ok() {
      if (!this.$refs.form.validate()) return;

      this.model.installmentDetails = this.$refs.instDetails.model;
      this.$emit("ok", this.model);
    }
  }
};
</script>

<style lang="scss" scoped>
</style>