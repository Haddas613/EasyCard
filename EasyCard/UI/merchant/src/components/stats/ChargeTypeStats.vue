<template>
  <v-card flat class="my-2">
    <v-card-title class="px-2 py-2 ecdgray--text subtitle-3 text-uppercase">
      <v-row no-gutters align="center">
        <v-col cols="6">{{$t("ChargeType")}}</v-col>
        <v-col cols="6" class="text-none text-end body-2">
          <stats-filter></stats-filter>
        </v-col>
      </v-row>
    </v-card-title>
    <v-divider></v-divider>
    <v-card-text>
      <v-row>
        <v-col cols="5" class="px-0" v-if="!nothingToShow">
          <v-row no-gutters v-for="d in stats" :key="d.paymentTypeEnum">
            <v-col cols="5">
              <div class="icon-circled">
                <v-icon :color="getTypeOpts(d.paymentTypeEnum).color">{{getTypeOpts(d.paymentTypeEnum).icon}}</v-icon>
              </div>
            </v-col>
            <v-col cols="7">
              {{getTypeOpts(d.paymentTypeEnum).label}}
              <p>
                <b>{{d.totalAmount | currency('ILS')}}</b>
              </p>
            </v-col>
          </v-row>
        </v-col>
        <v-col cols="7" class="px-0" v-if="!nothingToShow">
          <v-container class="chart-container px-0">
            <pie-chart v-if="draw" :options="chartOptions" :data="chartData"></pie-chart>
          </v-container>
        </v-col>
        <v-col cols="12" class="px-0" v-if="nothingToShow">
          <p class="text-center">{{$t("NothingToShow")}}</p>
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
    PieChart: () => import("../charts/PieChart"),
    EcMoney: () => import("../ec/EcMoney"),
    StatsFilter: () => import("./StatsFilter")
  },
  data() {
    return {
      draw: false, //set to true when ready to draw chart
      stats: [],
      chartOptions: null,
      nothingToShow: false,
      chartData: {
        labels: [],
        datasets: [
          {
            pointRadius: 0,
            data: []
          }
        ]
      },
      typeOpts: {
        card: {
          label: this.$t("Card"),
          color: this.$vuetify.theme.themes.light.primary,
          icon: "mdi-credit-card-outline"
        },
        cash: {
          label: this.$t("Cash"),
          color: this.$vuetify.theme.themes.light.secondary,
          icon: "mdi-cash"
        },
        bitcoin: {
          label: this.$t("Bitcoin"),
          color: this.$vuetify.theme.themes.light.accent,
          icon: "mdi-bitcoin"
        },
        _default: {
          label: this.$t("NotSpecified"),
          color: this.$vuetify.theme.themes.light.ecgray,
          icon: "mdi-cash"
        }
      },
    };
  },
  methods: {
    getTypeOpts(type) {
      return (type && this.typeOpts[type]) ? this.typeOpts[type] : this.typeOpts._default;
    },
    async getData() {
      this.draw = false;
      let report = await this.$api.reporting.dashboard.getPaymentTypeTotals({
        terminalID: this.terminalStore.terminalID,
        dateFrom: this.$formatDate(this.storeDateFilter.dateFrom),
        dateTo: this.$formatDate(this.storeDateFilter.dateTo),
        quickDateFilter: this.storeDateFilter.quickDateType,
        granularity: this.storeDateFilter.granularity,
      });
      
      if(!report || report.length === 0){
        this.nothingToShow = true;
        return;
      }

      this.stats = report;
      this.chartData.labels = this.lodash.map(report, e => this.getTypeOpts(e.paymentTypeEnum).label);
      this.chartData.datasets[0].backgroundColor = this.lodash.map(report, e => this.getTypeOpts(e.paymentTypeEnum).color);
      this.chartData.datasets[0].data = this.lodash.map(report, e => e.totalAmount);
      this.nothingToShow = false;
      this.draw = true;
    }
  },
  computed: {
    ...mapState({
      terminalStore: state => state.settings.terminal,
      storeDateFilter: state => state.ui.dashboardDateFilter
    }),
  },
  async mounted() {
    await this.getData();
  },
  watch: {
    storeDateFilter(newValue, oldValue) {
      this.getData();
    }
  }
};
</script>

<style lang="scss" scoped>
.chart-container {
  flex-grow: 1;
  min-height: 0;

  > div {
    position: relative;
    height: 125px;
  }
}
.icon-circled{
  background-color: var(--v-ecgray-lighten2);
  border-radius: 50%;
  height: 35px;
  width: 35px;
  display: flex;
  margin-top: 4px;
  i{
    padding-right: 6px!important;
    padding-left: 6px!important;
  }
}
</style>