<template>
  <v-card class="ec-card d-flex flex-column">
    <v-card-text class="py-2">
      <v-form class="ec-form" ref="form" lazy-validation>
        <v-select
          :items="dictionaries.jDealTypeEnum"
          item-text="description"
          item-value="code"
          v-model="model.jDealType"
          :label="$t('JDealType')"
          outlined
        ></v-select>
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
        ></installment-details>

        <v-text-field
          v-model="model.dealDetails.dealReference"
          :counter="50"
          :rules="[vr.primitives.maxLength(50)]"
          :label="$t('DealReference')"
          @keydown.native.space.prevent
          outlined
          required
        ></v-text-field>
        <v-text-field
          v-model="model.dealDetails.consumerEmail"
          :label="$t('ConsumerEmail')"
          :rules="[vr.primitives.email]"
          outlined
          @keydown.native.space.prevent
        ></v-text-field>
        <v-text-field
          v-model="model.dealDetails.consumerPhone"
          :label="$t('ConsumerPhone')"
          :rules="[vr.primitives.maxLength(50)]"
          outlined
          @keydown.native.space.prevent
        ></v-text-field>
        <v-textarea
          v-model="model.dealDetails.dealDescription"
          :counter="1024"
          outlined
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
import { mapState } from "vuex";

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
    },
    ...mapState({
      currencyStore: state => state.settings.currency
    })
  },
  async mounted() {
    let dictionaries = await this.$api.dictionaries.getTransactionDictionaries();
    if (dictionaries) {
      this.dictionaries = dictionaries;
      this.model.transactionType = this.dictionaries.transactionTypeEnum[0].code;

      if(!this.model.currency){
        this.model.currency = this.currencyStore.code || this.dictionaries.currencyEnum[0].code;
      }

      this.model.jDealType = this.dictionaries.jDealTypeEnum[0].code;
      this.model.cardPresence = this.dictionaries.cardPresenceEnum[1].code;
    }
  },
  methods: {
    ok() {
      if (!this.$refs.form.validate()) return;

      if(this.$refs.instDetails){
        this.model.installmentDetails = this.$refs.instDetails.model;
      }
      
      this.$emit("ok", this.model);
    }
  }
};
</script>

<style lang="scss" scoped>
</style>