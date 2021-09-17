<template>
  <v-card class="mx-auto" outlined>
    <v-expansion-panels :flat="true">
      <v-expansion-panel>
        <v-expansion-panel-header>{{$t('Filters')}}</v-expansion-panel-header>
        <v-expansion-panel-content>
          <terminals-tokens-report-filter  :filter-data="terminalsFilter" v-on:apply="applyFilter($event)"></terminals-tokens-report-filter>
        </v-expansion-panel-content>
      </v-expansion-panel>
    </v-expansion-panels>
    <v-divider></v-divider>
    <div>
      <v-data-table
        :headers="headers"
        :items="terminals"
        :options.sync="options"
        :server-items-length="totalAmount"
        :loading="loading"
        :header-props="{ sortIcon: null }"
        class="elevation-1"
      >
        <template v-slot:item.merchantName="{ item }">
          <router-link v-if="item.merchantID" class="text-decoration-none" link :to="{name: 'Merchant', params: {id: item.merchantID}}">
            {{item.merchantName || item.merchantID}}
          </router-link>
        </template>
        <template v-slot:item.label="{ item }">
          <router-link class="text-decoration-none" link :to="{name: 'EditTerminal', params: {id: item.terminalID}}">
            {{item.label || item.terminalID}}
          </router-link>
        </template> 
        <template v-slot:item.status="{ item }">
          <span v-bind:class="statusColors[item.$status]">{{item.status}}</span>
        </template>
        <template v-slot:item.createdCount="{ item }">
          <span class="success--text">{{item.createdCount}}</span>
        </template>
        <template v-slot:item.updatedCount="{ item }">
          <span class="secondary--text">{{item.updatedCount}}</span>
        </template>
        <template v-slot:item.expiredCount="{ item }">
          <span class="error--text">{{item.expiredCount}}</span>
        </template>
        <!-- <template v-slot:item.actions="{ item }">
          <v-btn class="mx-1" color="secondary" outlined x-small link :to="{name: 'Terminal', params: {id: item.terminalID}}">
            <re-icon small>mdi-arrow-right</re-icon>
          </v-btn>
        </template> -->
      </v-data-table>
    </div>
  </v-card>
</template>

<script>
import moment from "moment";

export default {
  name: "TerminalsTokensReportList",
  components: {
    TerminalsTokensReportFilter: () => import("../../components/reporting/TerminalsTokensReportFilter"),
    ReIcon: () => import("../../components/misc/ResponsiveIcon"),
  },
  data() {
    return {
      totalAmount: 0,
      terminals: [],
      loading: false,
      options: {},
      pagination: {},
      headers: [],
      terminalsFilter: {
        dateFrom: this.$formatDate(moment().subtract(1, 'months')),
        dateTo: this.$formatDate(moment()),
      },
      statusColors: {
        'approved': 'success--text',
        'disabled': 'error--text',
        'pendingApproval': 'secondary--text',
      }
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
      this.loading = true;
      let data = await this.$api.reporting.cardTokens.getTerminalsTokens({
        ...this.terminalsFilter,
        ...this.options
      });

      if(!data){
        return;
      }

      this.terminals = data.data;
      this.totalAmount = data.numberOfRecords;
      this.loading = false;

      if (!this.headers || this.headers.length === 0) {
        this.headers = data.headers;//[...data.headers, { value: "actions", text: this.$t("Actions"), sortable: false  }];
      }
    },
    async applyFilter(filter) {
      this.options.page = 1;
      this.terminalsFilter = filter;
      await this.getDataFromApi();
    },
    async enable(terminal){
      let opResult = await this.$api.terminals.enableTerminal(terminal.$terminalID);
      await this.getDataFromApi();
    },
    async disable(terminal){
      let opResult = await this.$api.terminals.disableTerminal(terminal.$terminalID);
      await this.getDataFromApi();
    }
  }
};
</script>