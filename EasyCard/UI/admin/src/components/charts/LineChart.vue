<script>
import { Line } from "vue-chartjs";
export default {
  extends: Line,
  data() {
    return {
      opts: {
        fill: true,
        label: false,
        responsive: true,
        maintainAspectRatio: false,
        legend: {
          display: false
        },
        tooltips: {
          enabled: false
        },
        scales: {
          xAxes: [
            {
              gridLines: {
                drawOnChartArea: false
              }
            }
          ],
          yAxes: [
            {
              gridLines: {
                drawOnChartArea: false,
                drawBorder: false
              }
            }
          ]
        },
        ...this.options
      }
    };
  },
  name: "LineChart",
  props: ["data", "options"],
  mounted() {
    this.renderChart(this.data, this.opts);
  }
};
</script>