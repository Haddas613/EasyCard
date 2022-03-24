<template>
  <v-card class="mx-auto" outlined>
    <transactions-transmit-dialog
      :show.sync="showTransmitDialog"
      v-on:ok="refresh()"
    ></transactions-transmit-dialog>
    <v-expansion-panels :flat="true">
      <v-expansion-panel>
        <v-expansion-panel-header>
          {{$t('Filters')}}
        </v-expansion-panel-header>
        <v-expansion-panel-content>
          <transactions-filter :filter-data="transactionsFilter" v-on:apply="applyFilter($event)"></transactions-filter>
        </v-expansion-panel-content>
      </v-expansion-panel>
    </v-expansion-panels>
    <v-divider></v-divider>
    <div>
      <transaction-slip-dialog v-if="selectedTransaction" :key="selectedTransaction.$paymentTransactionID" ref="slipDialog" :transaction="selectedTransaction" :show.sync="transactionSlipDialog"></transaction-slip-dialog>
      <v-data-table 
          :headers="headers"
          :items="transactions"
          :options.sync="options"
          :server-items-length="totalAmount"
          :loading="loading"
          :header-props="{ sortIcon: null }"
          class="elevation-1">
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
        <template v-slot:item.transactionAmount="{ item }">
          <b
            class="justify-currency"
            :class="{'red--text': item.specialTransactionType == 'refund'}"
          >
          {{item.transactionAmount | currency(item.currency)}}
          </b>
        </template>
        <template v-slot:item.transactionTimestamp="{ item }">
         {{item.$transactionTimestamp | ecdate}}
        </template>
        <template v-slot:item.quickStatus="{ item }">
          <span v-bind:class="quickStatusesColors[item.$quickStatus]">{{$t(item.quickStatus || 'None')}}</span>
        </template>
        <template v-slot:item.actions="{ item }">
          <v-btn class="mx-1" color="primary" outlined small link :to="{name: 'Transaction', params: {id: item.$paymentTransactionID}}">
            <re-icon small>mdi-arrow-right</re-icon>
          </v-btn>
          <v-btn v-if="(item.status == 'completed' || item.status == 'awaitingForTransmission') && item.$jDealType == 'J4'" color="orange" class="mx-1" outlined small @click="showSlipDialog(item)">
            <v-icon small>mdi-checkbook</v-icon>
          </v-btn>
        </template>    
        <template v-slot:item.cardPresence="{ item }">
          <v-icon :title="$t('Bank')" v-if="item.paymentTypeEnum == 'bank'" color="secondary">mdi-bank</v-icon>
          <span v-else :title="item.cardPresence">
            <v-icon v-if="item.$cardPresence == 'regular'" color="success">mdi-credit-card-check</v-icon>
            <v-icon v-else>mdi-credit-card-off-outline</v-icon>
          </span>
        </template>
        <template v-slot:item.cardNumber="{ item }">
          <span dir="ltr">{{item.cardNumber}}</span>
        </template>   
        <template v-slot:item.transactionType="{ item }">
          <span :title="item.transactionType">
            <v-icon v-if="item.$transactionType == 'regularDeal'" color="primary">mdi-cash</v-icon>
            <v-icon v-else-if="item.$transactionType == 'installments'" color="accent">mdi-credit-card-check</v-icon>
            <v-icon v-else color="secondary">mdi-credit-card-outline</v-icon>
          </span>
        </template>
        <template v-slot:item.paymentTransactionID="{ item }">
          <small>{{item.paymentTransactionID}}</small>
        </template> 
      </v-data-table>
    </div>
  </v-card>
</template>

<script>
export default {
  name: "Transactions",
  components: { 
    TransactionsFilter : () => import("../../components/transactions/TransactionsFilter"), 
    ReIcon: () => import("../../components/misc/ResponsiveIcon"),
    TransactionSlipDialog: () =>
      import("../../components/transactions/TransactionSlipDialog"),
    TransactionsTransmitDialog: () =>
      import("../../components/transactions/TransactionsTransmitDialog")
  },
  props: {
    filters: {
      default: () => {
        return {}
      },
    },
  },
  data() {
    return {
      totalAmount: 0,
      transactions: [],
      loading: false,
      options: {},
      pagination: {},
      headers: [],
      transactionsFilter: {
        ...this.filters
      },
      quickStatusesColors: {
        Pending: "primary--text",
        None: "ecgray--text",
        Completed: "success--text",
        Failed: "error--text",
        Canceled: "accent--text"
      },
      selectedTransaction: null,
      transactionSlipDialog: false,
      loadingTransaction: false,
      showTransmitDialog: false,
    }
  },
  watch: {
    options: {
      handler: async function(){ await this.getDataFromApi() },
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
        let data = await this.$api.transactions.get({ ...this.transactionsFilter, ...this.options });
        this.transactions = data.data;
        this.totalAmount = data.numberOfRecords;

        if(!this.headers || this.headers.length === 0){
          this.headers = [...data.headers, { value: "actions", text: this.$t("Actions"), sortable: false  }];
        }
      }finally{
        this.loading = false;
      }
    },
    async applyFilter(filter){
      this.options.page = 1;
      this.transactionsFilter = filter;
      await this.getDataFromApi();
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
  beforeRouteEnter(to, from, next) {
    next(vm => {
      vm.$store.commit("ui/changeHeader", {
        value: {
          threeDotMenu: [
            {
              text: vm.$t("TransmitAll"),
              fn: async () => {
                vm.showTransmitDialog = true;
              }
            }
          ],
          refresh: async () => {
            await vm.getDataFromApi();
          }
        }
      });
    });
  },
};
</script>