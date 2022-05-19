<template>
  <v-container fluid>
    <v-form ref="form" v-model="formIsValid">
      <v-row>
        <merchant-terminal-filter class="pt-3" v-model="model"></merchant-terminal-filter>
        <terminal-template-filter class="pt-3" v-model="model" md="4"></terminal-template-filter>
        <v-col cols="12" md="4" class="pb-0">
          <v-select
            :items="dictionaries.terminalStatusEnum"
            item-text="description"
            item-value="code"
            v-model="model.status"
            :label="$t('Status')"
            clearable
          ></v-select>
        </v-col>
        <v-col cols="12" md="4" class="pb-0">
          <v-text-field
            v-model="model.aggregatorTerminalReference"
            :label="$t('AggregatorTerminalReference')"
          ></v-text-field>
        </v-col>
        <v-col cols="12" md="4" class="pb-0">
          <v-text-field
            v-model="model.processorTerminalReference"
            :label="$t('ProcessorTerminalReference')"
          ></v-text-field>
        </v-col>
        <v-col cols="12" md="4" class="pb-0">
          <v-select
            :items="dictionaries.dateFilterTypeEnum"
            item-text="description"
            item-value="code"
            v-model="model.dateType"
            :label="$t('DateType')"
            clearable
          ></v-select>
        </v-col>
        <date-from-to-filter class="px-3 pt-3" v-model="model"></date-from-to-filter>
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
  name: "TerminalsFilter",
  components: {
    MerchantTerminalFilter: () => import("../filtering/MerchantTerminalFilter"),
    TerminalTemplateFilter: () => import("../filtering/TerminalTemplateFilter"),
    DateFromToFilter: () => import("../filtering/DateFromToFilter"),
  },
  data() {
    return {
      model: { ...this.filterData },
      dictionaries: {},
      vr: ValidationRules,
      formIsValid: true,
    };
  },
  async mounted() {
    this.dictionaries = await this.$api.dictionaries.getMerchantDictionaries();
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
      if(!this.model.statuses){
        this.model.statuses = null; //fix empty string
      }
      this.$emit("apply", this.model);
    }
  }
};
</script>