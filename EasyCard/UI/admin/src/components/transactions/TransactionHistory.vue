<template>
  <v-flex fill-height>
    <v-simple-table>
      <template v-slot:default>
        <thead>
          <tr>
            <th class="text-left">{{$t("Date")}}</th>
            <th class="text-left">{{$t("DoneBy")}}</th>
            <th class="text-left">{{$t("OperationCode")}}</th>
            <th class="text-left">{{$t("Description")}}</th>
            <th class="text-left">{{$t("Message")}}</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="item in items" :key="item.transactionHistoryID">
            <td>{{ item.operationDate | ecdate}}</td>
            <td>{{ item.operationDoneBy }}</td>
            <td>{{ item.operationCode }}</td>
            <td>{{ item.operationDescription }}</td>
            <td>{{ item.operationMessage }}</td>
          </tr>
        </tbody>
      </template>
    </v-simple-table>
  </v-flex>
</template>

<script>
export default {
  components: {
    EcList: () => import("../ec/EcList")
  },
  props: {
    transactionId: {
      type: String,
      required: true
    }
  },
  data() {
      return {
          items: []
      }
  },
  async mounted(){
      let history = await this.$api.transactions.getHistory(this.transactionId);
      if (history && history.numberOfRecords > 0){
          this.items = history.data;
      }
  }
};
</script>