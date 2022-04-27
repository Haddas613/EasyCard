<template>
  <v-card class="mx-auto" outlined>
    <v-expansion-panels :flat="true">
      <v-expansion-panel>
        <v-expansion-panel-header>{{$t('Filters')}}</v-expansion-panel-header>
        <v-expansion-panel-content>
          <tds-challenge-filter  :filter-data="tdsChallengesFilter" v-on:apply="applyFilter($event)"></tds-challenge-filter>
        </v-expansion-panel-content>
      </v-expansion-panel>
    </v-expansion-panels>
    <v-divider></v-divider>
    <div>
      <v-data-table
        :headers="headers"
        :items="tdsChallenges"
        :options.sync="options"
        :server-items-length="numberOfRecords"
        :loading="loading"
        :header-props="{ sortIcon: null }"
        class="elevation-1"
      >
        <template v-slot:item.merchantName="{ item }">
          <router-link class="text-decoration-none" link :to="{name: 'Merchant', params: {id: item.merchantID}}">
            {{item.merchantName || item.merchantID}}
          </router-link>
        </template>    
        <template v-slot:item.terminalName="{ item }">
          <router-link class="text-decoration-none" link :to="{name: 'EditTerminal', params: {id: item.terminalID}}">
            {{item.terminalName || item.terminalID}}
          </router-link>
        </template> 
        <template v-slot:item.dateFrom="{ item }">
          <b>{{item.$dateFrom | ecdate}}</b>
        </template>
        <template v-slot:item.dateTo="{ item }">
          <b>{{item.$dateTo | ecdate}}</b>
        </template>
      </v-data-table>
    </div>
  </v-card>
</template>

<script>
import moment from "moment";
import { mapState } from "vuex";

export default {
  name: "TokensTransactions",
  components: {
    TdsChallengeFilter: () => import("../../components/reporting/TDSChallengeReportFilter")
  },
  data() {
    return {
      numberOfRecords: 0,
      tdsChallenges: [],
      loading: false,
      options: {},
      pagination: {},
      headers: [],
      tdsChallengesFilter: {
        // dateFrom: this.$formatDate(moment().subtract(1, 'months')),
        // dateTo: this.$formatDate(moment())
      },
    };
  },
  watch: {
    options: {
      handler: async function() {
        await this.getDataFromApi();
      },
      deep: true
    }
  },
  beforeRouteEnter(to, from, next) {
    next(vm => {
      vm.$store.commit("ui/setRefreshHandler", { value: vm.getDataFromApi});
    });
  },
  methods: {
    async getDataFromApi() {
      if(this.loading) { return; }
      try{
        this.loading = true;
        let data = await this.$api.reporting.admin.get3DSChallengeReport({
          ...this.tdsChallengesFilter,
          ...this.options
        });
        this.tdsChallenges = data.data;
        this.numberOfRecords = data.numberOfRecords;

        if (!this.headers || this.headers.length === 0) {
          this.headers = [...data.headers];
        }
      }finally{
        this.loading = false;
      }
    },
    async applyFilter(filter) {
      this.options.page = 1;
      this.tdsChallengesFilter = filter;
      await this.getDataFromApi();
    }
  },
  computed: {
    ...mapState({
      currencyStore: state => state.settings.currency
    })
  },
};
</script>