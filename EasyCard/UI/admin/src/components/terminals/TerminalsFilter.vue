<template>
  <v-container fluid>
    <v-row>
      <!-- <merchant-terminal-filter v-model="model"></merchant-terminal-filter> -->
      <v-col cols="12" md="4">
        <v-text-field
          v-model="model.terminalID"
          :label="$t('TerminalID')"
          outlined
          hide-details="true"
        ></v-text-field>
      </v-col>
      <v-col cols="12" md="4">
        <v-select
          :items="dictionaries.terminalStatusEnum"
          item-text="description"
          item-value="code"
          v-model="model.status"
          :label="$t('Status')"
          outlined
          hide-details="true"
          clearable
        ></v-select>
      </v-col>
      <v-col cols="12" md="4">
        <v-text-field
          v-model="model.label"
          :label="$t('Label')"
          outlined
          hide-details="true"
        ></v-text-field>
      </v-col>
      <v-col cols="12" md="4">
        <v-text-field
          v-model="model.merchantID"
          :label="$t('MerchantID')"
          outlined
          hide-details="true"
        ></v-text-field>
      </v-col>
      <v-col cols="12" md="4">
        <v-text-field
          v-model="model.aggregatorTerminalReference"
          :label="$t('AggregatorTerminalReference')"
          outlined
          hide-details="true"
        ></v-text-field>
      </v-col>
      <v-col cols="12" md="4">
        <v-text-field
          v-model="model.processorTerminalReference"
          :label="$t('ProcessorTerminalReference')"
          outlined
          hide-details="true"
        ></v-text-field>
      </v-col>
    </v-row>
    <v-row>
      <v-col cols="12" class="d-flex justify-end">
        <v-btn color="success" class="mr-4" @click="apply()">{{$t('Apply')}}</v-btn>
      </v-col>
    </v-row>
  </v-container>
</template>

<script>
export default {
  name: "TerminalsFilter",
  components: {
    MerchantTerminalFilter: () => import("../filtering/MerchantTerminalFilter"),
  },
  data() {
    return {
      model: { ...this.filterData },
      dictionaries: {}
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
      if(!this.model.statuses){
        this.model.statuses = null; //fix empty string
      }
      this.$emit("apply", this.model);
    }
  }
};
</script>