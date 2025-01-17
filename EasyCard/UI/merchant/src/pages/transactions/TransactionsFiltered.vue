<template>
  <v-flex>
    <transactions-filter-dialog
      :show.sync="showDialog"
      :filter="transactionsFilter"
      :key="transactionsFilter.notTransmitted"
      v-on:ok="applyFilters($event)"
    ></transactions-filter-dialog>
    <transactions-transmit-dialog
      :show.sync="showTransmitDialog"
      v-on:ok="refresh()"
    ></transactions-transmit-dialog>
    <transaction-slip-dialog v-if="selectedTransaction" :key="selectedTransaction.$paymentTransactionID" ref="slipDialog" :transaction="selectedTransaction" :show.sync="transactionSlipDialog"></transaction-slip-dialog>
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
                  <template v-if="transactionsFilter.dateFrom">
                    {{transactionsFilter.dateFrom | ecdate("L")}}
                  </template>
                  <span>-</span>
                  <template v-if="transactionsFilter.dateTo">
                    {{transactionsFilter.dateTo | ecdate("L")}}
                  </template>
                </span>
              </v-col>
            </v-row>
          </v-col>
          <v-col cols="12" md="3">
            <v-row no-gutters>
              <v-col cols="12">{{$t("OperationsCountTotal")}}:</v-col>
              <v-col cols="12" class="font-weight-bold">
                {{numberOfRecords || '-'}}
              </v-col>
            </v-row>
          </v-col>
        </v-row>
        <v-row no-gutters class="px-1 body-2">
          <v-switch 
            v-model="transactionsFilter.notTransmitted" 
            @change="getDataFromApi(false)"
            dense
            persistent-hint
            :hint="$t('ShowOnlyNotTransmittedTransactions')">
            <template v-slot:label>
              <small>{{$t('NotTransmitted')}}</small>
            </template>
          </v-switch>
        </v-row>
      </v-card-text>
    </v-card>
    <v-card width="100%" flat :loading="!transactions">
      <v-card-text class="px-0">
        <template v-if="$vuetify.breakpoint.mdAndDown">
          <transactions-list :key="loading" :transactions="transactions" :select-limit="selectLimit" :selectable="transactionsFilter.notTransmitted" ref="transactionsList"></transactions-list>
          <v-flex class="text-center" v-if="canLoadMore">
            <v-btn outlined color="primary" :loading="loading" @click="loadMore()">{{$t("LoadMore")}}</v-btn>
          </v-flex>
        </template>
        <template v-else>
          <v-data-table 
            :headers="headers"
            :items="transactions"
            :options.sync="options"
            :server-items-length="numberOfRecords"
            :loading="loading"
            :header-props="{ sortIcon: null }"
            :footer-props="{
              'items-per-page-options': [10, 25, 50, 100]
            }"
            class="elevation-1">     
          <template v-slot:item.terminalName="{ item }">
            <small>{{item.terminalName || item.terminalID}}</small>
          </template> 
          <template v-slot:item.transactionAmount="{ item }">
            <b class="justify-currency" v-bind:class="{'red--text': item.specialTransactionType == 'refund'}">{{item.transactionAmount | currency(item.currency)}}</b>
          </template>
          <template v-slot:item.transactionTimestamp="{ item }">
          {{item.$transactionTimestamp | ecdate('DT')}}
          </template>
          <template v-slot:item.quickStatus="{ item }">
            <span v-bind:class="quickStatusesColors[item.$quickStatus]">{{$t(item.quickStatus || 'None')}}</span>
          </template>
          <template v-slot:item.actions="{ item }">
            <v-btn class="mx-1" color="primary" outlined small link :to="{name: 'Transaction', params: {id: item.$paymentTransactionID}}">
              <re-icon small>mdi-arrow-right</re-icon>
            </v-btn>
            <v-btn v-if="(item.status == 'completed' || item.status == 'awaitingForTransmission') && item.$jDealType == 'J4'" color="orange" class="mx-1" outlined small @click="showSlipDialog(item)">
              <v-icon small>mdi-checkbook</v-icon>
            </v-btn>
          </template>    
          <template v-slot:item.cardPresence="{ item }">
            <v-icon :title="$t('Bank')" v-if="item.paymentTypeEnum == 'bank'" color="secondary">mdi-bank</v-icon>
            <span v-else :title="item.cardPresence">
              <v-icon v-if="item.$cardPresence == 'regular'" color="success">mdi-credit-card-check</v-icon>
              <v-icon v-else>mdi-credit-card-off-outline</v-icon>
            </span>
          </template>
          <template v-slot:item.cardNumber="{ item }">
            <span dir="ltr">{{item.cardNumber}}</span>
          </template>   
          <template v-slot:item.transactionType="{ item }">
            <span :title="item.transactionType">
              <v-icon v-if="item.$transactionType == 'regularDeal'" color="primary">mdi-cash</v-icon>
              <v-icon v-else-if="item.$transactionType == 'installments'" color="accent">mdi-credit-card-check</v-icon>
              <v-icon v-else color="secondary">mdi-credit-card-outline</v-icon>
            </span>
          </template>
          <template v-slot:item.paymentTransactionID="{ item }">
            <small>{{item.paymentTransactionID}}</small>
          </template>
        </v-data-table>
        </template>
      </v-card-text>
    </v-card>
  </v-flex>
</template>

