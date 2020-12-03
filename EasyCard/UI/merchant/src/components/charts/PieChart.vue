<script>
import { Pie } from "vue-chartjs";
export default {
  extends: Pie,
  data() {
    return {
      opts: {
        fill: true,
        // label: false,
        responsive: true,
        maintainAspectRatio: false,
        legend: {
          display: false
        },
        tooltips: {
        //   enabled: false
        },
        ...this.options
      }
    };
  },
  name: "PieChart",
  props: ["data", "options"],
  mounted() {
    this.renderChart(this.data, this.opts);
  }
};
</script>