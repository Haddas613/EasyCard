<template>
  <v-card flat class="my-2">
    <v-card-title class="px-2 py-2 ecdgray--text subtitle-3 text-uppercase">
      <v-row no-gutters align="center">
        <v-col cols="6">{{$t("SMSSent")}}</v-col>
        <v-col cols="6" class="text-none text-end body-2">
          <stats-filter-alt></stats-filter-alt>
        </v-col>
      </v-row>
    </v-card-title>
    <v-divider></v-divider>
    <v-card-text class="px-0">
      <v-row class="px-2 black--text body-2" v-if="report">
        <v-col cols="6" md="6" class="pb-0">
          <span class="main-line-color">{{$t("Success")}}</span>
          <div>
            <b>{{report.successMeasure}}</b>
          </div>
        </v-col>
        <!-- <v-col cols="auto" class="px-0 hidden-sm-and-down text-center">
          <v-divider vertical></v-divider>
        </v-col> -->
        <v-col cols="6" md="6" class="pb-0 justify-end">
          <span class="ref-line-color">{{$t("Error")}}</span>
          <div>
            <b>{{report.errorMeasure}}</b>
          </div>
        </v-col>
      </v-row>
      <v-container class="chart-container mt-2 px-0" v-if="!nothingToShow">
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
            borderColor: '#0f99c7',
            backgroundColor: '#43b4e33d',
            // backgroundColor: '#ffa53269',
            data: []
          },
          {
            pointRadius: 0,
            borderColor: '#ff5252',
            backgroundColor: '#ff525240',
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
      return `${moment(this.report.dateFrom).format('DD/MM/YY')} ~ ${moment(this.report.dateTo).format('DD/MM/YY')}`;
    }
  },
  async mounted () {
     await this.getData();
  },
  methods: {
    async getData() {
      this.draw = false;
      let report = await this.$api.reporting.admin.getSmsTimelines({
        dateFrom: this.$formatDate(this.storeDateFilter.dateFrom),
        dateTo: this.$formatDate(this.storeDateFilter.dateTo),
        quickDateFilter: this.storeDateFilter.quickDateType,
        altQuickDateFilter: this.storeDateFilter.altQuickDateFilter
      });

      if(!report || (report.success.length < 3 && report.error.length < 3)){
        this.nothingToShow = true;
        return;
      }
      this.report = report;
      this.chartData.labels = this.lodash.map(report.success, e => e.dimension);
      
      if(report.success.length >= 3){
        this.chartData.datasets[0].data = this.lodash.map(report.success, e => e.measure);
        // this.chartOptions.scales.yAxes[0].ticks.max = Math.ceil(this.lodash.max(this.chartData.datasets[0].data) / 10) * 10;
        this.chartOptions.scales.yAxes[0].ticks.stepSize = Math.ceil(this.lodash.meanBy(this.chartData.datasets[0].data) / 10) * 10;
      }

      if(report.error.length >= 3){
        this.chartData.datasets[1].data = this.lodash.map(report.error, e => e.measure);
      }

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
  color: var(--v-primary-base);
}
.ref-line-color{
  color: var(--v-error-base);
}
</style>