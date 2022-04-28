<template>
  <v-flex>
    <billing-deals-filter-dialog
      :show.sync="showDialog"
      :filter="billingDealsFilter"
      v-on:ok="applyFilters($event)"
    ></billing-deals-filter-dialog>
    <billing-deals-trigger-dialog
      :show.sync="showTriggerDialog"
      v-on:ok="onTriggerByTerminal()"
    ></billing-deals-trigger-dialog>
    <v-card class="my-2" width="100%" flat>
      <v-card-title class="pb-0">
        <v-row class="py-0" no-gutters>
          <v-col cols="8">{{ $t("Overview") }}</v-col>
          <v-col cols="3" class="text-end">
            <v-btn
              class="button"
              color="primary"
              outlined
              @click="showDialog = true"
              >{{ $t("Filter") }}</v-btn
            >
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
              <v-col cols="12">{{ $t("PeriodShown") }}:</v-col>
              <v-col cols="12" class="font-weight-bold">
                <span dir="ltr">
                  <template v-if="billingDealsFilter.dateFrom">
                    {{billingDealsFilter.dateFrom | ecdate("L")}}
                  </template>
                  <template v-else>-</template>
                  <span>/</span>
                  <template v-if="billingDealsFilter.dateTo">
                    {{billingDealsFilter.dateTo | ecdate("L")}}
                  </template>
                  <template v-else>-</template>
                </span>
              </v-col>
            </v-row>
          </v-col>
          <v-col cols="12" md="3" lg="3" xl="3">
            <v-row no-gutters>
              <v-col cols="12">{{ $t("OperationsCountTotal") }}:</v-col>
              <v-col cols="12" class="font-weight-bold">{{
                numberOfRecords || "-"
              }}</v-col>
            </v-row>
          </v-col>
        </v-row>
        <v-row no-gutters class="body-2 mt-2" align-content="center">
          <v-col cols="12" class="d-flex justify-center">
            <v-btn-toggle
              class="d-flex"
              :class="{ 'flex-column': $vuetify.breakpoint.smAndDown }"
              v-model="billingDealsFilter.quickStatus"
              @change="getDataFromApi(false)"
            >
              <v-btn small outlined color="secondary" value="manualTrigger">{{
                $t("ManualTrigger")
              }}</v-btn>
              <v-btn small outlined color="primary" value="completed">{{
                $t("Completed")
              }}</v-btn>
              <v-btn small outlined color="teal" value="triggeredTomorrow">{{
                $t("Tomorrow")
              }}</v-btn>
              <v-btn small outlined color="success" value="inProgress">{{
                $t("InProgress")
              }}</v-btn>
              <v-btn small outlined color="accent" value="paused">{{
                $t("Paused")
              }}</v-btn>
              <v-btn small outlined color="orange" value="cardExpired">{{
                $t("CardExpiredNoCard")
              }}</v-btn>
              <v-btn
                small
                outlined
                color="deep-orange"
                value="expiredNextMonth"
                >{{ $t("ExpireNextMonth") }}</v-btn
              >
              <v-btn small outlined color="error" value="failed">{{
                $t("Failed")
              }}</v-btn>
              <v-btn small outlined color="gray" value="inactive">{{
                $t("Inactive")
              }}</v-btn>
            </v-btn-toggle>
          </v-col>
        </v-row>
      </v-card-text>
    </v-card>
    <v-card width="100%" flat :loading="!billingDeals">
      <v-card-text class="px-0 pt-0">
        <template v-if="billingDeals && billingDeals.length">
          <ec-list :items="billingDeals" v-if="$vuetify.breakpoint.mdAndDown">
            <template
              v-slot:prepend="{ item }"
              v-if="billingDealsFilter.quickStatus == 'manualTrigger'"
            >
              <div class="px-1">
                <v-checkbox
                  v-model="item.selected"
                  v-if="!item.processed"
                ></v-checkbox>
                <v-icon v-else color="success">mdi-check-circle</v-icon>
              </div>
            </template>
            <template v-slot:prepend="{ item }" v-else>
              <div class="mx-2 mb-1 pb-1px">
                <v-checkbox
                  hide-details
                  dense
                  v-model="item.selected"
                  v-if="!item.processed"
                ></v-checkbox>
                <v-icon v-else color="success">mdi-check-circle</v-icon>
              </div>

              <template v-if="$vuetify.breakpoint.mdAndUp">
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

              <v-tooltip top v-if="item.cardExpired">
                <template v-slot:activator="{ on, attrs }">
                  <v-btn color="ecred" dark icon v-bind="attrs" v-on="on">
                    <v-icon :title="$t('CreditCardExpired')"
                      >mdi-credit-card</v-icon
                    >
                  </v-btn>
                </template>
                {{ $t("CreditCardHasExpired") }}
              </v-tooltip>
              <v-tooltip top v-else-if="!item.active">
                <template v-slot:activator="{ on, attrs }">
                  <v-btn color="ecred" dark icon v-bind="attrs" v-on="on">
                    <v-icon :title="$t('Inactive')">mdi-close</v-icon>
                  </v-btn>
                </template>
                {{ $t("Inactive") }}
              </v-tooltip>
              <v-tooltip top v-else-if="item.paused">
                <template v-slot:activator="{ on, attrs }">
                  <v-btn color="accent" dark icon v-bind="attrs" v-on="on">
                    <v-icon :title="$t('Paused')">mdi-pause</v-icon>
                  </v-btn>
                </template>
                {{ $t("Paused") }}
              </v-tooltip>
              <v-tooltip top v-else-if="item.active">
                <template v-slot:activator="{ on, attrs }">
                  <v-btn color="success" dark icon v-bind="attrs" v-on="on">
                    <v-icon :title="$t('Active')">mdi-check</v-icon>
                  </v-btn>
                </template>
                {{ $t("Active") }}
              </v-tooltip>
            </template>

            <template v-slot:left="{ item }">
              <v-col cols="12" md="6" lg="6" class="caption ecgray--text">
                <b
                  v-if="item.$nextScheduledTransaction"
                  v-bind:class="{
                    'error--text': item.$nextScheduledTransaction > now,
                  }"
                >
                  {{ item.$nextScheduledTransaction | ecdate("DD/MM/YYYY") }}
                </b>
                <span v-else>-</span>
              </v-col>
              <v-col cols="12" md="6" lg="6" class="d-flex align-center">{{
                item.consumerName || "-"
              }}</v-col>
            </template>

            <template v-slot:right="{ item }">
              <v-col
                cols="12"
                md="6"
                lg="6"
                class="text-end body-2"
                v-bind:class="{ 'ecred--text': item.cardExpired }"
              >
                {{ item.currentDeal || "-" }}
              </v-col>
              <v-col
                cols="12"
                md="6"
                class="text-end font-weight-bold button primary--text"
                v-if="item.invoiceOnly"
              >
                <p class="my-0 py-0">
                  <small>{{ $t("InvoiceOnly") }}</small>
                </p>
                {{ item.transactionAmount | currency(item.$currency) }}
              </v-col>
              <v-col
                cols="12"
                md="6"
                lg="6"
                class="text-end font-weight-bold button"
                v-else
                v-bind:class="{
                  'ecred--text': item.cardExpired,
                  'ecgray--text': !item.active,
                }"
                >{{ item.transactionAmount | currency(item.$currency) }}</v-col
              >
            </template>

            <template v-slot:append="{ item }">
              <v-btn
                icon
                :to="{
                  name: 'BillingDeal',
                  params: { id: item.billingDealID },
                }"
              >
                <re-icon>mdi-chevron-right</re-icon>
              </v-btn>
            </template>
          </ec-list>
          <v-data-table
            v-else
            :headers="headers"
            :items="billingDeals"
            :options.sync="options"
            :server-items-length="numberOfRecords"
            :loading="loading"
            :header-props="{ sortIcon: null }"
            class="elevation-1"
          >
            <template v-slot:item.select="{ item }">
              <input
                type="checkbox"
                v-model="item.selected"
                :disabled="!item.active"
              />
            </template>
            <template v-slot:item.billingSchedule="{ item }">
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
            <template v-slot:item.transactionAmount="{ item }">
              <b class="justify-currency">{{
                item.transactionAmount | currency(item.currency)
              }}</b>
            </template>
            <template v-slot:item.cardExpired="{ item }">
              <span v-if="item.cardExpired" class="error--text">{{
                $t("Yes")
              }}</span>
              <span v-else>{{ $t("No") }}</span>
            </template>
            <template v-slot:item.cardNumber="{ item }">
              <span dir="ltr">{{ item.cardNumber }}</span>
            </template>
            <template v-slot:item.active="{ item }">
              <span
                v-bind:class="{
                  'success--text': item.active,
                  'error--text': !item.active,
                }"
              >
                {{ item.active ? $t("Yes") : $t("No") }}
              </span>
            </template>
            <template v-slot:item.paused="{ item }">
              <span
                v-bind:class="{
                  'success--text': !item.paused,
                  'accent--text': item.paused,
                }"
              >
                {{ item.paused ? $t("Yes") : $t("No") }}
              </span>
            </template>
            <template v-slot:item.invoiceOnly="{ item }">
              <span
                v-bind:class="{
                  'success--text': item.invoiceOnly,
                  'ecgray--text': !item.invoiceOnly,
                }"
              >
                {{ item.invoiceOnly ? $t("Yes") : $t("No") }}
              </span>
            </template>
            <template v-slot:item.actions="{ item }">
              <v-btn
                color="primary"
                outlined
                small
                link
                :to="{
                  name: 'BillingDeal',
                  params: { id: item.billingDealID },
                }"
              >
                <re-icon small>mdi-arrow-right</re-icon>
              </v-btn>
            </template>
          </v-data-table>
        </template>
        <p
          class="ecgray--text text-center pt-4"
          v-if="!billingDeals || billingDeals.length === 0"
        >
          {{ $t("NothingToShow") }}
        </p>

        <v-flex class="text-center" v-if="canLoadMore">
          <v-btn
            outlined
            color="primary"
            :loading="loading"
            @click="loadMore()"
            >{{ $t("LoadMore") }}</v-btn
          >
        </v-flex>
      </v-card-text>
    </v-card>
  </v-flex>
