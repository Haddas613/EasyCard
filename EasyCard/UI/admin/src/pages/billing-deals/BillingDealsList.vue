<template>
  <v-flex>
    <billing-deals-trigger-dialog
      :show.sync="showTriggerDialog"
      v-on:ok="onTriggerByTerminal()"
    ></billing-deals-trigger-dialog>
    <v-card class="my-2" width="100%" flat>
      <v-expansion-panels :flat="true">
        <v-expansion-panel>
          <v-expansion-panel-header >{{$t('Filters')}}</v-expansion-panel-header>
          <v-expansion-panel-content eager>
            <billing-deals-filter ref="filter" :filter-data="billingDealsFilter" v-on:apply="applyFilter($event)"></billing-deals-filter>
          </v-expansion-panel-content>
        </v-expansion-panel>
      </v-expansion-panels>
      <v-divider></v-divider>
      <v-card-text class="px-0">
        <v-data-table
          :headers="headers"
          :items="billingDeals"
          :options.sync="options"
          :server-items-length="numberOfRecords"
          :loading="loading"
          :header-props="{ sortIcon: null }"
          class="elevation-1"
        >
          <template v-slot:item.merchantName="{ item }">
            <router-link class="text-decoration-none" link :to="{name: 'Merchant', params: {id: item.merchantID}}">
              {{item.merchantName || item.merchantID}}
            </router-link>
          </template>    
          <template v-slot:item.terminalName="{ item }">
            <router-link class="text-decoration-none" link :to="{name: 'EditTerminal', params: {id: item.terminalID}}">
              {{item.terminalName || item.terminalID}}
            </router-link>
          </template>
          <template v-slot:item.select="{ item }">
            <input type="checkbox" v-model="item.selected" :disabled="!item.active">
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
            <b class="justify-currency">{{item.transactionAmount | currency(item.currency)}}</b>
          </template>
          <template v-slot:item.cardExpired="{ item }">
            <span v-if="item.cardExpired" class="error--text">{{$t("Yes")}}</span>
            <span v-else>{{$t("No")}}</span>
          </template>
          <template v-slot:item.cardNumber="{ item }">
            <span dir="ltr">{{item.cardNumber}}</span>
          </template> 
          <template v-slot:item.active="{ item }">
            <span v-bind:class="{'success--text': item.active, 'error--text': !item.active}">
              {{item.active ? $t("Yes") : $t("No")}}
            </span>
          </template>
          <template v-slot:item.paused="{ item }">
            <span v-bind:class="{'success--text': !item.paused, 'accent--text': item.paused}">
              {{item.paused ? $t("Yes") : $t("No")}}
            </span>
          </template>
          <template v-slot:item.invoiceOnly="{ item }">
            <span v-bind:class="{'success--text': item.invoiceOnly, 'ecgray--text': !item.invoiceOnly}">
              {{item.invoiceOnly ? $t("Yes") : $t("No")}}
            </span>
          </template>
          <template v-slot:item.actions="{ item }">
            <v-btn color="primary" outlined small link :to="{name: 'BillingDeal', params: {id: item.$billingDealID}}">
              <re-icon small>mdi-arrow-right</re-icon>
            </v-btn>
          </template>
          <template v-slot:footer>
            <p class="text-end px-4 pt-4 mb-2 body-2">
              {{$t("TotalAmount")}}
            </p>
            <p class="text-end mx-4">
              <v-chip color="primary" small>{{ totalAmountILS | currency('ILS') }}</v-chip>
              <v-chip class="mx-2" color="success" small>{{ totalAmountUSD | currency('USD') }}</v-chip>
              <v-chip color="secondary" small>{{ totalAmountEUR | currency('EUR') }}</v-chip>
            </p>
          </template> 
        </v-data-table>
      </v-card-text>
    </v-card>
  </v-flex>
</template>

<script>
import moment from "moment";

