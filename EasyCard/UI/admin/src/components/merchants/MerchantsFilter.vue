<template>
  <v-container fluid class="py-0">
    <v-row>
      <v-col cols="12">
        <v-text-field outlined hide-details="true" v-model="model.search" :label="$t('Search')"></v-text-field>
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
  name: "MerchantsFilter",
  components: {
    MerchantTerminalFilter: () => import("../filtering/MerchantTerminalFilter")
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
      this.$emit("apply", this.model);
    }
  }
};
</script>