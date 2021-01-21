<template>
  <v-container fluid>
    <v-row>
      <!-- <merchant-terminal-filter v-model="model"></merchant-terminal-filter> -->
      <v-col cols="12" md="10">
        <v-text-field
          v-model="model.label"
          :label="$t('Label')"
          outlined
          hide-details="true"
        ></v-text-field>
      </v-col>
      <v-col cols="12" md="2">
        <v-switch
          class="pt-0 mt-4 px-2"
          v-model="model.active"
          :label="$t('OnlyActive')"
          hide-details="true"
        ></v-switch>
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
  name: "TerminalTemplatesFilter",
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