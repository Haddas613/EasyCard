<template>
  <v-card class="mx-auto" outlined>
    <ec-dialog :dialog.sync="showDetailsDialog">
      <template v-slot:title>{{$t('LogDetails')}}</template>
      <template>
        <div v-if="selectedLog" class="body-1 black--text">
          <h3>
            <span class="log-type">
              <span v-if="logLevels[selectedLog.logLevel]" v-bind:class="logLevels[selectedLog.logLevel].color">{{logLevels[selectedLog.logLevel].title}}</span>
              <span v-else>{{selectedLog.logLevel}}</span>
            </span>
            {{selectedLog.userName || '-NO USER-'}}, {{selectedLog.$timestamp | ecdate}}
          </h3>
          <p class="pt-1">{{$t('CorrelationID')}}: <b>{{selectedLog.correlationID}}</b></p>
          <v-switch :label="$t('FormatCode')" v-model="preMode"></v-switch>
          <div class="py-2 body-2">
            <code v-if="preMode">
              <pre>{{selectedLog.message}}</pre>
            </code>
            <p v-else>{{selectedLog.message}}</p>
          </div>
          <div class="py-2 body-2" v-if="selectedLog.exception">
            <code v-if="preMode">
              <pre>{{selectedLog.exception}}</pre>
            </code>
            <p v-else>{{selectedLog.exception}}</p>
          </div>
        </div>
      </template>
    </ec-dialog>
    <v-expansion-panels :flat="true">
      <v-expansion-panel>
        <v-expansion-panel-header>{{$t('Filters')}}</v-expansion-panel-header>
        <v-expansion-panel-content>
          <system-logs-filter :filter-data="systemLogsFilter" v-on:apply="applyFilter($event)"></system-logs-filter>
        </v-expansion-panel-content>
      </v-expansion-panel>
    </v-expansion-panels>
    <v-divider></v-divider>
    <div>
      <v-data-table
        :headers="headers"
        :items="systemLogs"
        :options.sync="options"
        :server-items-length="totalAmount"
        :loading="loading"
        :header-props="{ sortIcon: null }"
        class="elevation-1"
      >
      <template v-slot:item.timestamp="{ item }">
        {{item.$timestamp | ecdate}}
      </template> 
      <template v-slot:item.logLevel="{ item }">
          <span v-if="logLevels[item.logLevel]" v-bind:class="logLevels[item.logLevel].color">{{logLevels[item.logLevel].title}}</span>
          <span v-else>{{item.logLevel}}</span>
      </template> 
      <template v-slot:item.categoryName="{ item }">
        <small>{{item.categoryName}}</small>
      </template> 
      <template v-slot:item.message="{ item }">
        <span class="cursor-pointer primary--text" @click="showLogDetails(item)">
          {{item.message | length(100)}}...
        </span>
      </template>
      <template v-slot:item.correlationID="{ item }">
        <small>{{item.correlationID | guid}}</small>
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
    SystemLogsFilter: () => import("../../components/system-logs/SystemLogsFilter")
  },
  data() {
    return {
      totalAmount: 0,
      systemLogs: [],
      loading: false,
      options: {},
      pagination: {},
      headers: [],
      systemLogsFilter: {},
      showDetailsDialog: false,
      selectedLog: null,
      preMode: false,
      logLevels: {
        "Trace": {
          color: "ecgray--text",
          title: "TRC"
        },
        "Debug": {
          color: "ecgray--text",
          title: "DBG"
        },
        "Information": {
          color: "primary--text",
          title: "INF"
        },
        "Warning": {
          color: "accent--text",
          title: "WRN"
        },
        "Error": {
          color: "error--text",
          title: "ERR"
        },
        "Critical": {
          color: "red--text",
          title: "CRT"
        }
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
  async mounted () {
    this.$store.commit("ui/setRefreshHandler", { value: this.getDataFromApi});
  },
  methods: {
    async getDataFromApi() {
      if(this.loading) { return; }
      this.loading = true;
      let data = await this.$api.system.getSystemLogs({
        ...this.systemLogsFilter,
        ...this.options
      });
      this.systemLogs = data.data;
      this.totalAmount = data.numberOfRecords;
      this.loading = false;

      if (!this.headers || this.headers.length === 0) {
        this.headers = data.headers;
      }
    },
    async applyFilter(filter) {
      this.options.page = 1;
      this.systemLogsFilter = filter;
      await this.getDataFromApi();
    },
    showLogDetails(log){
      this.selectedLog = log;
      this.showDetailsDialog = true;
    }
  }
};
</script>
<style lang="scss" scoped>
.log-type{
  border-left: 2px inset black;
  border-right: 2px inset black;
  padding: 0 5px;
}
</style>