export default {
  name: "BillingDeals",
  components: {
    EcList: () => import("../../components/ec/EcList"),
    ReIcon: () => import("../../components/misc/ResponsiveIcon"),
    BillingScheduleString: () => import("../../components/billing-deals/BillingScheduleString"),
    BillingDealsFilter: () => import("../../components/billing-deals/BillingDealsFilter"),
    BillingDealsTriggerDialog: () =>
      import("../../components/billing-deals/BillingDealsTriggerDialog"),
  },
  props: {
    filters: {
      default: null
    }
  },
  data() {
    return {
      billingDeals: [],
      moment: moment,
      loading: false,
      billingDealsFilter: {
        take: this.$appConstants.config.ui.defaultTake,
        skip: 0,
        filterDateByNextScheduledTransaction: true,
        ...this.filters,
      },
      options: {},
      headers: [],
      numberOfRecords: 0,
      totalAmountILS: null,
      totalAmountUSD: null,
      totalAmountEUR: null,
      selectAll: false,
      showTriggerDialog: false,
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
      if(this.loading) { return; }
      this.loading = true;
      try{
        let data = await this.$api.billingDeals.get({
          ...this.billingDealsFilter,
          ...this.options
        });
        
        this.billingDeals = data.data;
        this.numberOfRecords = data.numberOfRecords;
        this.totalAmountILS = data.totalAmountILS;
        this.totalAmountUSD = data.totalAmountUSD;
        this.totalAmountEUR = data.totalAmountEUR;

        if (!this.headers || this.headers.length === 0) {
          this.headers = [{ value: "select", text: "", sortable: false }, ...data.headers, { value: "actions", text: this.$t("Actions"), sortable: false }];
        }
      }finally{
        this.loading = false;
      }
    },
    async applyFilter(data) {
      this.options.page = 1;
      this.billingDealsFilter = {
        ...data,
        skip: 0,
        take: 100
      };
      await this.getDataFromApi();
    },
    async createTransactions() {
      let selected = this.getSelected();
      if(!selected) { return; }
      let opResult = await this.$api.billingDeals.triggerBillingDeals(
        this.lodash.map(selected, i => i.$billingDealID)
      );
      
      await this.getDataFromApi();
    },
    async disableBillingDeals() {
      let selected = this.getSelected();
      if(!selected) { return; }
      let opResult = await this.$api.billingDeals.disableBillingDeals(
        this.lodash.map(selected, i => i.$billingDealID)
      );
      
      await this.getDataFromApi();
    },
    async activateBillingDeals() {
      let selected = this.getSelected();
      if(!selected) { return; }
      let opResult = await this.$api.billingDeals.activateBillingDeals(
        this.lodash.map(selected, i => i.$billingDealID)
      );
      
      await this.refresh();
    },
    getSelected(){
      let billings = this.lodash.filter(this.billingDeals, i => i.selected);
      if (billings.length === 0) {
        this.$toasted.show(this.$t("SelectDealsFirst"), {
          type: "error"
        });
        return null;
      }
      return billings;
    },
    onTriggerByTerminal(){
      this.$refs.filter.model.inProgress = true;
      this.$refs.filter.switchFilterChanged('inProgress');
      this.$refs.filter.apply();
    },
  },
  beforeRouteEnter(to, from, next) {
    next(vm => {
      vm.$store.commit("ui/changeHeader", {
        value: {
          threeDotMenu: [
            {
              text: vm.$t("TriggerSelected"),
              fn: async () => {
                await vm.createTransactions();
              }
            },
            {
              text: vm.$t("DisableSelected"),
              fn: async () => {
                await vm.disableBillingDeals();
              }
            },
            {
              text: vm.$t("DisableSelected"),
              fn: async () => {
                await vm.activateBillingDeals();
              }
            },
            {
              text: vm.$t("TriggerAll"),
              fn: async () => {
                vm.showTriggerDialog = true;
              }
            }
          ],
          refresh: async () => {
            await vm.getDataFromApi();
          }
        }
      });
    });
  },
};
</script>