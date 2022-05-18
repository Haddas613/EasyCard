<template>
  <v-container fluid>
    <v-form v-model="formIsValid" ref="form">
      <v-row>
        <merchant-terminal-filter class="pt-3" v-model="model"></merchant-terminal-filter>
        <!-- <terminal-template-filter md="4" class="pt-3" v-model="model"></terminal-template-filter> -->
        <date-from-to-filter class="px-3 pt-3" v-model="model"></date-from-to-filter>
      </v-row>
      <!-- <v-row no-gutters>
        <v-switch
          class="mt-2"
          v-model="model.success" 
          persistent-hint
          :hint="$t('SuccessfulTransmissionsSwitchTip')"
          color="success">
          <template v-slot:label>
            <small>{{$t('Successful')}}</small>
          </template>
        </v-switch>
      </v-row> -->
      <v-row no-gutters>
        <v-radio-group class="mt-4" :column="false" v-model="model.success">
          <v-radio
            class="px-2"
            :label="$t('Successful')"
            color="success"
            :value="true"
          ></v-radio>
          <v-radio
            class="px-2"
            :label="$t('Error')"
            color="error"
            :value="false"
          ></v-radio>
        </v-radio-group>
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
  name: "TransmissionsFilter",
  components: {
    MerchantTerminalFilter: () => import("../filtering/MerchantTerminalFilter"),
    TerminalTemplateFilter: () => import("../filtering/TerminalTemplateFilter"),
    DateFromToFilter: () => import("../filtering/DateFromToFilter"),
  },
  data() {
    return {
      model: { ...this.filterData },
      dictionaries: {},
      formIsValid: true,
      vr: ValidationRules
    };
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