<template>
  <v-card flat class="my-2">
    <v-card-title class="px-2 py-2 ecdgray--text subtitle-3 text-uppercase">
      <v-row no-gutters align="center">
        <v-col cols="6">{{$t("Sales")}}</v-col>
        <v-col cols="6" class="text-none text-end body-2">
          <stats-filter></stats-filter>
        </v-col>
      </v-row>
    </v-card-title>
    <v-divider></v-divider>
    <v-card-text>
      <v-row align="center" justify="center" class="py-4">
        <v-col class="text-center">
          <h1 class="sum black--text">{{stats.totalAmount | currency('ILS')}}</h1>
          <p class="undertext pt-4">{{$t("@TransactionsCount").replace("@count", stats.transactionsCount)}}</p>
        </v-col>
      </v-row>
    </v-card-text>
  </v-card>
</template>

<script>
import { mapState } from "vuex";
import moment from "moment";

export default {
  components: {
    EcDialogInvoker: () => import("../ec/EcDialogInvoker"),
    StatsFilter: () => import("./StatsFilter")
  },
  data() {
    return {
      stats: {
        transactionsCount: 0,
        totalAmount: 0
      }
    }
  },
  computed: {
    ...mapState({
      terminalStore: state => state.settings.terminal,
      storeDateFilter: state => state.ui.dashboardDateFilter
    }),
  },
  async mounted () {
    await this.getData();
  },
  methods: {
    async getData() {
      let report = await this.$api.reporting.dashboard.getTransactionsTotals({
        terminalID: this.terminalStore.terminalID,
        dateFrom: this.storeDateFilter.dateFrom ? (moment(this.storeDateFilter.dateFrom).toISOString()) : null,
        dateTo: this.storeDateFilter.dateTo ? (moment(this.storeDateFilter.dateTo).toISOString()) : null,
        quickDateFilter: this.storeDateFilter.quickDateType
      });

      if(!report || report.length === 0){
        return;
      }

      this.stats.transactionsCount = report[0].transactionsCount || 0;
      this.stats.totalAmount = report[0].totalAmount || 0;
    }
  },
  watch: {
    storeDateFilter(newValue, oldValue) {
      this.getData();
    }
  }
};
</script>

<style lang="scss" scoped>
.sum {
  font-weight: 400;
  font-size: 2rem;
}
.undertext {
  font-weight: 400;
  font-size: 1.1rem;
}
</style>