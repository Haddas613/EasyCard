<template>
  <v-flex>
    <future-billing-deals-filter-dialog
      :show.sync="showDialog"
      :filter="futureBillingDealsFilter"
      v-on:ok="applyFilters($event)"
    ></future-billing-deals-filter-dialog>
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
              <v-col cols="12" class="font-weight-bold">{{numberOfRecords || '-'}}</v-col>
            </v-row>
          </v-col>
        </v-row>
      </v-card-text>
    </v-card>
    <v-card width="100%" flat :loading="!futureBillingDeals">
      <v-card-text class="px-0 pt-0">
        <ec-list :items="futureBillingDeals" v-if="futureBillingDeals">
          <template v-slot:prepend="{ item }">
            <small class="secondary--text">
              <b>{{item.billingDealID}}</b>
            </small>
          </template>

          <template v-slot:left="{ item }">
            <v-col
              cols="12"
              md="6"
              lg="6"
              class="pt-1 caption ecgray--text"
            >
            <b v-if="item.$futureScheduledTransaction" 
              v-bind:class="{'error--text': (item.$futureScheduledTransaction > now)}">
              {{item.$futureScheduledTransaction | ecdate('DD/MM/YYYY')}}
            </b>
            <span v-else>-</span>
            </v-col>
            <v-col cols="12" md="6" lg="6">{{item.cardOwnerName || '-'}}</v-col>
          </template>

          <template v-slot:right="{ item }">
            <v-col
              cols="12"
              md="6"
              lg="6"
              class="text-end body-2"
              v-bind:class="{'ecred--text': item.cardExpired}"
            >
              {{item.futureDeal || '-'}}
            </v-col>
            <v-col
              cols="12"
              md="6"
              lg="6"
              class="text-end font-weight-bold button"
            >{{item.transactionAmount | currency(item.$currency)}}</v-col>
          </template>

          <template v-slot:append="{ item }">
            <v-btn icon :to="{ name: 'BillingDeal', params: { id: item.$billingDealID } }">
              <re-icon>mdi-chevron-right</re-icon>
            </v-btn>
          </template>
        </ec-list>
        <p
          class="ecgray--text text-center"
          v-if="futureBillingDeals && futureBillingDeals.length === 0"
        >{{$t("NothingToShow")}}</p>

        <v-flex class="text-center" v-if="canLoadMore">
          <v-btn outlined color="primary" :loading="loading" @click="loadMore()">{{$t("LoadMore")}}</v-btn>
        </v-flex>
      </v-card-text>
    </v-card>
  </v-flex>
</template>

<script>
import moment from "moment";
import { mapState } from "vuex";

export default {
  components: {
    EcList: () => import("../../components/ec/EcList"),
    ReIcon: () => import("../../components/misc/ResponsiveIcon"),
    FutureBillingDealsFilterDialog: () =>
      import("../../components/future-billing-deals/FutureBillingDealsFilterDialog"),
    BillingScheduleString: () =>
      import("../../components/billing-deals/BillingScheduleString"),
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
      futureBillingDeals: null,
      quickStatusesColors: {
        Pending: "ecgray--text",
        None: "",
        Completed: "success--text",
        Failed: "error--text"
      },
      customerInfo: null,
      moment: moment,
      loading: false,
      futureBillingDealsFilter: {
        take: 100,
        skip: 0,
        dateFrom: this.$formatDate(moment()),
        dateTo: this.$formatDate(moment().add(1, 'y')),
        ...this.filters
      },
      showDialog: this.showFiltersDialog,
      datePeriod: null,
      numberOfRecords: 0,
      selectAll: false,
      now: new Date()
    };
  },
  methods: {
    async getDataFromApi(extendData) {
      this.loading = true;
      let data = await this.$api.futureBillingDeals.get({
        ...this.futureBillingDealsFilter
      });
      if (data) {
        let futureBillingDeals = data.data || [];
        this.futureBillingDeals = extendData
          ? [...this.futureBillingDeals, ...futureBillingDeals]
          : futureBillingDeals;
        this.numberOfRecords = data.numberOfRecords || 0;

        if (futureBillingDeals.length > 0) {
          let newest = this.futureBillingDeals[0].$futureScheduledTransaction;
          let oldest = this.futureBillingDeals[this.futureBillingDeals.length - 1]
            .$futureScheduledTransaction;
          this.datePeriod =
            this.$options.filters.ecdate(newest, "L") +
            ` - ${this.$options.filters.ecdate(oldest, "L")}`;
        } else {
          this.datePeriod = null;
        }
      }
      this.selectAll = false;
      this.loading = false;
    },
    async applyFilters(data) {
      this.futureBillingDealsFilter = {
        ...data
      };
      await this.getDataFromApi();
    },
    async refresh() {
      this.futureBillingDealsFilter.skip = 0;
      await this.getDataFromApi();
    },
    async loadMore() {
      this.futureBillingDealsFilter.skip += this.futureBillingDealsFilter.take;
      await this.getDataFromApi(true);
    },
  },
  computed: {
    canLoadMore() {
      return (
        this.numberOfRecords > 0 &&
        this.futureBillingDealsFilter.take + this.futureBillingDealsFilter.skip <
          this.numberOfRecords
      );
    },
    ...mapState({
      terminalStore: state => state.settings.terminal
    })
  },
  async mounted() {
    await this.getDataFromApi();
  }
};
</script>