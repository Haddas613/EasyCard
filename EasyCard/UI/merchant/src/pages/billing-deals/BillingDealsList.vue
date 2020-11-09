<template>
  <v-flex>
    <billing-deals-filter-dialog
      :show.sync="showDialog"
      :filter="billingDealsFilter"
      v-on:ok="applyFilters($event)"
    ></billing-deals-filter-dialog>
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
    <v-card width="100%" flat :loading="!billingDeals">
      <v-card-text class="px-0">
        <ec-list :items="billingDeals" v-if="billingDeals">
          <template v-slot:prepend="{ item }">
            <v-tooltip top v-if="item.billingSchedule">
              <template v-slot:activator="{ on, attrs }">
                <v-btn color="primary" dark icon v-bind="attrs" v-on="on">
                  <v-icon>mdi-calendar</v-icon>
                </v-btn>
              </template>
              <billing-schedule-string
                :schedule="item.billingSchedule"
                replacement-text="ScheduleIsNotDefined"
              ></billing-schedule-string>
            </v-tooltip>
            <v-icon v-else>mdi-calendar</v-icon>
          </template>

          <template v-slot:left="{ item }">
            <v-col cols="12" md="6" lg="6" class="pt-1 caption ecgray--text">{{item.billingDealID}}</v-col>
            <v-col
              cols="12"
              md="6"
              lg="6"
            >{{item.$billingDealTimestamp | ecdate('DD/MM/YYYY HH:mm')}}</v-col>
          </template>

          <template v-slot:right="{ item }">
            <v-col cols="12" md="6" lg="6" class="text-end body-2">
              <v-badge
                inline
                color="primary"
                :content="item.numberOfPayments || '...'"
              >{{item.currency}}{{item.transactionAmount}}</v-badge>
            </v-col>
            <v-col
              cols="12"
              md="6"
              lg="6"
              class="text-end font-weight-bold button"
            >{{item.currency}}{{item.totalAmount}}</v-col>
          </template>

          <template v-slot:append="{ item }">
            <v-btn icon :to="{ name: 'BillingDeal', params: { id: item.$billingDealID } }">
              <re-icon>mdi-chevron-right</re-icon>
            </v-btn>
          </template>
        </ec-list>
        <p
          class="ecgray--text text-center"
          v-if="billingDeals && billingDeals.length === 0"
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

export default {
  components: {
    EcList: () => import("../../components/ec/EcList"),
    ReIcon: () => import("../../components/misc/ResponsiveIcon"),
    BillingDealsFilterDialog: () =>
      import("../../components/billing-deals/BillingDealsFilterDialog"),
    BillingScheduleString: () => import("../../components/billing-deals/BillingScheduleString"),
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
      billingDeals: null,
      quickStatusesColors: {
        Pending: "ecgray--text",
        None: "",
        Completed: "success--text",
        Failed: "error--text"
      },
      customerInfo: null,
      moment: moment,
      loading: false,
      billingDealsFilter: {
        take: 100,
        skip: 0,
        ...this.filters
      },
      showDialog: this.showFiltersDialog,
      datePeriod: null,
      numberOfRecords: 0
    };
  },
  methods: {
    async getDataFromApi(extendData) {
      this.loading = true;
      let data = await this.$api.billingDeals.get({
        ...this.billingDealsFilter
      });
      if (data) {
        let billingDeals = data.data || [];
        this.billingDeals = extendData
          ? [...this.billingDeals, ...billingDeals]
          : billingDeals;
        this.numberOfRecords = data.numberOfRecords || 0;

        if (billingDeals.length > 0) {
          let newest = this.billingDeals[0].$billingDealTimestamp;
          let oldest = this.billingDeals[this.billingDeals.length - 1]
            .$billingDealTimestamp;
          this.datePeriod =
            this.$options.filters.ecdate(oldest, "L") + ` - ${this.$options.filters.ecdate(newest, "L")}`;
        } else {
          this.datePeriod = null;
        }
      }
      this.loading = false;
    },
    async applyFilters(data) {
      this.billingDealsFilter = {
        ...data,
        skip: 0,
        take: 100
      };
      await this.getDataFromApi();
    },
    async refresh() {
      this.billingDealsFilter.skip = 0;
      await this.getDataFromApi();
    },
    async loadMore() {
      this.billingDealsFilter.skip += this.billingDealsFilter.take;
      await this.getDataFromApi(true);
    }
  },
  computed: {
    canLoadMore() {
      return (
        this.numberOfRecords > 0 &&
        this.billingDealsFilter.take + this.billingDealsFilter.skip <
          this.numberOfRecords
      );
    }
  },
  async mounted() {
    await this.getDataFromApi();

    this.$store.commit("ui/changeHeader", {
      value: {
        threeDotMenu: [
          {
            text: this.$t("Create"),
            fn: () => {
              this.$router.push({ name: "CreateBillingDeal" });
            }
          }
        ]
      }
    });
  }
};
</script>