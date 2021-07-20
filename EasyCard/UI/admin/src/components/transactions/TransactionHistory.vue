<template>
  <v-flex fill-height>
    <ec-dialog :dialog.sync="showDetailsDialog">
      <template v-slot:title>{{$t('Transaction')}}</template>
      <template>
        <div v-if="selectedHistory" class="body-1 black--text">
          <h3>
            {{selectedHistory.$timestamp | ecdate}}
          </h3>
          <p class="pt-1">{{$t('CorrelationID')}}: <b>{{selectedHistory.correlationId}}</b></p>
          <v-switch :label="$t('FormatCode')" v-model="preMode"></v-switch>
          <div class="py-2 body-2">
            <code v-if="preMode">
              <pre>{{selectedHistory.operationDescription}}</pre>
            </code>
            <p v-else>{{selectedHistory.operationDescription}}</p>
          </div>
        </div>
      </template>
    </ec-dialog>
    <ec-dialog :dialog.sync="showRequestLogDialog" paddings="1">
      <template v-slot:title>{{$t('RequestLog')}}</template>
      <template>
        <div v-if="requestLog" class="body-1 black--text">
          <h3>
            {{requestLog.requestDate | ecdate}}
          </h3>
          <p class="pt-1">{{$t('CorrelationID')}}: <b>{{requestLog.correlationId}}</b></p>
          <p class="pt-1">{{$t('URL')}}: <b>{{requestLog.requestUrl}}</b></p>
          <div class="py-2 body-2">
            <v-row no-gutters>
              <v-col cols="12" md="6">
                <p>{{$t("Request")}}</p>
                <code>
                  <pre v-if="requestLog.requestBody">{{requestLog.requestBody | pretty}}</pre>
                </code>
              </v-col>
              <v-col cols="12" md="6">
                <p>{{$t("Response")}} {{requestLog.responseStatus}}</p>
                <code>
                  <pre>{{requestLog.responseBody | pretty}}</pre>
                </code>
              </v-col>
            </v-row>
          </div>
        </div>
      </template>
    </ec-dialog>
    <v-simple-table>
      <template v-slot:default>
        <thead>
          <tr>
            <th class="text-left">{{$t("Date")}}</th>
            <th class="text-left">{{$t("DoneBy")}}</th>
            <th class="text-left">{{$t("OperationCode")}}</th>
            <th class="text-left">{{$t("Description")}}</th>
            <th class="text-left">{{$t("Message")}}</th>
            <th class="text-left">{{$t("Actions")}}</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="item in items" :key="item.transactionHistoryID">
            <td>{{ item.operationDate | ecdate}}</td>
            <td>{{ item.operationDoneBy }}</td>
            <td>{{ item.operationCode }}</td>
            <td>
              <span class="cursor-pointer primary--text" @click="showDialog(item)">
                {{item.operationDescription | length(100)}}...
              </span>
            </td>
            <td>{{ item.operationMessage }}</td>
            <td>
              <v-btn color="primary" @click="loadRequestLog(item)" small>{{$t("RequestLog")}}</v-btn>
            </td>
          </tr>
        </tbody>
      </template>
    </v-simple-table>
  </v-flex>
</template>

<script>
export default {
  components: {
    EcList: () => import("../ec/EcList"),
    EcDialog: () => import("../ec/EcDialog"),
  },
  props: {
    transactionId: {
      type: String,
      required: true
    }
  },
  data() {
      return {
        showDetailsDialog: false,
        showRequestLogDialog: false,
        selectedHistory: null,
        requestLog: null,
        preMode: true,
        items: []
      }
  },
  async mounted(){
      let history = await this.$api.transactions.getHistory(this.transactionId);
      if (history && history.numberOfRecords > 0){
          this.items = history.data;
      }
  },
  methods: {
    showDialog(transactionHistory) {
      this.selectedHistory = transactionHistory;
      this.showDetailsDialog = true;
    },
    async loadRequestLog(item){
      this.requestLog = await this.$api.transactionsSystem.getCorrelationLog(this.$formatDate(item.operationDate), item.correlationId);
      this.showRequestLogDialog = true;
    }
  },
};
</script>