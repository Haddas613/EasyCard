<template>
  <v-flex>
    <ec-list :items="data" v-if="data">
      <template v-slot:prepend="{ item }" v-if="selectable">
         <v-checkbox
            v-model="item.selected"
         ></v-checkbox>
      </template>
      <template v-slot:prepend v-else>
        <v-icon>mdi-credit-card-outline</v-icon>
      </template>

      <template v-slot:left="{ item }">
        <v-col
          cols="12"
          md="6"
          lg="6"
          class="pt-1 caption ecgray--text"
        >{{item.$transactionTimestamp | ecdate('DD/MM/YYYY HH:mm')}}</v-col>
        <v-col cols="12" md="6" lg="6">{{item.cardOwnerName || '-'}}</v-col>
      </template>

      <template v-slot:right="{ item }">
        <v-col
          cols="12"
          md="6"
          lg="6"
          class="text-end body-2"
          v-bind:class="quickStatusesColors[item.quickStatus]"
        >{{$t(item.quickStatus || 'None')}}</v-col>
        <v-col
          cols="12"
          md="6"
          lg="6"
          class="text-end font-weight-bold button"
          v-bind:class="{'red--text': item.$specialTransactionType == 'refund'}"
        >{{item.currency}}{{item.transactionAmount}}</v-col>
      </template>

      <template v-slot:append="{ item }">
        <v-btn icon :to="{ name: 'Transaction', params: { id: item.$paymentTransactionID } }">
          <re-icon>mdi-chevron-right</re-icon>
        </v-btn>
      </template>
    </ec-list>
    <p
      class="ecgray--text text-center"
      v-if="data && data.length === 0"
    >{{$t("NothingToShow")}}</p>
  </v-flex>
</template>

<script>
import moment from "moment";

export default {
  components: {
    EcList: () => import("../../components/ec/EcList"),
    ReIcon: () => import("../../components/misc/ResponsiveIcon"),
  },
  props: {
    transactions: {
        required: true,
        default: () => []
    },
    selectable: {
      type: Boolean,
      required: false,
      default: false
    }
  },
  data() {
    return {
      data: this.transactions,
      quickStatusesColors: {
        Pending: "primary--text",
        None: "ecgray--text",
        Completed: "success--text",
        Failed: "error--text",
        Canceled: "accent--text"
      },
      customerInfo: null,
      moment: moment
    };
  }
};
</script>

<style lang="scss" scoped>
</style>