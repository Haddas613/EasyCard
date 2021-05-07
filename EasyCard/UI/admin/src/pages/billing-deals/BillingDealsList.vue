<template>
  <v-flex>
    <v-card class="my-2" width="100%" flat>
      <v-expansion-panels :flat="true">
        <v-expansion-panel>
          <v-expansion-panel-header >{{$t('Filters')}}</v-expansion-panel-header>
          <v-expansion-panel-content>
            <billing-deals-filter :filter-data="billingDealsFilter" v-on:apply="applyFilter($event)"></billing-deals-filter>
          </v-expansion-panel-content>
        </v-expansion-panel>
      </v-expansion-panels>
      <v-divider></v-divider>
      <v-card-text class="px-0">
        <v-data-table
          :headers="headers"
          :items="billingDeals"
          :options.sync="options"
          :server-items-length="totalAmount"
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
            <b>{{item.transactionAmount | currency(item.currency)}}</b>
          </template>
          <template v-slot:item.cardExpired="{ item }">
            <span v-if="item.cardExpired" class="error--text">{{$t("Yes")}}</span>
            <span v-else>{{$t("No")}}</span>
          </template>
          <template v-slot:item.active="{ item }">
            {{item.active ? $t("Yes") : $t("No")}}
          </template>
          <template v-slot:item.actions="{ item }">
            <v-btn color="primary" outlined small link :to="{name: 'BillingDeal', params: {id: item.$billingDealID}}">
              <re-icon small>mdi-arrow-right</re-icon>
            </v-btn>
          </template>
        </v-data-table>
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
    BillingScheduleString: () => import("../../components/billing-deals/BillingScheduleString"),
    BillingDealsFilter: () => import("../../components/billing-deals/BillingDealsFilter")
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
      billingDeals: [],
      moment: moment,
      loading: false,
      billingDealsFilter: {
        take: 100,
        skip: 0,
        ...this.filters
      },
      options: {},
      headers: [],
      totalAmount: 0,
      selectAll: false,
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
      this.loading = true;
      let data = await this.$api.billingDeals.get({
        ...this.billingDealsFilter,
        ...this.options
      });
      
      this.billingDeals = data.data;
      this.totalAmount = data.numberOfRecords;
      this.loading = false;

      if (!this.headers || this.headers.length === 0) {
        this.headers = [{ value: "select", text: "", sortable: false }, ...data.headers, { value: "actions", text: this.$t("Actions"), sortable: false }];
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
    }
  },
  async mounted() {
    const vm = this;
    this.$store.commit("ui/changeHeader", {
      value: {
        threeDotMenu: [
          {
            text: this.$t("TriggerTransactions"),
            fn: async () => {
              await vm.createTransactions();
            }
          }
        ]
      }
    });
  }
};
</script>