<template>
  <v-flex>
    <v-card width="100%" flat class="hidden-sm-and-down">
      <v-card-title>{{$t('Transactions')}}</v-card-title>
    </v-card>

    <v-card class="mt-4" width="100%" flat v-for="groupedTransaction in groupedTransactions" v-bind:key="groupedTransaction.groupValue.transactionDate">
      <v-card-title class="subtitle-2">{{groupedTransaction.groupValue.transactionDate | ecdate}}</v-card-title>
      <v-card-text class="px-0">
        <ec-list :items="groupedTransaction.data">
          <template v-slot:prepend>
            <v-icon>mdi-credit-card-outline</v-icon>
          </template>

          <template v-slot:left="{ item }">
            <v-col
              cols="12"
              md="6"
              lg="6"
              class="pt-1 caption ecgray--text"
            >{{item.paymentTransactionID}}</v-col>
            <v-col cols="12" md="6" lg="6">{{item.cardOwnerName}}</v-col>
          </template>

          <template v-slot:right="{ item }">
            <v-col
              cols="12"
              md="6"
              lg="6"
              class="text-end body-2"
              v-bind:class="quickStatusesColors[item.quickStatus]"
            >{{$t(item.quickStatus)}}</v-col>
            <v-col
              cols="12"
              md="6"
              lg="6"
              class="text-end font-weight-bold button"
            >{{item.currency}}{{item.transactionAmount}}</v-col>
          </template>

          <template v-slot:append>
            <v-icon>mdi-chevron-right</v-icon>
          </template>
        </ec-list>
      </v-card-text>
    </v-card>
  </v-flex>
</template>

<script>
import TransactionsFilter from "../../components/transactions/TransactionsFilter";
import EcList from "../../components/ec/EcList";
import moment from "moment";

export default {
  name: "TransactionsList",
  components: { TransactionsFilter, EcList },
  data() {
    return {
      totalAmount: 0,
      transactions: [],
      groupedTransactions: {},
      loading: true,
      options: {},
      pagination: {},
      headers: [],
      transactionsFilter: {},
      dictionaries: {},
      quickStatusesColors: {
        Pending: "ecgray--text",
        None: "",
        Completed: "success--text",
        Failed: "error--text"
      },
      moment: moment
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
    /**todo: obsolete, remove */
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
    },
    async getGroupedDataFromApi(){
      let timeout = setTimeout(
        (() => {
          this.loading = true;
        }).bind(this),
        1000
      );
      let data = await this.$api.transactions.getGrouped({
        ...this.transactionsFilter,
        ...this.options
      });
      if (data) {
        this.groupedTransactions = data;

        if (!this.headers || this.headers.length === 0) {
          this.headers = data.headers;
        }
      }
      clearTimeout(timeout);
      this.loading = false;
    }
  },
  async mounted() {
    await this.getGroupedDataFromApi();
  }
};
</script>
