<template>
  <v-card class="mx-auto" outlined>
    <v-card-title>{{$t('Transactions List')}}</v-card-title>
    <v-expansion-panels :flat="true">
      <v-expansion-panel>
        <v-expansion-panel-header class="primary white--text">{{$t('Filters')}}</v-expansion-panel-header>
        <v-expansion-panel-content>
          <div class="pt-4 pb-2">
            <transactions-filter :filter-data="options" v-on:apply="applyFilter($event)"></transactions-filter>
          </div>
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
        class="elevation-1"
      ></v-data-table>
    </div>
  </v-card>
</template>

<script>
import TransactionsFilter from "../../components/transactions/TransactionsFilter";

export default {
  name: "TransactionsList",
  components: { TransactionsFilter },
  data() {
    return {
      totalAmount: 0,
      transactions: [],
      loading: true,
      options: {},
      pagination: {},
      headers: [],
      transactionsFilter: {}
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
  methods: {
    async getDataFromApi() {
      let timeout = setTimeout(
        (() => {
          this.loading = true;
        }).bind(this),
        1000
      );
      let data = await this.$api.transactions.get({
        ...this.transactionsFilter,
        ...this.options
      });
      if (data) {
        this.transactions = data.data || [];
        this.totalAmount = data.numberOfRecords || 0;
        this.loading = false;

        if (!this.headers || this.headers.length === 0) {
          this.headers = data.headers;
        }
      }
      clearTimeout(timeout);
      this.loading = false;
    },
    async applyFilter(filter) {
      this.transactionsFilter = filter;
      await this.getDataFromApi();
    }
  }
};
</script>