<template>
  <v-card flat class="my-2">
    <v-card-title class="px-2 py-2 ecdgray--text subtitle-3 text-uppercase">
      <v-row no-gutters align="center">
        <v-col cols="6">{{$t("CashFlow")}}</v-col>
        <v-col cols="6" class="text-none text-end body-2">
          <stats-filter-alt></stats-filter-alt>
        </v-col>
      </v-row>
    </v-card-title>
    <v-divider></v-divider>
    <v-card-text class="px-0">
      <v-row class="px-2 black--text body-2">
        <v-col cols="12" md="5" class="pb-0">
          <span class="main-line-color">{{$t("Selection")}}</span>
          <div v-if="report">
            <div>
              <small>{{givenPeriodDate}}</small>
            </div>
            <div class="pt-1">
              <div>{{$t("Total")}}</div>
              <div>
                <b>{{report.givenPeriodMeasure | currency('ILS')}}</b>
              </div>
            </div>
          </div>
        </v-col>
        <v-col md="2" class="px-0 hidden-sm-and-down text-center">
          <v-divider vertical></v-divider>
        </v-col>
        <v-col cols="12" md="5" class="pb-0">
          <span class="ref-line-color">{{$t("Reference")}}</span>
          <div v-if="report">
            <div>
              <small>{{altPeriodDate}}</small>
            </div>
            <div v-if="report.altPeriodMeasure" class="pt-1">
              <div>{{$t("Total")}}</div>
              <div>
                <b>{{report.altPeriodMeasure | currency('ILS')}}</b>
              </div>
            </div>
          </div>
        </v-col>
      </v-row>
      <v-container class="chart-container px-0" v-if="!nothingToShow">
        <line-chart v-if="draw" :options="chartOptions" :data="chartData"></line-chart>
      </v-container>
      <p class="text-center" v-else>
        {{$t("NothingToShow")}}
      </p>
    </v-card-text>
  </v-card>
</template>

<script>
import { mapState } from "vuex";
import moment from "moment";

export default {
  components: {
    LineChart: () => import("../charts/LineChart"),
    StatsFilterAlt: () => import("./StatsFilterAlt")
  },
  data() {
    return {
      draw: false, //set to true when ready to draw chart
      nothingToShow: false,
      chartOptions: {
        scales: {
          yAxes: [
            {
              ticks: {
                //min: 0,
                //max: 1500,
                // stepSize: 500,
                // display: false,
                // maxTicksLimit: 3,
              },
              gridLines: {
                drawOnChartArea: true,
                drawBorder: false,
                drawTicks: false,
                //offsetGridLines: true
              },
              afterTickToLabelConversion: function(scaleInstance) {
                // set the first and last tick to null so it does not display
                // note, ticks[0] is the last tick and ticks[length - 1] is the first
                scaleInstance.ticks[scaleInstance.ticks.length - 1] = null;

                // need to do the same thing for this similiar array which is used internally
                scaleInstance.ticksAsNumbers[scaleInstance.ticksAsNumbers.length - 1] = null;
              },
            }
          ],
          xAxes: [
            {
              gridLines: {
                drawOnChartArea: false,
                drawTicks: false,
                display:false,
              }
            }
          ]
        }
      },
      chartData: {
        datasets: [
          {
            pointRadius: 0,
            borderColor: '#fd8442bf',
            backgroundColor: '#fd84420d',
            // backgroundColor: '#ffa53269',
            data: []
          },
          {
            pointRadius: 0,
            borderColor: '#8e8e8e',
            backgroundColor: '#ff000000',
            // backgroundColor: '#ffa53269',
            data: []
          }
        ]
      },
      report: null
    };
  },
  computed: {
    ...mapState({
      terminalStore: state => state.settings.terminal,
      storeDateFilter: state => state.ui.dashboardDateFilterAlt
    }),
    givenPeriodDate(){
      if(!this.report || !this.report.dateFrom || !this.report.dateTo){
        return '-';
      }
      return `${moment(this.report.dateFrom).format('MM/DD/YY')} ~ ${moment(this.report.dateTo).format('MM/DD/YY')}`;
    },
    altPeriodDate(){
      if(!this.report || !this.report.altDateFrom || !this.report.altDateTo){
        return '-';
      }
      return `${moment(this.report.altDateFrom).format('MM/DD/YY')} ~ ${moment(this.report.altDateTo).format('MM/DD/YY')}`;
    },
  },
  async mounted () {
     await this.getData();
  },
  methods: {
    async getData() {
      this.draw = false;
      let report = await this.$api.reporting.dashboard.getTransactionTimeline({
        terminalID: this.terminalStore.terminalID,
        dateFrom: this.storeDateFilter.dateFrom ? (moment(this.storeDateFilter.dateFrom).toISOString()) : null,
        dateTo: this.storeDateFilter.dateTo ? (moment(this.storeDateFilter.dateTo).toISOString()) : null,
        quickDateFilter: this.storeDateFilter.quickDateType,
        altQuickDateFilter: this.storeDateFilter.altQuickDateFilter
      });

      if(!report || report.length < 3){
        this.nothingToShow = true;
        return;
      }
      this.report = report;
      this.chartData.labels = this.lodash.map(report.givenPeriod, e => e.dimension);
      this.chartData.datasets[0].data = this.lodash.map(report.givenPeriod, e => e.measure);
      this.chartOptions.scales.yAxes[0].ticks.max = Math.ceil(this.lodash.max(this.chartData.datasets[0].data) / 10) * 10;
      this.chartOptions.scales.yAxes[0].ticks.stepSize = Math.ceil(this.lodash.meanBy(this.chartData.datasets[0].data) / 10) * 10;

      this.chartData.datasets[1].data = this.lodash.map(report.altPeriod, e => e.measure);
      // this.chartOptions.scales.yAxes[1].ticks.max = Math.ceil(this.lodash.max(this.chartData.datasets[1].data) / 10) * 10;
      // this.chartOptions.scales.yAxes[1].ticks.stepSize = Math.ceil(this.lodash.meanBy(this.chartData.datasets[1].data) / 10) * 10;


      this.draw = true;
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
.chart-container{
  flex-grow: 1;
  min-height: 0;

  > div {
    position: relative;
    height: 250px;
  }
}
.main-line-color{
  color: #fd8442bf;
}
.ref-line-color{
  color: #8e8e8e;
}
</style>