</template>

<script>
import moment from "moment";
import { mapState } from "vuex";

export default {
  name: "BillingDealsList",
  components: {
    EcList: () => import("../../components/ec/EcList"),
    ReIcon: () => import("../../components/misc/ResponsiveIcon"),
    BillingDealsFilterDialog: () =>
      import("../../components/billing-deals/BillingDealsFilterDialog"),
    BillingDealsTriggerDialog: () =>
      import("../../components/billing-deals/BillingDealsTriggerDialog"),
    BillingScheduleString: () =>
      import("../../components/billing-deals/BillingScheduleString"),
    EcDialogInvoker: () => import("../../components/ec/EcDialogInvoker"),
  },
  props: {
    filters: {
      default: null,
    },
    showFiltersDialog: {
      type: Boolean,
      default: false,
      required: false,
    },
  },
  data() {
    return {
      billingDeals: null,
      quickStatusesColors: {
        Pending: "ecgray--text",
        None: "",
        Completed: "success--text",
        Failed: "error--text",
      },
      customerInfo: null,
      moment: moment,
      loading: false,
      billingDealsFilter: {
        take: this.$appConstants.config.ui.defaultTake,
        skip: 0,
        actual: null,
        filterDateByNextScheduledTransaction: true,
        terminalID: null,
        quickStatus: null,
        ...this.filters,
      },
      showDialog: this.showFiltersDialog,
      showTriggerDialog: false,
      numberOfRecords: 0,
      selectAll: false,
      now: new Date(),
      headers: [],
      options: {},
    };
  },
  methods: {
    async getDataFromApi(extendData) {
      //this.loading = true;
      let data = await this.$api.billingDeals.get({
        ...this.billingDealsFilter,
        ...this.options,
      });
      if (data) {
        let billingDeals = data.data || [];
        this.billingDeals = extendData
          ? [...this.billingDeals, ...billingDeals]
          : billingDeals;
        this.numberOfRecords = data.numberOfRecords || 0;

        if (!this.headers || this.headers.length === 0) {
          this.headers = [{ value: "select", text: "", sortable: false }, ...data.headers, { value: "actions", text: this.$t("Actions"), sortable: false }];
        }
      }
      this.selectAll = false;
      this.loading = false;
    },
    async applyFilters(data) {
      this.options.page = 1;
      this.billingDealsFilter = {
        ...this.billingDealsFilter,
        ...data,
        skip: 0,
        quickStatus: null, //quick status must be reset if filter is applied
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
    getSelected() {
      let billings = this.lodash.filter(this.billingDeals, (i) => i.selected);
      if (billings.length === 0) {
        this.$toasted.show(this.$t("SelectDealsFirst"), {
          type: "error",
        });
        return null;
      }
      return billings;
    },
    async disableBillingDeals() {
      let selected = this.getSelected();
      if (!selected) {
        return;
      }
      let opResult = await this.$api.billingDeals.disableBillingDeals(
        this.lodash.map(selected, (i) => i.billingDealID)
      );

      await this.refresh();
    },
    async activateBillingDeals() {
      let selected = this.getSelected();
      if (!selected) {
        return;
      }
      let opResult = await this.$api.billingDeals.activateBillingDeals(
        this.lodash.map(selected, (i) => i.billingDealID)
      );

      await this.refresh();
    },
    async createTransactions() {
      if (!this.billingDealsFilter.quickStatus == "manualTrigger") {
        return this.$toasted.show(this.$t("PleaseEnableManualModeFirst"), {
          type: "error",
        });
      }
      let selected = this.getSelected();
      if (!selected) {
        return;
      }

      let opResult = await this.$api.billingDeals.triggerBillingDeals(
        this.lodash.map(selected, (i) => i.billingDealID)
      );
      //this.switchFilterChanged('inProgress');
      this.billingDealsFilter.inProgress = true;
      await this.refresh();
    },
    async onTriggerByTerminal() {
      this.billingDealsFilter.inProgress = true;
      await this.refresh();
      //await this.switchFilterChanged('inProgress');
    },
    switchSelectAll() {
      // if (!this.billingDealsFilter.quickStatus == 'manualTrigger') {
      //   return this.$toasted.show(this.$t("PleaseEnableManualModeFirst"), {
      //     type: "error"
      //   });
      // }
      this.selectAll = !this.selectAll;
      for (var i of this.billingDeals) {
        this.$set(i, "selected", this.selectAll);
      }
    },
    // async switchFilterChanged(type){
    //   let allTypes = ['showDeleted', 'actual', 'paused', 'finished', 'hasError', 'inProgress', 'creditCardExpired'].filter(v => v != type);
    //   for(var t of allTypes){
    //     if(t === "showDeleted"){
    //       this.$set(this.billingDealsFilter, t, 0);
    //     }else{
    //       this.$set(this.billingDealsFilter, t, false);
    //     }
    //   }
    //   await this.getDataFromApi(false);
    // },
    async exportExcel() {
      let operation = await this.$api.billingDeals.getExcel({
        ...this.billingDealsFilter,
      });
      if (!this.$apiSuccess(operation)) return;
      window.open(operation.entityReference, "_blank");
    },
    async initThreeDotMenu() {
      const vm = this;
      this.$store.commit("ui/changeHeader", {
        value: {
          threeDotMenu: [
            {
              text: this.$t("Create"),
              fn: () => {
                this.$router.push({ name: "CreateBillingDeal" });
              },
            },
            // {
            //   text: this.$t("SelectAll"),
            //   fn: () => {
            //     vm.switchSelectAll();
            //   }
            // },
            {
              text: this.$t("TriggerSelected"),
              fn: async () => {
                await vm.createTransactions();
              },
            },
            {
              text: this.$t("DisableSelected"),
              fn: async () => {
                await vm.disableBillingDeals();
              },
            },
            {
              text: this.$t("ActivateSelected"),
              fn: async () => {
                await vm.activateBillingDeals();
              },
            },
            {
              text: this.$t("TriggerAll"),
              fn: async () => {
                vm.showTriggerDialog = true;
              },
            },
            {
              text: this.$t("Excel"),
              fn: () => {
                this.exportExcel();
              },
            },
          ],
        },
      });
    },
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
      terminalStore: (state) => state.settings.terminal,
    }),
  },
  async mounted() {
    await this.applyFilters();
    await this.initThreeDotMenu();
  },
  /** Header is initialized in mounted but since components are cached (keep-alive) it's required to
    manually update menu on route change to make sure header has correct value*/
  beforeRouteEnter(to, from, next) {
    next(vm => {
      vm.initThreeDotMenu();
    });
  },
};
</script>

<style lang="scss" scoped>
.pb-1px {
  padding-bottom: 1px;
}
</style>
