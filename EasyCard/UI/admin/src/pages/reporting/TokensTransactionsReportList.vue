<template>
  <v-card class="mx-auto" outlined>
    <v-expansion-panels :flat="true">
      <v-expansion-panel>
        <v-expansion-panel-header>{{$t('Filters')}}</v-expansion-panel-header>
        <v-expansion-panel-content>
          <tokens-transactions-filter  :filter-data="tokensFilter" v-on:apply="applyFilter($event)"></tokens-transactions-filter>
        </v-expansion-panel-content>
      </v-expansion-panel>
    </v-expansion-panels>
    <v-divider></v-divider>
    <div>
      <v-data-table
        :headers="headers"
        :items="tokens"
        :options.sync="options"
        :server-items-length="numberOfRecords"
        :loading="loading"
        :header-props="{ sortIcon: null }"
        class="elevation-1"
      >
        <template v-slot:item.merchantName="{ item }">
          <router-link class="text-decoration-none" link :to="{name: 'Merchant', params: {id: item.merchantID}}">
            {{item.merchantName || item.merchantID}}
          </router-link>
        </template>    
        <template v-slot:item.terminalName="{ item }">
          <router-link class="text-decoration-none" link :to="{name: 'EditTerminal', params: {id: item.terminalID}}">
            {{item.terminalName || item.terminalID}}
          </router-link>
        </template> 
        <template v-slot:item.cardNumber="{ item }">
          <span dir="ltr">{{item.cardNumber}}</span>
        </template>
        <template v-slot:item.expired="{ item }">
          {{$t(item.expired ? "Yes" : "No")}}
        </template>
        <template v-slot:item.productionTransactions="{ item }">
          <b class="success--text">{{item.productionTransactions}}</b>
        </template>
        <template v-slot:item.failedTransactions="{ item }">
          <b class="error--text">{{item.failedTransactions}}</b>
        </template>
        <template v-slot:item.totalSum="{ item }">
          <b>{{item.totalSum | currency(currencyStore.code)}}</b>
        </template>
        <template v-slot:item.totalRefund="{ item }">
          <b>{{item.totalRefund | currency(currencyStore.code)}}</b>
        </template>
      </v-data-table>
    </div>
  </v-card>
</template>

<script>
import moment from "moment";
import { mapState } from "vuex";

export default {
  name: "TokensTransactions",
  components: {
    TokensTransactionsFilter: () => import("../../components/reporting/TokensTransactionsReportFilter")
  },
  data() {
    return {
      numberOfRecords: 0,
      tokens: [],
      loading: false,
      options: {},
      pagination: {},
      headers: [],
      tokensFilter: {
        dateFrom: this.$formatDate(moment().subtract(1, 'months')),
        dateTo: this.$formatDate(moment())
      },
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
      try{
        this.loading = true;
        let data = await this.$api.reporting.cardTokens.getTokensTransactions({
          ...this.tokensFilter,
          ...this.options
        });
        this.tokens = data.data;
        this.numberOfRecords = data.numberOfRecords;

        if (!this.headers || this.headers.length === 0) {
          this.headers = [...data.headers];
        }
      }finally{
        this.loading = false;
      }
    },
    async applyFilter(filter) {
      this.options.page = 1;
      this.tokensFilter = filter;
      await this.getDataFromApi();
    }
  },
  computed: {
    ...mapState({
      currencyStore: state => state.settings.currency
    })
  },
};
</script>