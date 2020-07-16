<template>
  <v-card width="100%" outlined>
    <v-card-title>{{$t('Transactions')}}</v-card-title>

    <!-- TODO: REMOVE -->
    <v-expansion-panels :flat="true" v-if="false">
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
    <div v-if="false">
      <v-data-table
        :headers="headers"
        :items="transactions"
        :options.sync="options"
        :server-items-length="totalAmount"
        :loading="loading"
        class="elevation-1"
      ></v-data-table>
    </div>

    <ec-list :items="transactions">
      <template v-slot:prepend>
        <v-icon>mdi-credit-card-outline</v-icon>
      </template>

      <template v-slot:left="{ item }">
        <v-col cols="12" md="6" lg="6" class="pt-1 caption ecgray--text">{{item.paymentTransactionID}}</v-col>
        <v-col cols="12" md="6" lg="6">{{item.cardOwnerName}}</v-col>
      </template>

      <template v-slot:right="{ item }">
        <v-col cols="12" md="6" lg="6" class="text-end body-2" v-bind:class="quickStatusesColors[item.quickStatus]">{{$t(item.quickStatus)}}</v-col>
        <v-col cols="12" md="6" lg="6" class="text-end font-weight-bold button">{{item.currency}}{{item.transactionAmount}}</v-col>
      </template>

      <template v-slot:append>
        <v-icon>mdi-chevron-right</v-icon>
      </template>
    </ec-list>
  </v-card>
</template>

<script>
import TransactionsFilter from "../../components/transactions/TransactionsFilter";
import EcList from "../../components/ec/EcList";

export default {
  name: "TransactionsList",
  components: { TransactionsFilter, EcList },
  data() {
    return {
      totalAmount: 0,
      transactions: [],
      loading: true,
      options: {},
      pagination: {},
      headers: [],
      transactionsFilter: {},
      dictionaries: {},
      quickStatusesColors: {
        "Pending": "ecgray--text",
        "None": "",
        "Completed": "success--text",
        "Failed": "error--text"
      }
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
  },
  async mounted() {
    await this.getDataFromApi();
  }
};
</script>
