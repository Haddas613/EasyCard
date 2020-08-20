<template>
  <v-card class="mx-auto" outlined>
    <v-card-title>{{$t('MerchantsList')}}</v-card-title>
    <v-expansion-panels :flat="true">
      <v-expansion-panel>
       <v-expansion-panel-header class="primary white--text">
          {{$t('Filters')}}
        </v-expansion-panel-header>
        <v-expansion-panel-content>
          <div class="pt-4 pb-2">
            <merchants-filter :filter-data="options" v-on:apply="applyFilter($event)"></merchants-filter>
          </div>
        </v-expansion-panel-content>
      </v-expansion-panel>
    </v-expansion-panels>
    <v-divider></v-divider>
    <div>
      <v-data-table 
          :headers="headers"
          :items="merchants"
          :options.sync="options"
          :server-items-length="totalAmount"
          :loading="loading"
          class="elevation-1"></v-data-table>
    </div>
  </v-card>
</template>

<script>
import MerchantsFilter from '../../components/merchants/MerchantsFilter';
export default {
  name: "MerchantsList",
  components: {MerchantsFilter},
  data() {
    return {
      totalAmount: 0,
      merchants: [],
      loading: true,
      options: {},
      pagination: {},
      headers: [],
      merchantsFilter: {}
    }
  },
  watch: {
    options: {
      handler: async function(){ await this.getDataFromApi() },
      deep: true
    }
  },
  methods: {
    async getDataFromApi() {
      this.loading = true;
      let data = await this.$api.merchants.get({ ...this.merchantsFilter, ...this.options });
      this.merchants = data.data;
      this.totalAmount = data.numberOfRecords;
      this.loading = false;

      if(!this.headers || this.headers.length === 0){
        this.headers = data.headers;
      }
    },
    async applyFilter(filter){
      this.merchantsFilter = filter;
      await this.getDataFromApi();
    }
  }
};
</script>