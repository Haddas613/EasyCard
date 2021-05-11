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
        <v-row no-gutters class="px-1 body-2">
          <v-switch
            v-model="billingDealsFilter.onlyActual"
            @change="getDataFromApi(false)"
            :hint="$t('WhileEnabledYouCanManuallyTriggerTheTransaction')"
            :persistent-hint="true"
          >
            <template v-slot:label>
              <small>{{$t('OnlyActual')}}</small>
            </template>
          </v-switch>
        </v-row>
      </v-card-text>
    </v-card>
    <v-card width="100%" flat :loading="!billingDeals">
      <v-card-text class="px-0 pt-0">
        <ec-list :items="billingDeals" v-if="billingDeals">
          <template v-slot:prepend="{ item }" v-if="billingDealsFilter.onlyActual">
            <div class="px-1">
              <v-checkbox v-model="item.selected" v-if="!item.processed"></v-checkbox>
              <v-icon v-else color="success">mdi-check-circle</v-icon>
            </div>
          </template>
          <template v-slot:prepend="{ item }" v-else>
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

            <v-tooltip top v-if="item.cardExpired">
              <template v-slot:activator="{ on, attrs }">
                <v-btn color="ecred" dark icon v-bind="attrs" v-on="on">
                  <v-icon :title="$t('CreditCardExpired')">mdi-credit-card</v-icon>
                </v-btn>
              </template>
              {{$t('CreditCardHasExpired')}}
            </v-tooltip>
            <v-tooltip top v-else-if="item.active">
              <template v-slot:activator="{ on, attrs }">
                <v-btn color="success" dark icon v-bind="attrs" v-on="on">
                  <v-icon :title="$t('Active')">mdi-check</v-icon>
                </v-btn>
              </template>
              {{$t('Active')}}
            </v-tooltip>
            <v-tooltip top v-else-if="!item.active">
              <template v-slot:activator="{ on, attrs }">
                <v-btn color="ecgray" dark icon v-bind="attrs" v-on="on">
                  <v-icon :title="$t('Inactive')">mdi-check</v-icon>
                </v-btn>
              </template>
              {{$t('Inactive')}}
            </v-tooltip>
          </template>

          <template v-slot:left="{ item }">
            <v-col
              cols="12"
              md="6"
              lg="6"
              class="pt-1 caption ecgray--text"
            >
            <b v-if="item.$nextScheduledTransaction" 
              v-bind:class="{'error--text': (item.$nextScheduledTransaction > now)}">
              {{item.$nextScheduledTransaction | ecdate('DD/MM/YYYY')}}
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
              <v-chip small :color="item.cardExpired ? 'ecred': 'primary'" text-color="white">
                <v-avatar
                  left
                  :color="item.cardExpired ? 'ecred': 'primary'"
                  class="darken-2"
                >{{item.numberOfPayments || '∞'}}</v-avatar>
                {{item.currency}}{{item.transactionAmount}}
              </v-chip>
              <!-- <v-badge
                :color="item.cardExpired ? 'ecred': 'primary'"
                :content="item.numberOfPayments || '...'"
              >{{item.currency}}{{item.transactionAmount}}</v-badge>-->
            </v-col>
            <v-col
              cols="12"
              md="6"
              lg="6"
              class="text-end font-weight-bold button"
              v-bind:class="{'ecred--text': item.cardExpired}"
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
import { mapState } from "vuex";

export default {
  components: {
    EcList: () => import("../../components/ec/EcList"),
    ReIcon: () => import("../../components/misc/ResponsiveIcon"),
    BillingDealsFilterDialog: () =>
      import("../../components/billing-deals/BillingDealsFilterDialog"),
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
        onlyActual: null,
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
            this.$options.filters.ecdate(oldest, "L") +
            ` - ${this.$options.filters.ecdate(newest, "L")}`;
        } else {
          this.datePeriod = null;
        }
      }
      this.selectAll = false;
      this.loading = false;
    },
    async applyFilters(data) {
      this.billingDealsFilter = {
        ...data
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
    },
    async createTransactions() {
      if (!this.billingDealsFilter.onlyActual) {
        return this.$toasted.show(this.$t("PleaseEnableManualModeFirst"), {
          type: "error"
        });
      }

      let billings = this.lodash.filter(this.billingDeals, i => i.selected);
      if (billings.length === 0) {
        return this.$toasted.show(this.$t("SelectDealsFirst"), {
          type: "error"
        });
      }

      let opResult = await this.$api.transactions.triggerBillingDeals(
        this.lodash.map(billings, i => i.$billingDealID)
      );

      if (true || opResult.status === "success") {
        this.lodash.forEach(billings, i => {
          i.selected = false;
          i.processed = true;
        });
      }
    },
    switchSelectAll() {
      if (!this.billingDealsFilter.onlyActual) {
        return this.$toasted.show(this.$t("PleaseEnableManualModeFirst"), {
          type: "error"
        });
      }
      this.selectAll = !this.selectAll;
      for (var i of this.billingDeals) {
        this.$set(i, "selected", this.selectAll);
      }
    }
  },
  computed: {
    canLoadMore() {
      return (
        this.numberOfRecords > 0 &&
        this.billingDealsFilter.take + this.billingDealsFilter.skip <
          this.numberOfRecords
      );
    },
    ...mapState({
      terminalStore: state => state.settings.terminal
    })
  },
  async mounted() {
    await this.getDataFromApi();
    const vm = this;
    this.$store.commit("ui/changeHeader", {
      value: {
        threeDotMenu: [
          {
            text: this.$t("Create"),
            fn: () => {
              this.$router.push({ name: "CreateBillingDeal" });
            }
          },
          {
            text: this.$t("TriggerTransactions"),
            fn: async () => {
              await vm.createTransactions();
            }
          },
          {
            text: this.$t("SelectAll"),
            fn: () => {
              vm.switchSelectAll();
            }
          }
        ]
      }
    });
  }
};
</script>