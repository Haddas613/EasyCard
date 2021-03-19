<template>
  <v-container fluid>
    <v-form ref="form" v-model="formIsValid">
      <v-row>
        <!-- <merchant-terminal-filter v-model="model"></merchant-terminal-filter> -->
        <v-col cols="12" md="4" class="pb-0">
          <v-text-field
            v-model="model.terminalID"
            :label="$t('TerminalID')"
            :rules="[vr.primitives.guid]"
          ></v-text-field>
        </v-col>
        <v-col cols="12" md="4" class="pb-0">
          <v-text-field
            v-model="model.merchantID"
            :label="$t('MerchantID')"
            :rules="[vr.primitives.guid]"
          ></v-text-field>
        </v-col>
        <v-col cols="12" md="4" class="pb-0">
          <v-text-field
            v-model="model.cardNumber"
            :label="$t('CardNumber')"
          ></v-text-field>
        </v-col>
        <v-col cols="12" md="4" class="pb-0">
          <v-text-field
            v-model="model.cardOwnerName"
            :label="$t('CardOwnerName')"
          ></v-text-field>
        </v-col>
        <v-col cols="12" md="4" class="pb-0">
          <v-text-field
            v-model="model.cardOwnerNationalID"
            :label="$t('CardOwnerNationalID')"
          ></v-text-field>
        </v-col>
        <v-col cols="12" md="4" class="pb-0">
          <v-text-field
            v-model="model.consumerEmail"
            :label="$t('ConsumerEmail')"
          ></v-text-field>
        </v-col>
      </v-row>
      <v-row>
        <v-col cols="12" class="d-flex justify-end">
          <v-btn color="success" class="mr-4" @click="apply()" :disabled="!formIsValid">{{$t('Apply')}}</v-btn>
        </v-col>
      </v-row>
    </v-form>
  </v-container>
</template>

<script>
import ValidationRules from "../../helpers/validation-rules";

export default {
  name: "CardTokensFilter",
  components: {
    MerchantTerminalFilter: () => import("../filtering/MerchantTerminalFilter"),
  },
  data() {
    return {
      model: { ...this.filterData },
      vr: ValidationRules,
      formIsValid: true,
    };
  },
  async mounted() {
    
  },
  props: {
    filterData: {
      type: Object
    }
  },
  methods: {
    apply() {
      if(!this.$refs.form.validate()){
        return;
      }
      this.$emit("apply", this.model);
    }
  }
};
</script>