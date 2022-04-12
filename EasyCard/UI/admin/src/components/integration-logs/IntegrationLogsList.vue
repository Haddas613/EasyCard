<template>
  <v-card class="mx-auto" outlined>
    <ec-dialog :dialog.sync="showDetailsDialog" paddings="1">
      <template v-slot:title>{{$t('RequestLog')}}</template>
      <template>
        <div v-if="requestLog" class="body-1 black--text">
          <h3>
            {{requestLog.$messageDate | ecdate}}
          </h3>
          <p class="pt-1">{{$t('CorrelationID')}}: <b>{{requestLog.correlationId}}</b></p>
          <p class="pt-1">{{$t('URL')}}: <b>{{requestLog.address}}</b></p>
          <div class="py-2 body-2">
            <v-row no-gutters>
              <v-col cols="12" md="6">
                <p>{{$t("Request")}}</p>
                <code>
                  <pre v-if="requestLog.request">{{requestLog.request}}</pre>
                </code>
              </v-col>
              <v-col cols="12" md="6">
                <p>{{$t("Response")}} <b>{{requestLog.responseStatus}}</b></p>
                <code>
                  <pre>{{requestLog.response}}</pre>
                </code>
              </v-col>
            </v-row>
          </div>
        </div>
      </template>
    </ec-dialog>
    <div class="d-flex justify-center items-group my-2">
      <v-btn class="mx-1" v-for="i in integrations" :key="i" @click="loadLogsFor(i)" :disabled="currentIntegration == i">
        {{i}}
      </v-btn>
    </div>
    <v-divider></v-divider>
    <div>
      <v-data-table
        :headers="headers"
        :items="integrationLogs"
        :options.sync="options"
        :server-items-length="totalAmount"
        :loading="loading"
        :header-props="{ sortIcon: null }"
        class="elevation-1"
      >
        <template v-slot:item.messageDate="{ item }">
          {{item.$messageDate | ecdate}}
        </template> 
        <template v-slot:item.request="{ item }">
          <span class="cursor-pointer primary--text" @click="showLogDetails(item)">
            {{item.request | length(100)}}...
          </span>
        </template> 
        <template v-slot:item.address="{ item }">
          <span class="cursor-pointer primary--text" @click="showLogDetails(item)">
            {{item.address | length(100)}}...
          </span>
        </template>
        <template v-slot:item.correlationID="{ item }">
          <small>{{item.correlationID | guid}}</small>
        </template>
        <template v-slot:footer.page-text></template>
      </v-data-table>
    </div>
  </v-card>
</template>

<script>
export default {
  props: {
    entityId: {
      type: String,
      default: null,
      required: true
    },
  },
  components: {
    ReIcon: () => import("../../components/misc/ResponsiveIcon"),
    EcDialog: () => import("../../components/ec/EcDialog"),
  },
  data() {
    return {
      totalAmount: 0,
      integrationLogs: [],
      loading: false,
      options: {},
      pagination: {},
      headers: [],
      integrationLogsFilter: {},
      showDetailsDialog: false,
      requestLog: null,
      preMode: false,
      integrations: ["shva", "clearing-house", "upay", "nayax", "rapidone", "easy-invoice", "bit"],
      currentIntegration: null,
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
      if(this.loading || !this.currentIntegration) { return; }
      this.loading = true;
      try{
        let data = await this.$api.integrations.common.getIntegrationLogs(this.currentIntegration, this.entityId);
        this.integrationLogs = data.data;
        this.totalAmount = (data.data.length >= this.options.itemsPerPage ? data.data.length + 1 : data.data.length) * this.options.page;

        if (!this.headers || this.headers.length === 0) {
          this.headers = data.headers;
        }
      }finally{
        this.loading = false;
      }
    },
    async applyFilter(filter) {
      this.options.page = 1;
      this.integrationLogsFilter = filter;
      await this.getDataFromApi();
    },
    showLogDetails(log){
      this.requestLog = log;
      this.showDetailsDialog = true;
    },
    async loadLogsFor(integration){
      this.currentIntegration = integration;
      await this.getDataFromApi();
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