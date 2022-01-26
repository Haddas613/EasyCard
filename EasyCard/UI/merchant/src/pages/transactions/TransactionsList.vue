<template>
  <v-flex>
    <v-card class="mt-2" width="100%" flat>
      <v-card-title class="pb-0">
        <v-row class="py-0" no-gutters>
          <v-col cols="10">{{$t("Overview")}}</v-col>
          <v-col cols="1" class="text-end hidden-sm-and-down">
            <v-btn
              class="button"
              color="primary"
              :to="{name: 'TransactionsFiltered', params:{ showFiltersDialog: true }}"
              outlined
            >{{$t("Filter")}}</v-btn>
          </v-col>
          <v-col cols="2" lg="1" class="text-end">
            <v-btn icon @click="refresh()" :loading="loading">
              <v-icon color="primary">mdi-refresh</v-icon>
            </v-btn>
          </v-col>
        </v-row>
      </v-card-title>
      <v-card-text class="body-2">
        <v-row no-gutters class="py-1">
          <v-col cols="12" md="3" lg="3" xl="3">
            <v-row no-gutters>
              <v-col cols="12">{{$t("PeriodShown")}}:</v-col>
              <v-col cols="12" class="font-weight-bold">
                <span dir="ltr">{{datePeriod || '-'}}</span>
              </v-col>
            </v-row>
          </v-col>
          <v-col cols="12" md="3" lg="3" xl="3">
            <v-row no-gutters>
              <v-col cols="12">{{$t("OperationsCount")}}:</v-col>
              <v-col cols="12" class="font-weight-bold">
                <span
                  v-if="totalOperationsCountShown"
                >({{$t("@Displayed").replace("@amount", totalOperationsCountShown)}})</span>
              </v-col>
            </v-row>
          </v-col>
        </v-row>
      </v-card-text>
      <v-card-actions class="hidden-md-and-up">
        <v-btn
          class="button"
          color="primary"
          :to="{name: 'TransactionsFiltered', params:{ showFiltersDialog: true }}"
          block
          outlined
        >{{$t("Filter")}}</v-btn>
      </v-card-actions>
    </v-card>

    <v-card
      class="mt-4"
      width="100%"
      flat
      v-for="groupedTransaction in groupedTransactions"
      v-bind:key="groupedTransaction.groupValue.transactionDate"
    >
      <v-card-title class="subtitle-2">{{groupedTransaction.groupValue.transactionDate | ecdate}}</v-card-title>
      <v-card-text class="px-0">
        <transactions-list-component :transactions="groupedTransaction.data"></transactions-list-component>
        <!-- TODO: config -->
        <v-card-actions
          class="justify-center"
          v-if="groupedTransaction.groupValue.numberOfRecords > 10"
        >
          <router-link
            class="primary--text"
            link
            :to="{name: 'TransactionsFiltered', params: {filters: getTransactionDayFilters(groupedTransaction.groupValue)}}"
          >{{$t("SeeMore")}} {{groupedTransaction.groupValue.numberOfRecords}}</router-link>
        </v-card-actions>
      </v-card-text>
    </v-card>
  </v-flex>
</template>

<script>
import moment from "moment";
import { mapState } from "vuex";

export default {
  name: "TransactionsList",
  components: {
    EcList: () => import("../../components/ec/EcList"),
    ReIcon: () => import("../../components/misc/ResponsiveIcon"),
    EcMoney: () => import("../../components/ec/EcMoney"),
    TransactionsListComponent: () =>
      import("../../components/transactions/TransactionsList")
  },
  data() {
    return {
      totalAmount: 0,
      groupedTransactions: [],
      options: {},
      pagination: {},
      headers: [],
      transactionsFilter: {
        take: 100,
        skip: 0,
        terminalID: null,
      },
      dictionaries: {},
      datePeriod: null,
      totalOperationsCount: null,
      totalOperationsCountShown: null,
      loading: false,
      moment
    };
  },
  methods: {
    async applyFilter(filter) {
      this.transactionsFilter = {
        ...this.transactionsFilter,
        ...filter,
      };
      await this.getGroupedDataFromApi();
    },
    async refresh() {
      this.totalOperationsCount = null;
      this.totalOperationsCountShown = null;
      this.datePeriod = null;
      this.groupedTransactions = [];
      await this.getGroupedDataFromApi();
    },
    async getGroupedDataFromApi() {
      if(this.loading) { return; }
      this.loading = true;
      let data = await this.$api.transactions.getGrouped({
        ...this.transactionsFilter,
        ...this.options
      });
      if (data && data.length > 0) {
        this.groupedTransactions = this.groupedTransactions.concat(data);
        let newest = this.groupedTransactions[0].groupValue.transactionDate;
        let oldest =
          this.groupedTransactions.length > 1
            ? this.groupedTransactions[this.groupedTransactions.length - 1]
                .groupValue.transactionDate
            : null;
        this.totalOperationsCount = this.lodash.sumBy(
          this.groupedTransactions,
          t => t.groupValue.numberOfRecords
        );
        this.totalOperationsCountShown = this.lodash.sumBy(
          this.groupedTransactions,
          t => t.data.length
        );
        this.datePeriod =
          this.$options.filters.ecdate(oldest, "L") + ` - ${this.$options.filters.ecdate(newest, "L")}`;
      }
      this.loading = false;
    },
    getTransactionDayFilters(transaction) {
      let dateFrom = this.moment(transaction.transactionDate)
        .startOf("day")
        .format();

      let dateTo = this.moment(transaction.transactionDate)
        .endOf("day")
        .format();

      return { dateFrom, dateTo };
    },
    async exportExcel() {
      let operation = await this.$api.transactions.getExcel({
        ...this.transactionsFilter,
        ...this.options
      });
      if(!this.$apiSuccess(operation)) return;
      window.open(operation.entityReference, "_blank");
    }
  },
  computed: {
    ...mapState({
      terminalStore: state => state.settings.terminal
    }),
  },
  async mounted() {
    await this.applyFilters({
      terminalID: this.terminalStore.terminalID,
    });

    this.$store.commit("ui/changeHeader", {
      value: {
        threeDotMenu: [
          {
            text: this.$t("Charge"),
            fn: () => {
              this.$router.push({ name: "Charge" });
            }
          },
          {
            text: this.$t("Refund"),
            fn: () => {
              this.$router.push({ name: "Refund" });
            }
          },
          {
            text: this.$t("Excel"),
            fn: () => {
              this.exportExcel();
            }
          }
        ]
      }
    });
  }
};
</script>
