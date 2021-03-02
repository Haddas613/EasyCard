<template>
  <v-card flat class="my-2">
    <v-card-title class="px-2 py-2 ecdgray--text subtitle-3 text-uppercase">
      <v-row no-gutters align="center">
        <v-col cols="6">{{$t("TopItems")}}</v-col>
        <v-col cols="6" class="text-none text-end body-2">
          <stats-filter></stats-filter>
        </v-col>
      </v-row>
    </v-card-title>
    <v-divider></v-divider>
    <v-card-text class="px-0">
      <ec-list class="px-2" v-if="items && items.length > 0" :items="items" dense dashed>
        <template v-slot:prepend="{ item }">
          {{item.rowN}}
        </template>
        <template v-slot:left="{ item }">
          <v-col cols="12" class="text-align-initial text-oneline">
            <span class="body-1">{{item.itemName}}</span>
          </v-col>
        </template>
        <template v-slot:right="{ item }">
          <v-col cols="12" class="text-end font-weight-bold subtitle-2">
            {{item.totalAmount | currency('ILS')}}
          </v-col>
        </template>
      </ec-list>
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
    EcList: () => import("../ec/EcList"),
    StatsFilter: () => import("./StatsFilter")
  },
  data() {
    return {
      items: null
    }
  },
  computed: {
    ...mapState({
      terminalStore: state => state.settings.terminal,
      storeDateFilter: state => state.ui.dashboardDateFilter
    }),
  },
  async mounted(){
    await this.getData();
  },
  methods: {
    async getData() {
      let report = await this.$api.reporting.dashboard.getItemsTotals({
        terminalID: this.terminalStore.terminalID,
        dateFrom: this.storeDateFilter.dateFrom ? (moment(this.storeDateFilter.dateFrom).toISOString()) : null,
        dateTo: this.storeDateFilter.dateTo ? (moment(this.storeDateFilter.dateTo).toISOString()) : null,
        quickDateFilter: this.storeDateFilter.quickDateType
      });

      if(!report || report.length === 0){
        return;
      }

      this.items = report;
    }
  },
  watch: {
    storeDateFilter(newValue, oldValue) {
      this.getData();
    }
  }
};
</script>