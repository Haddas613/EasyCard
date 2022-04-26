<template>
  <v-card flat class="my-2">
    <v-card-title class="px-2 py-2 ecdgray--text subtitle-3 text-uppercase">
      <v-row no-gutters align="center">
        <v-col cols="6">{{$t("Information")}}</v-col>
        <v-col cols="6" class="text-none text-end body-2">
          <stats-filter></stats-filter>
        </v-col>
      </v-row>
    </v-card-title>
    <v-divider></v-divider>
    <v-card-text>
      <ec-list class="" v-if="stats && stats.length > 0" :items="stats" dense dashed>
        <template v-slot:left="{ item }">
          <v-col cols="12" class="text-align-initial text-oneline">
            <span class="body-2">{{item.name}}</span>
          </v-col>
        </template>
        <template v-slot:right="{ item }">
          <v-col cols="12" class="text-end font-weight-bold subtitle-2">{{item.value}}</v-col>
        </template>
      </ec-list>
    </v-card-text>
  </v-card>
</template>

<script>
import { mapState } from "vuex";
import moment from "moment";

export default {
  components: {
    EcList: () => import("../ec/EcList"),
    StatsFilter: () => import("./StatsFilter")
  },
  data() {
    return {
      stats: [
        {
          name: this.$t("AverageSpend"),
          value: null
        },
        {
          name: this.$t("NumberOfChargedCustomers"),
          value: null
        },
        {
          name: this.$t("NewCustomers"),
          value: null
        },
        {
          name: this.$t("RepeatingCustomers"),
          value: null
        }
      ]
    };
  },
  computed: {
    ...mapState({
      terminalStore: state => state.settings.terminal,
      storeDateFilter: state => state.ui.dashboardDateFilter,
      currencyStore: state => state.settings.currency,
    }),
  },
  async mounted(){
    await this.getData();
  },
  methods: {
    async getData() {
      let report = await this.$api.reporting.dashboard.getConsumersTotals({
        terminalID: this.terminalStore.terminalID,
        dateFrom: this.$formatDate(this.storeDateFilter.dateFrom),
        dateTo: this.$formatDate(this.storeDateFilter.dateTo),
        quickDateFilter: this.storeDateFilter.quickDateType
      });

      if(!report || report.length === 0){
        return;
      }

      const r = report[0];
      //Average spend
      this.stats[0].value = this.$options.filters.currency(r.averageAmount || 0, 'ILS');

      //Total customers
      this.stats[1].value = r.customersCount;

      //New customers
      let newCustomersSum = this.$options.filters.currency(r.newCustomers || 0, 'ILS');
      this.stats[2].value = `${newCustomersSum} / ${this.toPercentage(r.newCustomersRate)}`;

      //this.stats[2].value = `${r.newCustomers.toFixed(2)}₪(${r.newCustomersRate}%)`;

      //Repeating customers
      let repCustomersSum = this.$options.filters.currency(r.repeatingCustomers || 0, 'ILS');
      this.stats[3].value = `${repCustomersSum} / ${this.toPercentage(r.repeatingCustomersRate)}`;
    },
    toPercentage(value){
      return value ? `${(value * 100).toFixed(2)}%` : 0;
    },
  },
  watch: {
    storeDateFilter(newValue, oldValue) {
      this.getData();
    }
  }
};
</script>