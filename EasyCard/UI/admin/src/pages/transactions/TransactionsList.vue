<template>
  <v-card class="mx-auto" outlined>
    <v-expansion-panels :flat="true">
      <v-expansion-panel>
        <v-expansion-panel-header class="primary white--text">
          {{$t('Filters')}}
        </v-expansion-panel-header>
        <v-expansion-panel-content>
          <transactions-filter :filter-data="transactionsFilter" v-on:apply="applyFilter($event)"></transactions-filter>
        </v-expansion-panel-content>
      </v-expansion-panel>
    </v-expansion-panels>
    <v-divider></v-divider>
    <div>
      <v-data-table 
          :headers="headers"
          :items="transactions"
          :options.sync="options"
          :server-items-length="totalAmount"
          :loading="loading"
          class="elevation-1">
        <template v-slot:item.merchantName="{ item }">
          <router-link class="text-decoration-none" link :to="{name: 'Merchant', params: {id: item.merchantID}}">
            {{item.merchantName || item.merchantID}}
          </router-link>
        </template>    
        <template v-slot:item.transactionAmount="{ item }">
          <b>{{item.transactionAmount | currency(item.currency)}}</b>
        </template>  
        <template v-slot:item.quickStatus="{ item }">
          <span v-bind:class="quickStatusesColors[item.quickStatus]">{{item.quickStatus}}</span>
        </template> 
        <template v-slot:item.actions="{ item }">
          <v-btn color="primary" outlined small link :to="{name: 'Transaction', params: {id: item.$paymentTransactionID}}">
            <re-icon small>mdi-arrow-right</re-icon>
          </v-btn>
        </template>    
      </v-data-table>
    </div>
  </v-card>
</template>

<script>
export default {
  name: "TransactionsList",
  components: { 
    TransactionsFilter : () => import("../../components/transactions/TransactionsFilter"), 
    ReIcon: () => import("../../components/misc/ResponsiveIcon") 
  },
  data() {
    return {
      totalAmount: 0,
      transactions: [],
      loading: true,
      options: {},
      pagination: {},
      headers: [],
      transactionsFilter: {},
      quickStatusesColors: {
        Pending: "primary--text",
        None: "ecgray--text",
        Completed: "success--text",
        Failed: "error--text",
        Canceled: "accent--text"
      }
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
      let data = await this.$api.transactions.get({ ...this.transactionsFilter, ...this.options });
      this.transactions = data.data;
      this.totalAmount = data.numberOfRecords;
      this.loading = false;

      if(!this.headers || this.headers.length === 0){
        this.headers = [...data.headers, { value: "actions", text: this.$t("Actions") }];
      }
    },
    async applyFilter(filter){
      this.options.page = 1;
      this.transactionsFilter = filter;
      await this.getDataFromApi();
    }
  }
};
</script>