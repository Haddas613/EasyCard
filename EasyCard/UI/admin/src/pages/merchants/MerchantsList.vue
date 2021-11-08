<template>
  <v-card class="mx-auto" outlined>
    <v-expansion-panels :flat="true">
      <v-expansion-panel>
        <v-expansion-panel-header >{{$t('Filters')}}</v-expansion-panel-header>
        <v-expansion-panel-content>
          <merchants-filter :filter-data="merchantsFilter" v-on:apply="applyFilter($event)"></merchants-filter>
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
        :header-props="{ sortIcon: null }"
        class="elevation-1"
      >
        <template v-slot:item.actions="{ item }">
          <v-btn color="primary" outlined small link :to="{name: 'Merchant', params: {id: item.$merchantID}}">
            <re-icon small>mdi-arrow-right</re-icon>
          </v-btn>
          <!-- <router-link class="text-decoration-none" link :to="{name: 'EditMerchant', params: {id: item.$merchantID}}">
              <v-icon small color="secondary" class="mr-2">mdi-pencil</v-icon>
          </router-link>
          <router-link class="text-decoration-none" link :to="{name: 'Merchant', params: {id: item.$merchantID}}">
              <v-icon small color="primary" class="mr-2">mdi-eye</v-icon>
          </router-link> -->
          <!-- <v-icon small @click="deleteItem(item)">mdi-delete</v-icon> -->
        </template>
      </v-data-table>
    </div>
  </v-card>
</template>

<script>
export default {
  name: "Merchants",
  components: {
    ReIcon: () => import("../../components/misc/ResponsiveIcon"),
    MerchantsFilter: () => import("../../components/merchants/MerchantsFilter"),
  },
  data() {
    return {
      totalAmount: 0,
      merchants: [],
      loading: false,
      options: {},
      headers: [],
      merchantsFilter: {}
    };
  },
  watch: {
    options: {
      handler: async function() {
        await this.getDataFromApi();
      },
      deep: true
    }
  },
  beforeRouteEnter(to, from, next) {
    next(vm => {
      vm.$store.commit("ui/setRefreshHandler", { value: vm.getDataFromApi});
    });
  },
  methods: {
    async getDataFromApi() {
      if(this.loading) { return; }
      this.loading = true;
      try{
        let data = await this.$api.merchants.get({
          ...this.merchantsFilter,
          ...this.options
        });
        this.merchants = data.data;
        this.totalAmount = data.numberOfRecords;

        if (!this.headers || this.headers.length === 0) {
          this.headers = [...data.headers, { value: "actions", text: this.$t("Actions"), sortable: false  }];
        }
      }finally{
         this.loading = false;
      }
    },
    async applyFilter(filter) {
      this.options.page = 1;
      this.merchantsFilter = filter;
      await this.getDataFromApi();
    }
  }
};
</script>