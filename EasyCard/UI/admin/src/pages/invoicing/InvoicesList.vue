<template>
  <v-flex>
    <v-card class="my-2" width="100%" flat>
      <v-expansion-panels :flat="true">
        <v-expansion-panel>
          <v-expansion-panel-header >{{$t('Filters')}}</v-expansion-panel-header>
          <v-expansion-panel-content>
            <invoices-filter :filter-data="invoicesFilter" v-on:apply="applyFilter($event)"></invoices-filter>
          </v-expansion-panel-content>
        </v-expansion-panel>
      </v-expansion-panels>
      <v-divider></v-divider>
      <v-card-text class="px-0">
        <v-data-table
          :headers="headers"
          :items="invoices"
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
          <template v-slot:item.invoiceAmount="{ item }">
            <b
              v-bind:class="{'red--text': item.$invoiceType == 'creditNote' || item.$invoiceType == 'refundInvoice'}"
              class="justify-currency">
            {{item.invoiceAmount | currency(item.currency)}}</b>
          </template>
          <template v-slot:item.actions="{ item }">
            <v-btn outlined color="success" small :disabled="item.$status != 'sent'" :title="$t('ClickToDownload')" @click="downloadInvoicePDF(item.$invoiceID)">
              <v-icon color="red" size="1.25rem">mdi-file-pdf-outline</v-icon>
            </v-btn>
            <v-btn color="primary" outlined small link :to="{name: 'Invoice', params: {id: item.$invoiceID}}">
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
  name: "Invoicing",
  components: {
    EcList: () => import("../../components/ec/EcList"),
    ReIcon: () => import("../../components/misc/ResponsiveIcon"),
    InvoicesFilter: () => import("../../components/invoicing/InvoicesFilter")
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
      invoices: [],
      statusColors: {
        pending: "gray--text",
        none: "",
        sent: "success--text",
        sending: "primary--text",
        sendingFailed: "error--text"
      },
      moment: moment,
      loading: false,
      invoicesFilter: {
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
        let data = await this.$api.invoicing.get({
          ...this.invoicesFilter,
          ...this.options
        });
        
        this.invoices = data.data;
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
      this.invoicesFilter = {
        ...data,
        skip: 0,
        take: 100
      };
      await this.getDataFromApi();
    },
    async resendSelectedInvoices() {
      let invoices = this.lodash.filter(
        this.invoices,
        i => i.selected && (i.$status != 'sending')
      );
      if (invoices.length === 0) {
        return this.$toasted.show(this.$t("SelectInvoicesFirst"), {
          type: "error"
        });
      }

      let opResult = await this.$api.invoicing.resend(
        this.lodash.map(invoices, i => i.$invoiceID)
      );

      if (opResult.status === "success") {
        await this.getDataFromApi();
      }
    },
    async downloadInvoicePDF(invoiceID){
      let opResult = await this.$api.invoicing.downloadPDF(invoiceID);

      if(opResult.status === "success" && opResult.downloadLinks){
        for(var link of opResult.downloadLinks){
          window.open(link, '_blank');
        }
      }
    }
  },
  beforeRouteEnter(to, from, next) {
    next(vm => {
      vm.$store.commit("ui/changeHeader", {
        value: {
          threeDotMenu: [
            {
              text: vm.$t("ResendInvoices"),
              fn: async () => {
                await vm.resendSelectedInvoices();
              }
            }
          ],
          refresh: async () => {
            await vm.getDataFromApi();
          }
        }
      });
    });
  }
};
</script>