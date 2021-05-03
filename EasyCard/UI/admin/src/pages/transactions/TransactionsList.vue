<template>
  <v-card class="mx-auto" outlined>
    <v-expansion-panels :flat="true">
      <v-expansion-panel>
        <v-expansion-panel-header>
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
          :header-props="{ sortIcon: null }"
          class="elevation-1">
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
        <template v-slot:item.transactionAmount="{ item }">
          <p class="text-right">
            <b>{{item.transactionAmount | currency(item.currency)}}</b>
          </p>
        </template>  
        <template v-slot:item.quickStatus="{ item }">
          <span v-bind:class="quickStatusesColors[item.quickStatus]">{{item.quickStatus}}</span>
        </template> 
        <template v-slot:item.actions="{ item }">
          <v-btn color="primary" outlined small link :to="{name: 'Transaction', params: {id: item.$paymentTransactionID}}">
            <re-icon small>mdi-arrow-right</re-icon>
          </v-btn>
        </template>    
        <template v-slot:item.cardPresence="{ item }">
          <span :title="item.cardPresence">
            <v-icon v-if="item.$cardPresence == 'regular'" color="success">mdi-credit-card-check</v-icon>
            <v-icon v-else>mdi-credit-card-off-outline</v-icon>
          </span>
        </template>  
        <template v-slot:item.transactionType="{ item }">
          <span :title="item.transactionType">
            <v-icon v-if="item.$transactionType == 'regularDeal'" color="primary">mdi-cash</v-icon>
            <v-icon v-else-if="item.$transactionType == 'installments'" color="accent">mdi-credit-card-check</v-icon>
            <v-icon v-else color="secondary">mdi-credit-card-outline</v-icon>
          </span>
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
  props: {
    filters: {
      default: () => {
        return {}
      },
    },
  },
  data() {
    return {
      totalAmount: 0,
      transactions: [],
      loading: true,
      options: {},
      pagination: {},
      headers: [],
      transactionsFilter: {
        ...this.filters
      },
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
        this.headers = [...data.headers, { value: "actions", text: this.$t("Actions"), sortable: false  }];
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