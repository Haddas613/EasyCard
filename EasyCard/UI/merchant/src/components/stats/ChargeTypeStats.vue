<template>
  <v-card flat class="my-2">
    <v-card-title class="px-2 py-3 ecdgray--text subtitle-3 text-uppercase">
      <v-row no-gutters>
        <v-col cols="6">{{$t("ChargeType")}}</v-col>
        <v-col cols="6" class="text-none text-end body-2">{{$t("Today")}}</v-col>
      </v-row>
    </v-card-title>
    <v-divider></v-divider>
    <v-card-text>
      <v-row>
        <v-col cols="5" class="px-0">
          <v-row no-gutters v-for="d in data" :key="d.type">
            <v-col cols="4">
              <div class="icon-circled">
                <v-icon :color="typeOpts[d.type].color">{{typeOpts[d.type].icon}}</v-icon>
              </div>
            </v-col>
            <v-col cols="8">
              {{d.label}}
              <p>
                <ec-money :amount="d.value" :currency="d.currency" bold></ec-money>
              </p>
            </v-col>
          </v-row>
        </v-col>
        <v-col cols="7" class="px-0">
          <v-container class="chart-container px-0">
            <pie-chart v-if="draw" :options="chartOptions" :data="chartData"></pie-chart>
          </v-container>
        </v-col>
      </v-row>
    </v-card-text>
  </v-card>
</template>

<script>
export default {
  components: {
    PieChart: () => import("../charts/PieChart"),
    EcMoney: () => import("../ec/EcMoney")
  },
  data() {
    return {
      draw: false, //set to true when ready to draw chart
      typeOpts: {
        card: {
          color: this.$vuetify.theme.themes.light.primary,
          icon: "mdi-credit-card-outline"
        },
        cash: {
          color: this.$vuetify.theme.themes.light.secondary,
          icon: "mdi-cash"
        },
        bitcoin: {
          color: this.$vuetify.theme.themes.light.accent,
          icon: "mdi-cash"
        }
      },
      data: [
        {
          label: "Credit Card",
          type: 'card',
          value: 500,
          currency: 'USD'
        },
        {
          label: "Cash",
          type: 'cash',
          value: 140,
          currency: 'USD'
        },
        {
          label: "Bitcoin",
          type: 'bitcoin',
          value: 26,
          currency: 'USD'
        }
      ],
      chartOptions: null,
      chartData: {
        labels: ["Credit Card", "Cash", "Bitcoin"],
        datasets: [
          {
            pointRadius: 0,
            label: "Data One",
            data: [500, 140, 26]
          }
        ]
      }
    };
  },
  mounted() {
    this.chartData.datasets[0].backgroundColor = [
      this.typeOpts["card"].color,
      this.typeOpts["cash"].color,
      this.typeOpts["bitcoin"].color
    ];
    this.draw = true;
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
  i{
    padding-right: 6px!important;
    padding-left: 6px!important;
  }
}
</style>