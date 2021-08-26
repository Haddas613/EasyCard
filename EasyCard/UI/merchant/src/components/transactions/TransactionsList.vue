<template>
  <v-flex>
    <transaction-slip-dialog ref="slipDialog" v-if="selectedTransaction" :key="selectedTransaction.$paymentTransactionID"  :transaction="selectedTransaction" :show.sync="transactionSlipDialog"></transaction-slip-dialog>
    <ec-list :items="data" v-if="data">
      <template v-slot:prepend="{ item }" v-if="selectable">
         <v-checkbox
            v-model="item.selected"
            @change="itemSelected(item)"
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
          v-bind:class="quickStatusesColors[item.$quickStatus]"
        >
        <v-btn x-small color="success" outlined v-if="item.$quickStatus == 'Completed'" @click="showSlipDialog(item)">
          <span>{{item.quickStatus}}</span>
          <v-icon class="mx-1" small :right="$vuetify.$ltr" :left="$vuetify.$ltr">
            mdi-checkbook
          </v-icon>
        </v-btn>
        <span v-else>
          {{item.quickStatus}}
        </span>
        </v-col>
        <v-col
          cols="12"
          md="6"
          lg="6"
          class="text-end font-weight-bold button"
          v-bind:class="{'red--text': item.$specialTransactionType == 'refund'}"
        >{{item.transactionAmount | currency(item.$currency)}}</v-col>
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
    TransactionSlipDialog: () => import("../../components/transactions/TransactionSlipDialog"),
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
    },
    selectLimit: {
      type: Number,
      default: 0
    }
  },
  methods: {
    itemSelected(item) {
      if(!this.selectLimit){ return; }
      if(this.lodash.countBy(this.data, d => d.selected).true > this.selectLimit){
        this.$toasted.show(this.$t("@MaxSelectionCount").replace("@count", this.selectLimit), { type: "error" });
        item.selected = false;
      }
    },
    async showSlipDialog(transaction){
      if(this.loadingTransaction){
        return;
      }
      this.loadingTransaction = true;
      this.selectedTransaction = await this.$api.transactions.getTransaction(
        transaction.$paymentTransactionID
      );
      this.loadingTransaction = false;
      this.transactionSlipDialog = true;
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
      moment: moment,
      selectedTransaction: null,
      transactionSlipDialog: false,
      loadingTransaction: false
    };
  }
};
</script>