<template>
  <v-card flat class="my-2">
    <v-card-title class="px-2 py-3 ecdgray--text subtitle-3 text-uppercase">
      <v-row no-gutters>
        <v-col cols="6">{{$t("CashFlow")}}</v-col>
        <v-col cols="6" class="text-none text-end body-2">{{$t("3Months")}}</v-col>
      </v-row>
    </v-card-title>
    <v-divider></v-divider>
    <v-card-text class="px-0">
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
    LineChart: () => import("../charts/LineChart")
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
        //labels: ["Jul", "Aug", "Sep"],
        datasets: [
          {
            pointRadius: 0,
            borderColor: '#fd8442bf',
            backgroundColor: '#fd84420d',
            // backgroundColor: '#ffa53269',
            data: []
          }
        ]
      }
    };
  },
  computed: {
    ...mapState({
      terminalStore: state => state.settings.terminal,
    }),
  },
  async mounted () {
    
     let report = await this.$api.reporting.dashboard.getTransactionTimeline({
      terminalID: this.terminalStore.terminalID,
      timelineDateFrom: moment().add(-3, 'M').toISOString(),
      timelineDateTo: moment().toISOString(),
    });

    if(!report || report.length < 3){
      this.nothingToShow = true;
      return;
    }
    this.chartData.labels = this.lodash.map(report, e => moment(e.date).format("MM/DD"));
    this.chartData.datasets[0].data = this.lodash.map(report, e => e.totalAmount);
    this.chartOptions.scales.yAxes[0].ticks.max = Math.ceil(this.lodash.max(this.chartData.datasets[0].data) / 10) * 10;
    this.chartOptions.scales.yAxes[0].ticks.stepSize = Math.ceil(this.lodash.meanBy(this.chartData.datasets[0].data) / 10) * 10;
    this.draw = true;
  },
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
</style>