<script>
import moment from "moment";
import { mapState } from "vuex";
export default {
  name: "TransactionsFiltered",
  components: {
    EcList: () => import("../../components/ec/EcList"),
    ReIcon: () => import("../../components/misc/ResponsiveIcon"),
    TransactionsList: () => import("../../components/transactions/TransactionsList"),
    TransactionsFilterDialog: () =>
      import("../../components/transactions/TransactionsFilterDialog"),
    EcDialogInvoker: () => import("../../components/ec/EcDialogInvoker"),
    TransactionsTransmitDialog: () =>
      import("../../components/transactions/TransactionsTransmitDialog"),
      TransactionSlipDialog: () =>
      import("../../components/transactions/TransactionSlipDialog"),
  },
  props: {
    filters: {
      default: () => {
        return {
          notTransmitted: false
        }
      },
    },
    showFiltersDialog: {
      type: Boolean,
      default: false,
      required: false
    }
  },
  watch: {
    options: {
      handler: async function() {
        await this.getDataFromApi();
      },
      deep: true
    }
  },
  data() {
    return {
      transactions: [],
      customerInfo: null,
      moment: moment,
      loading: false,
      transactionsFilter: {
        ...this.filters,
        terminalID: null,
      },
      headers: [],
      defaultFilter: {
        take: this.$appConstants.config.ui.defaultTake,
        skip: 0,
        jDealType: "J4",
        dateFrom: this.$formatDate(moment().startOf('month')),
        dateTo: this.$formatDate(new Date()),
      },
      showDialog: this.showFiltersDialog,
      showTransmitDialog: false,
      datePeriod: null,
      numberOfRecords: 0,
      selectAll: false,
      selectLimit: 1000, // TODO: from config
      options: {},
      quickStatusesColors: {
        Pending: "primary--text",
        None: "ecgray--text",
        Completed: "success--text",
        Failed: "error--text",
        Canceled: "accent--text"
      },
      selectedTransaction: null,
      transactionSlipDialog: false,
      loadingTransaction: false,
      totalAmountILS: null,
      totalAmountUSD: null,
      totalAmountEUR: null,
    };
  },
  methods: {
    async getDataFromApi(extendData) {
      if(this.loading) { return; }
      this.loading = true;
      let data = await this.$api.transactions.get({
        ...this.transactionsFilter,
        ...this.options,
      });
      
      if (data) {
        if(!this.headers || this.headers.length === 0){
          this.headers = [...data.headers, { value: "actions", text: this.$t("Actions"), sortable: false  }];
        }
        let transactions = data.data || [];
        this.transactions = extendData ? [...this.transactions, ...transactions] : transactions;
        this.numberOfRecords = data.numberOfRecords || 0;
        this.totalAmountILS = data.totalAmountILS;
        this.totalAmountUSD = data.totalAmountUSD;
        this.totalAmountEUR = data.totalAmountEUR;
      }
      this.selectAll = false;
      this.loading = false;
    },
    async applyFilters(data) {
      this.options.page = 1;
      this.transactionsFilter = {
        ...this.filters,
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
    },
    async transmitSelected(){
      let selected = this.lodash.filter(this.transactions, t => t.selected);
      if(selected.length === 0){
        return this.$toasted.show(this.$t("SelectTransactionsFirst"), { type: "error" });
      }

      let opResult = await this.$api.transmissions.transmit({
        terminalID: this.terminalStore.terminalID, 
        paymentTransactionIDs: this.lodash.map(selected, i => i.$paymentTransactionID)
      });

      await this.getDataFromApi(false);
    },
    switchSelectAll(){
      if(!this.transactionsFilter.notTransmitted){
        return this.$toasted.show(this.$t("PleaseEnableManualModeFirst"), { type: "error" });
      }

      if(this.transactions.length > this.selectLimit){
        return this.$toasted.show(this.$t("@MaxSelectionCount").replace("@count", this.selectLimit), { type: "error" });
      }

      this.selectAll = !this.selectAll;
      for(var i of this.transactions){
          this.$set(i, 'selected', this.selectAll);
      }
    },
    async exportExcel() {
      let operation = await this.$api.transactions.getExcel({
        ...this.transactionsFilter,
        ...this.options
      });
      if(!this.$apiSuccess(operation)) return;
      window.open(operation.entityReference, "_blank");
    },
    async showSlipDialog(transaction){
      if(this.loadingTransaction){
        return;
      }
      this.loadingTransaction = true;
      this.selectedTransaction = await this.$api.transactions.getTransaction(
        transaction.$paymentTransactionID
      );
      this.loadingTransaction = false;
      this.transactionSlipDialog = true;
    },
    async initThreeDotMenu(){
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
              text: this.$t("TransmitSelected"),
              fn: async () => await this.transmitSelected()
            },
            {
              text: this.$t("TransmitAll"),
              fn: () => this.showTransmitDialog = true
            },
            // {
            //   text: this.$t("SelectAll"),
            //   fn: () => {
            //     this.switchSelectAll();
            //   }
            // },
            {
              text: this.$t("Excel"),
              fn: () => {
                this.exportExcel();
              }
            }
          ]
        }
      });
    },
  },
  computed: {
    canLoadMore() {
      return this.numberOfRecords > 0 
        && (this.transactionsFilter.take + this.transactionsFilter.skip) < this.numberOfRecords;
    },
    ...mapState({
      terminalStore: state => state.settings.terminal,
    })
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
    });
    this.initThreeDotMenu();
  }
};
</script>