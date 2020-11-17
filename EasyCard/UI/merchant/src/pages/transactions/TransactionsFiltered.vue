<template>
  <v-flex>
    <transactions-filter-dialog
      :show.sync="showDialog"
      :filter="transactionsFilter"
      v-on:ok="applyFilters($event)"
    ></transactions-filter-dialog>
    <v-card class="my-2" width="100%" flat>
      <v-card-title class="pb-0">
        <v-row class="py-0" no-gutters>
          <v-col cols="8">{{$t("Overview")}}</v-col>
          <v-col cols="3" class="text-end">
            <v-btn
              class="button"
              color="primary"
              outlined
              @click="showDialog = true;"
            >{{$t('Filter')}}</v-btn>
          </v-col>
          <v-col cols="1" class="text-end">
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
              <v-col cols="12">{{$t("OperationsCountTotal")}}:</v-col>
              <v-col cols="12" class="font-weight-bold">
                {{numberOfRecords || '-'}}
              </v-col>
            </v-row>
          </v-col>
        </v-row>
      </v-card-text>
    </v-card>
    <v-card width="100%" flat :loading="!transactions">
      <v-card-text class="px-0">
        <transactions-list :key="loading" :transactions="transactions"></transactions-list>
        <v-flex class="text-center" v-if="canLoadMore">
          <v-btn outlined color="primary" :loading="loading" @click="loadMore()">{{$t("LoadMore")}}</v-btn>
        </v-flex>
      </v-card-text>
    </v-card>
  </v-flex>
</template>

<script>
import moment from "moment";

export default {
  components: {
    EcList: () => import("../../components/ec/EcList"),
    ReIcon: () => import("../../components/misc/ResponsiveIcon"),
    TransactionsList: () => import("../../components/transactions/TransactionsList"),
    TransactionsFilterDialog: () =>
      import("../../components/transactions/TransactionsFilterDialog"),
    EcDialogInvoker: () => import("../../components/ec/EcDialogInvoker")
  },
  props: {
    filters: {
      default: null
    },
    showFiltersDialog: {
      type: Boolean,
      default: false,
      required: false
    }
  },
  data() {
    return {
      transactions: null,
      customerInfo: null,
      moment: moment,
      loading: false,
      transactionsFilter: {
        ...this.filters
      },
      defaultFilter: {
        take: 100,
        skip: 0,
        jDealType: "J4"
      },
      showDialog: this.showFiltersDialog,
      datePeriod: null,
      numberOfRecords: 0
    };
  },
  methods: {
    async getDataFromApi(extendData) {
      this.loading = true;
      let data = await this.$api.transactions.get({
        ...this.transactionsFilter
      });
      if (data) {
        let transactions = data.data || [];
        this.transactions = extendData ? [...this.transactions, ...transactions] : transactions;
        this.numberOfRecords = data.numberOfRecords || 0;

        if(transactions.length > 0){
          let newest = this.transactions[0].$transactionTimestamp;
          let oldest = this.transactions[this.transactions.length - 1].$transactionTimestamp;
          this.datePeriod = this.$options.filters.ecdate(oldest, "L") +  ` - ${this.$options.filters.ecdate(newest, "L")}`;
        }else{
          this.datePeriod = null;
        }
      }
      this.loading = false;
    },
    async applyFilters(data) {
      this.transactionsFilter = {
        ...this.defaultFilter,
        ...data
      };
      await this.getDataFromApi();
    },
    async refresh(){
      this.transactionsFilter.skip = 0;
      await this.getDataFromApi();
    },
    async loadMore() {
      this.transactionsFilter.skip += this.transactionsFilter.take;
      await this.getDataFromApi(true);
    }
  },
  computed: {
    canLoadMore() {
      return this.numberOfRecords > 0 
        && (this.transactionsFilter.take + this.transactionsFilter.skip) < this.numberOfRecords;
    }
  },
  async mounted() {
    await this.applyFilters();

    this.$store.commit("ui/changeHeader", {
      value: {
        threeDotMenu: [
          {
            text: this.$t("Charge"),
            fn: () => {
              this.$router.push({ name: "Charge" });
            }
          }
        ]
      }
    });
  }
};
</script>

<style lang="scss" scoped>
</style>