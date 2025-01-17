<template>
  <v-card class="mx-auto" outlined>
    <ec-dialog :dialog.sync="showDetailsDialog">
      <template v-slot:title>{{$t('LogDetails')}}</template>
      <template>
        <div v-if="selectedAudit" class="body-1 black--text">
          <h3>
            {{selectedAudit.operationDoneBy || '-NO USER-'}}, {{selectedAudit.$operationDate | ecdate}}
          </h3>
          <p class="pt-1">{{$t('OperationCode')}}: <b>{{selectedAudit.operationCode}}</b></p>
          <v-switch :label="$t('FormatCode')" v-model="preMode"></v-switch>
          <div class="py-2 body-2">
            <code v-if="preMode">
              <pre>{{selectedAudit.operationDescription}}</pre>
            </code>
            <p v-else>{{selectedAudit.operationDescription}}</p>
          </div>
        </div>
      </template>
    </ec-dialog>
    <v-expansion-panels :flat="true">
      <v-expansion-panel>
        <v-expansion-panel-header>{{$t('Filters')}}</v-expansion-panel-header>
        <v-expansion-panel-content>
          <audit-filter :filter-data="auditFilter" v-on:apply="applyFilter($event)"></audit-filter>
        </v-expansion-panel-content>
      </v-expansion-panel>
    </v-expansion-panels>
    <v-divider></v-divider>
    <div>
      <v-data-table
        :headers="headers"
        :items="audits"
        :options.sync="options"
        :server-items-length="totalAmount"
        :loading="loading"
        :header-props="{ sortIcon: null }"
        class="elevation-1"
      >
      
      <template v-slot:item.operationDate="{ item }">
        {{item.$operationDate | ecdate}}
      </template> 
      <template v-slot:item.operationCode="{ item }">
        <small>{{item.operationCode}}</small>
      </template> 
      <template v-slot:item.operationDescription="{ item }">
        <span class="cursor-pointer primary--text" @click="showDetails(item)">
          {{item.operationDescription | length(100)}}...
        </span>
      </template>
      <template v-slot:item.terminalName="{ item }">
        <v-btn v-if="item.terminalID" class="mx-1" color="secondary" outlined x-small text link :to="{name: 'EditTerminal', params: {id: item.terminalID}}">
          {{item.terminalName || item.terminalID}}
        </v-btn>
      </template>
      <template v-slot:item.merchantName="{ item }">
        <router-link class="text-decoration-none" link :to="{name: 'Merchant', params: {id: item.merchantID}}">
        {{item.merchantName || item.merchantID}}
        </router-link>
      </template>
      </v-data-table>
    </div>
  </v-card>
</template>

<script>
export default {
  components: {
    ReIcon: () => import("../../components/misc/ResponsiveIcon"),
    EcDialog: () => import("../../components/ec/EcDialog"),
    AuditFilter: () => import("../../components/audit/AuditFilter")
  },
  props: {
    filters: {
      default: () => {
        return {
          notTransmitted: false
        }
      },
    }
  },
  data() {
    return {
      totalAmount: 0,
      audits: [],
      loading: false,
      options: {},
      pagination: {},
      headers: [],
      auditFilter: {
        ...this.filters
      },
      showDetailsDialog: false,
      selectedAudit: null,
      preMode: true,
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
  async mounted () {
    this.$store.commit("ui/setRefreshHandler", { value: this.getDataFromApi});
  },
  methods: {
    async getDataFromApi() {
      if(this.loading) { return; }
      this.loading = true;
      try{
        let data = await this.$api.audit.get({
          ...this.auditFilter,
          ...this.options
        });
        this.audits = data.data;
        this.totalAmount = data.numberOfRecords;

        if (!this.headers || this.headers.length === 0) {
          this.headers = data.headers;
        }
      }finally{
        this.loading = false;
      }
    },
    async applyFilter(filter) {
      this.options.page = 1;
      this.auditFilter = filter;
      await this.getDataFromApi();
    },
    showDetails(log){
      this.selectedAudit = log;
      this.showDetailsDialog = true;
    }
  }
};
</script>