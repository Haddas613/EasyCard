<template>
  <v-flex>
    <invoices-filter-dialog
      :show.sync="showDialog"
      :filter="invoicesFilter"
      v-on:ok="applyFilters($event)"
    ></invoices-filter-dialog>
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
          <v-col cols="12" md="6">
            <v-row no-gutters>
              <v-col cols="12">{{$t("TotalAmount")}}:</v-col>
              <v-col cols="12" class="mt-1 font-weight-bold">
                <v-chip color="primary" small>{{ totalAmountILS | currency('ILS') }}</v-chip>
                <v-chip class="mx-2" color="success" small>{{ totalAmountUSD | currency('USD') }}</v-chip>
                <v-chip color="secondary" small>{{ totalAmountEUR | currency('EUR') }}</v-chip>
              </v-col>
            </v-row>
          </v-col>
          <v-col cols="12" md="3">
            <v-row no-gutters>
              <v-col cols="12">{{$t("PeriodShown")}}:</v-col>
              <v-col cols="12" class="font-weight-bold">
                <span dir="ltr">
                  <template v-if="invoicesFilter.dateFrom">
                    {{invoicesFilter.dateFrom | ecdate("L")}}
                  </template>
                  <span>-</span>
                  <template v-if="invoicesFilter.dateTo">
                    {{invoicesFilter.dateTo | ecdate("L")}}
                  </template>
                </span>
              </v-col>
            </v-row>
          </v-col>
          <v-col cols="12" md="3">
            <v-row no-gutters>
              <v-col cols="12">{{$t("OperationsCountTotal")}}:</v-col>
              <v-col cols="12" class="font-weight-bold">{{numberOfRecords || '-'}}</v-col>
            </v-row>
          </v-col>
        </v-row>
      </v-card-text>
    </v-card>
    <v-card width="100%" flat :loading="!invoices">
      <v-card-text class="px-0" v-if="invoices">
        <!-- <v-flex class="d-flex justify-start" v-if="$vuetify.breakpoint.mdAndUp">
          <v-btn class="mx-2" :outlined="!selectAll" @click="switchSelectAll()" color="primary" x-small>{{$t('SelectAll')}}</v-btn>
        </v-flex>-->
        <invoices-list-component :key="loadCount" selectable :invoices="invoices"></invoices-list-component>

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
  name: "InvoicesList",
  components: {
    EcList: () => import("../../components/ec/EcList"),
    ReIcon: () => import("../../components/misc/ResponsiveIcon"),
    InvoicesFilterDialog: () =>
      import("../../components/invoicing/InvoicesFilterDialog"),
    EcDialogInvoker: () => import("../../components/ec/EcDialogInvoker"),
    InvoicesListComponent: () =>
      import("../../components/invoicing/InvoicesList"),
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
      invoices: null,
      customerInfo: null,
      moment: moment,
      loading: false,
      loadCount: 0,
      defaultFilter: {
        take: this.$appConstants.config.ui.defaultTake,
        skip: 0,
        dateFrom: this.$formatDate(moment().startOf('month')),
        dateTo: this.$formatDate(new Date()),
      },
      invoicesFilter: {
        ...this.filters
      },
      showDialog: this.showFiltersDialog,
      numberOfRecords: 0,
      selectAll: false,
      totalAmountILS: null,
      totalAmountUSD: null,
      totalAmountEUR: null,
    };
  },
  methods: {
    async getDataFromApi(extendData) {
      if(this.loading) { return; }
      this.loading = true;
      let data = await this.$api.invoicing.get({
        ...this.invoicesFilter
      });
      if (data) {
        let invoices = data.data || [];
        this.invoices = extendData ? [...this.invoices, ...invoices] : invoices;
        this.numberOfRecords = data.numberOfRecords || 0;
        this.totalAmountILS = data.totalAmountILS;
        this.totalAmountUSD = data.totalAmountUSD;
        this.totalAmountEUR = data.totalAmountEUR;
      }
      this.selectAll = false;
      this.loading = false;
      this.loadCount++;
    },
    async applyFilters(data) {
      this.invoicesFilter = {
        ...this.defaultFilter,
        ...data,
        skip: 0,
      };
      await this.getDataFromApi();
    },
    async refresh() {
      this.invoicesFilter.skip = 0;
      await this.getDataFromApi();
    },
    async loadMore() {
      this.invoicesFilter.skip += this.invoicesFilter.take;
      await this.getDataFromApi(true);
    },
    async resendSelectedInvoices() {
      let invoices = this.lodash.filter(
        this.invoices,
        i => i.selected && (i.$status != "sending")
      );
      if (invoices.length === 0) {
        return this.$toasted.show(this.$t("SelectInvoicesFirst"), {
          type: "error"
        });
      }

      let opResult = await this.$api.invoicing.resend(
        this.terminalStore.terminalID,
        this.lodash.map(invoices, i => i.$invoiceID)
      );

      if (opResult.status === "success") {
        let $dictionaries = await this.$api.dictionaries.$getTransactionDictionaries();
        this.lodash.forEach(invoices, i => {
          i.selected = false;
          i.$status = "sending";
          i.status = $dictionaries.invoiceStatusEnum[i.$status];
        });
      }
    },
    switchSelectAll() {
      this.selectAll = !this.selectAll;
      for (var i of this.invoices) {
        if (i.$status == "initial") {
          this.$set(i, "selected", this.selectAll);
        }
      }
    },
    async initThreeDotMenu(){
      if(!this.$integrationAvailable(this.terminalStore, this.$appConstants.terminal.integrations.invoicing)) return;
    
      this.$store.commit("ui/changeHeader", {
        value: {
          threeDotMenu: [
            {
              text: this.$t("Create"),
              fn: () => {
                this.$router.push({ name: "CreateInvoice" });
              }
            },
            {
              text: this.$t("ResendInvoices"),
              fn: async () => {
                await this.resendSelectedInvoices();
              }
            },
            {
              text: this.$t("Excel"),
              fn: async () => {
                await this.exportExcel();
              }
            },
            // {
            //   text: this.$t("SelectAll"),
            //   fn: () => {
            //     this.switchSelectAll();
            //   }
            // }
          ]
        }
      });
    },
    async exportExcel() {
      let operation = await this.$api.invoicing.getExcel({
        ...this.invoicesFilter,
      });
      
      if (!this.$apiSuccess(operation)) return;
      window.open(operation.entityReference, '_blank');
    },
  },
  computed: {
    ...mapState({
      terminalStore: state => state.settings.terminal
    }),
    canLoadMore() {
      return (
        this.numberOfRecords > 0 &&
        this.invoicesFilter.take + this.invoicesFilter.skip <
          this.numberOfRecords
      );
    }
  },
  /** Header is initialized in mounted but since components are cached (keep-alive) it's required to
    manually update menu on route change to make sure header has correct value*/
  beforeRouteEnter(to, from, next) {
    next(vm => {
      vm.initThreeDotMenu();
    });
  },
  async mounted() {

    await this.applyFilters({
      terminalID: this.terminalStore.terminalID,
      consumerID: this.filters ? this.filters.consumerID : null,
    });
    this.initThreeDotMenu();
  }
};
</script>