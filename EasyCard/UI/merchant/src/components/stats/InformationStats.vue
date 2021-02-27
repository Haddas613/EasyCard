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
            <span class="body-1">{{item.name}}</span>
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
          name: this.$t("TotalCustomers"),
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
    }),
  },
  async mounted(){
    let report = await this.$api.reporting.dashboard.getConsumersTotals({
      terminalID: this.terminalStore.terminalID,
      dateFrom: moment().toISOString(),
      dateTo: moment().toISOString(),
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
    this.stats[2].value = `${r.newCustomers}(${r.newCustomersRate}%)`;

    //Repeating customers
    this.stats[3].value = `${r.repeatingCustomers}(${r.repeatingCustomersRate}%)`;
  }
};
</script>