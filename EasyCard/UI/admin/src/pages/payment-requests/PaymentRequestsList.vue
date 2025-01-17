<template>
  <v-flex>
    <v-card class="my-2" width="100%" flat>
      <v-expansion-panels :flat="true">
        <v-expansion-panel>
          <v-expansion-panel-header >{{$t('Filters')}}</v-expansion-panel-header>
          <v-expansion-panel-content>
            <payment-requests-filter :filter-data="paymentRequestsFilter" v-on:apply="applyFilter($event)"></payment-requests-filter>
          </v-expansion-panel-content>
        </v-expansion-panel>
      </v-expansion-panels>
      <v-divider></v-divider>
      <v-card-text class="px-0">
        <v-data-table
          :headers="headers"
          :items="paymentRequests"
          :options.sync="options"
          :server-items-length="numberOfRecords"
          :loading="loading"
          :header-props="{ sortIcon: null }"
          class="elevation-1"
        >
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
          <template v-slot:item.status="{ item }">
            <span v-bind:class="statusColors[item.$status]">{{$t(item.status || 'None')}}</span>
          </template>
          <template v-slot:item.select="{ item }">
            <input type="checkbox" v-model="item.selected" :disabled="item.$status == 'sending'">
          </template>
          <template v-slot:item.paymentRequestAmount="{ item }">
            <b class="justify-currency">{{item.paymentRequestAmount | currency(item.currency)}}</b>
          </template>
          <template v-slot:item.paymentTransactionID="{ item }">
            <router-link v-if="item.paymentTransactionID" class="text-decoration-none" link :to="{name: 'Transaction', params: {id: item.$paymentTransactionID}}">
              {{item.paymentTransactionID | guid}}
            </router-link>
          </template> 
          <template v-slot:item.actions="{ item }">
            <v-btn color="primary" outlined small link :to="{name: 'PaymentRequest', params: {id: item.$paymentRequestID}}">
              <re-icon small>mdi-arrow-right</re-icon>
            </v-btn>
          </template>
          <template v-slot:footer>
            <p class="text-end px-4 pt-4 mb-2 body-2">
              {{$t("TotalAmount")}}
            </p>
            <p class="text-end mx-4">
              <v-chip color="primary" small>{{ totalAmountILS | currency('ILS') }}</v-chip>
              <v-chip class="mx-2" color="success" small>{{ totalAmountUSD | currency('USD') }}</v-chip>
              <v-chip color="secondary" small>{{ totalAmountEUR | currency('EUR') }}</v-chip>
            </p>
          </template> 
        </v-data-table>
      </v-card-text>
    </v-card>
  </v-flex>
</template>

<script>
import moment from "moment";

export default {
  name: "PaymentRequests",
  components: {
    EcList: () => import("../../components/ec/EcList"),
    ReIcon: () => import("../../components/misc/ResponsiveIcon"),
    PaymentRequestsFilter: () => import("../../components/payment-requests/PaymentRequestsFilter")
  },
  props: {
    filters: {
      default: null
    },
    showFiltersDialog: {
      type: Boolean,
      default: false,
      required: false
    }
  },
  data() {
    return {
      paymentRequests: [],
      statusColors: {
        initial: "gray--text",
        sending: "gray--text",
        none: "",
        sent: "primary--text",
        viewed: "teal--text",
        canceled: "amber--text",
        rejected: "accent--text",
        paymentFailed: "error--text",
        payed: "success--text",
      },
      moment: moment,
      loading: false,
      paymentRequestsFilter: {
        take: this.$appConstants.config.ui.defaultTake,
        skip: 0,
        ...this.filters
      },
      options: {},
      headers: [],
      numberOfRecords: 0,
      totalAmountILS: null,
      totalAmountUSD: null,
      totalAmountEUR: null,
      selectAll: false,
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
  methods: {
    async getDataFromApi() {
      if(this.loading) { return; }
      this.loading = true;
      try{
        let data = await this.$api.paymentRequests.get({
          ...this.paymentRequestsFilter,
          ...this.options
        });
        
        this.paymentRequests = data.data;
        this.numberOfRecords = data.numberOfRecords;
        this.totalAmountILS = data.totalAmountILS;
        this.totalAmountUSD = data.totalAmountUSD;
        this.totalAmountEUR = data.totalAmountEUR;

        if (!this.headers || this.headers.length === 0) {
          this.headers = [{ value: "select", text: "", sortable: false }, ...data.headers, { value: "actions", text: this.$t("Actions"), sortable: false }];
        }
      }finally{
        this.loading = false;
      }
    },
    async applyFilter(data) {
      this.options.page = 1;
      this.paymentRequestsFilter = {
        ...data,
        skip: 0,
        take: 100
      };
      await this.getDataFromApi();
    },
  },
  beforeRouteEnter(to, from, next) {
    next(vm => {
      vm.$store.commit("ui/setRefreshHandler", { value: vm.getDataFromApi});
    });
  }
};
</script>