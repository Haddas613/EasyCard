<template>
  <v-card class="mx-auto" outlined>
    <v-expansion-panels :flat="true">
      <v-expansion-panel>
        <v-expansion-panel-header>
          {{$t('Filters')}}
        </v-expansion-panel-header>
        <v-expansion-panel-content>
          <transmissions-filter :filter-data="transmissionsFilter" v-on:apply="applyFilter($event)"></transmissions-filter>
        </v-expansion-panel-content>
      </v-expansion-panel>
    </v-expansion-panels>
    <v-divider></v-divider>
    <div>
      <transaction-slip-dialog v-if="selectedTransaction" :key="selectedTransaction.$paymentTransactionID" ref="slipDialog" :transaction="selectedTransaction" :show.sync="transmissionslipDialog"></transaction-slip-dialog>
      <v-data-table 
          :headers="headers"
          :items="transmissions"
          :options.sync="options"
          :server-items-length="numberOfRecords"
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
        <template v-slot:item.totalAmount="{ item }">
          <b class="justify-currency">{{item.totalAmount | currency(item.currency)}}</b>
        </template>
        <template v-slot:item.solek="{ item }">
          <span class="secondary--text">{{item.solek}}</span>
        </template> 
        <template v-slot:item.actions="{ item }">
          <v-btn class="mx-1" color="primary" outlined small link :to="{name: 'Transaction', params: {id: item.$paymentTransactionID}}">
            <re-icon small>mdi-arrow-right</re-icon>
          </v-btn>
        </template>
        <template v-slot:footer>
          <p class="text-end px-4 pt-4 mb-2 body-2">
            {{$t("TotalAmount")}}: <ec-money :amount="totalAmount" bold></ec-money>
          </p>
        </template> 
      </v-data-table>
    </div>
  </v-card>
</template>

<script>
export default {
  name: "Transmissions",
  components: { 
    TransmissionsFilter : () => import("../../components/transmissions/TransmissionsFilter"), 
    ReIcon: () => import("../../components/misc/ResponsiveIcon"),
    EcMoney: () => import("../../components/ec/EcMoney"),
  },
  props: {
    filters: {
      default: () => {
        return {
          success: true
        }
      },
    },
  },
  data() {
    return {
      numberOfRecords: 0,
      totalAmount: 0,
      transmissions: [],
      loading: false,
      options: {},
      pagination: {},
      headers: [],
      transmissionsFilter: {
        ...this.filters
      },
      selectedTransaction: null,
      transmissionslipDialog: false,
      loadingTransaction: false,
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
        let data = await this.$api.reporting.transmissions.get({ ...this.transmissionsFilter, ...this.options });
        this.transmissions = data.data;
        this.numberOfRecords = data.numberOfRecords;
        this.totalAmount = data.totalAmount;

        if(!this.headers || this.headers.length === 0){
          this.headers = [...data.headers, { value: "actions", text: this.$t("Actions"), sortable: false  }];
        }
      }finally{
        this.loading = false;
      }
    },
    async applyFilter(filter){
      this.options.page = 1;
      this.transmissionsFilter = filter;
      await this.getDataFromApi();
    },
    async showSlipDialog(transaction){
      if(this.loadingTransaction){
        return;
      }
      this.loadingTransaction = true;
      this.selectedTransaction = await this.$api.transmissions.getTransaction(
        transaction.$paymentTransactionID
      );
      this.loadingTransaction = false;
      this.transmissionslipDialog = true;
    }
  }
};
</script>