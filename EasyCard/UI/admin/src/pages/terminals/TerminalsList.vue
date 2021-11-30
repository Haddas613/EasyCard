<template>
  <v-card class="mx-auto" outlined>
    <v-expansion-panels :flat="true">
      <v-expansion-panel>
        <v-expansion-panel-header>{{$t('Filters')}}</v-expansion-panel-header>
        <v-expansion-panel-content>
          <terminals-filter  :filter-data="terminalsFilter" v-on:apply="applyFilter($event)"></terminals-filter>
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
        <template v-slot:item.merchantBusinessName="{ item }">
          <router-link v-if="item.merchantID" class="text-decoration-none" link :to="{name: 'Merchant', params: {id: item.$merchantID}}">
            {{item.merchantBusinessName || item.merchantID}}
          </router-link>
        </template> 
        <template v-slot:item.status="{ item }">
          <span v-bind:class="statusColors[item.$status]">{{item.status}}</span>
        </template>
        <template v-slot:item.actions="{ item }">
          <v-btn class="mx-1" color="secondary" outlined x-small link :to="{name: 'EditTerminal', params: {id: item.$terminalID}}">
            <v-icon small>mdi-pencil</v-icon>
          </v-btn>
          <v-btn class="mx-1" color="error" outlined x-small v-if="item.$status != 'disabled'" @click="disable(item)">
            <v-icon small>mdi-cancel</v-icon>
          </v-btn>
          <v-btn class="mx-1" color="success" outlined x-small v-if="item.$status == 'disabled'" @click="enable(item)">
            <v-icon small>mdi-chevron-down-circle</v-icon>
          </v-btn>
        </template>
      </v-data-table>
    </div>
  </v-card>
</template>

<script>
export default {
  name: "Terminals",
  components: {
    TerminalsFilter: () => import("../../components/terminals/TerminalsFilter")
  },
  data() {
    return {
      totalAmount: 0,
      terminals: [],
      loading: false,
      options: {},
      pagination: {},
      headers: [],
      terminalsFilter: {},
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
      try{
        let data = await this.$api.terminals.get({
          ...this.terminalsFilter,
          ...this.options
        });
        this.terminals = data.data;
        this.totalAmount = data.numberOfRecords;

        if (!this.headers || this.headers.length === 0) {
          this.headers = [...data.headers, { value: "actions", text: this.$t("Actions"), sortable: false  }];
        }
      }finally{
        this.loading = false;
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