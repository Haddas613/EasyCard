<template>
  <v-flex>
    <invoices-filter-dialog
      :show.sync="showDialog"
      :filter="invoicesFilter"
      v-on:ok="applyFilters($event)"
    ></invoices-filter-dialog>
    <v-card class="my-2" width="100%" flat>
      <v-card-title class="pb-0">
        <v-row class="py-0" no-gutters>
          <v-col cols="8">{{$t("Overview")}}</v-col>
          <v-col cols="3" class="text-end">
            <v-btn
              class="button"
              color="primary"
              outlined
              @click="showDialog = true;"
            >{{$t('Filter')}}</v-btn>
          </v-col>
          <v-col cols="1" class="text-end">
            <v-btn icon @click="refresh()" :loading="loading">
              <v-icon color="primary">mdi-refresh</v-icon>
            </v-btn>
          </v-col>
        </v-row>
      </v-card-title>
      <v-card-text class="body-2">
        <v-row no-gutters class="py-1">
          <v-col cols="12" md="3" lg="3" xl="3">
            <v-row no-gutters>
              <v-col cols="12">{{$t("PeriodShown")}}:</v-col>
              <v-col cols="12" class="font-weight-bold">
                <span dir="ltr">{{datePeriod || '-'}}</span>
              </v-col>
            </v-row>
          </v-col>
          <v-col cols="12" md="3" lg="3" xl="3">
            <v-row no-gutters>
              <v-col cols="12">{{$t("OperationsCountTotal")}}:</v-col>
              <v-col cols="12" class="font-weight-bold">{{numberOfRecords || '-'}}</v-col>
            </v-row>
          </v-col>
        </v-row>
      </v-card-text>
    </v-card>
    <v-card width="100%" flat :loading="!invoices">
      <v-card-text class="px-0" v-if="invoices">
        <!-- <v-flex class="d-flex justify-start" v-if="$vuetify.breakpoint.mdAndUp">
          <v-btn class="mx-2" :outlined="!selectAll" @click="switchSelectAll()" color="primary" x-small>{{$t('SelectAll')}}</v-btn>
        </v-flex> -->
        <ec-list :items="invoices">
          <template v-slot:prepend="{ item }">
            <v-checkbox v-model="item.selected" :disabled="item.$status != 'initial'"></v-checkbox>
          </template>
          <template v-slot:left="{ item }">
            <v-col cols="12" md="6" lg="6" class="pt-1 caption" v-if="item.invoiceNumber">
              <b>{{item.invoiceNumber}}</b>
            </v-col>
            <v-col
              cols="12"
              md="6"
              lg="6"
              class="pt-1 caption ecgray--text"
              v-else
            >{{item.$invoiceDate | ecdate('DD/MM/YYYY HH:mm')}}</v-col>
            <v-col cols="12" md="6" lg="6">{{item.cardOwnerName || '-'}}</v-col>
          </template>

          <template v-slot:right="{ item }">
            <v-col
              cols="12"
              md="6"
              lg="6"
              class="text-end body-2"
              v-bind:class="statusColors[item.status]"
            >{{$t(item.status || 'None')}}</v-col>
            <v-col
              cols="12"
              md="6"
              lg="6"
              class="text-end font-weight-bold button"
            >{{item.currency}}{{item.invoiceAmount}}</v-col>
          </template>

          <template v-slot:append="{ item }">
            <v-btn icon :to="{ name: 'Invoice', params: { id: item.$invoiceID } }">
              <re-icon>mdi-chevron-right</re-icon>
            </v-btn>
          </template>
        </ec-list>
        <p
          class="ecgray--text text-center"
          v-if="invoices && invoices.length === 0"
        >{{$t("NothingToShow")}}</p>

        <v-flex class="text-center" v-if="canLoadMore">
          <v-btn outlined color="primary" :loading="loading" @click="loadMore()">{{$t("LoadMore")}}</v-btn>
        </v-flex>
      </v-card-text>
    </v-card>
  </v-flex>
</template>

<script>
import moment from "moment";
import { mapState } from "vuex";

export default {
  components: {
    EcList: () => import("../../components/ec/EcList"),
    ReIcon: () => import("../../components/misc/ResponsiveIcon"),
    InvoicesFilterDialog: () =>
      import("../../components/invoicing/InvoicesFilterDialog"),
    EcDialogInvoker: () => import("../../components/ec/EcDialogInvoker")
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
      invoices: null,
      statusColors: {
        Pending: "gray--text",
        None: "",
        Sent: "success--text",
        Sending: "primary--text",
        SendingFailed: "error--text"
      },
      customerInfo: null,
      moment: moment,
      loading: false,
      invoicesFilter: {
        take: 100,
        skip: 0,
        ...this.filters
      },
      showDialog: this.showFiltersDialog,
      datePeriod: null,
      numberOfRecords: 0,
      selectAll: false
    };
  },
  methods: {
    async getDataFromApi(extendData) {
      this.loading = true;
      let data = await this.$api.invoicing.get({
        ...this.invoicesFilter
      });
      if (data) {
        let invoices = data.data || [];
        this.invoices = extendData ? [...this.invoices, ...invoices] : invoices;
        this.numberOfRecords = data.numberOfRecords || 0;

        if (invoices.length > 0) {
          let newest = this.invoices[0].$invoiceTimestamp;
          let oldest = this.invoices[this.invoices.length - 1]
            .$invoiceTimestamp;
          this.datePeriod =
            this.$options.filters.ecdate(oldest, "L") +
            ` - ${this.$options.filters.ecdate(newest, "L")}`;
        } else {
          this.datePeriod = null;
        }
      }
      this.selectAll = false;
      this.loading = false;
    },
    async applyFilters(data) {
      this.invoicesFilter = {
        ...data,
        skip: 0,
        take: 100
      };
      await this.getDataFromApi();
    },
    async refresh() {
      this.invoicesFilter.skip = 0;
      await this.getDataFromApi();
    },
    async loadMore() {
      this.invoicesFilter.skip += this.invoicesFilter.take;
      await this.getDataFromApi(true);
    },
    async resendSelectedInvoices(){
      let invoices = this.lodash.filter(this.invoices, i => i.selected && i.$status == 'initial');
      if(invoices.length === 0){
        return this.$toasted.show(this.$t("SelectInvoicesFirst"), { type: "error" });
      }

      let opResult = await this.$api.invoicing
        .resend(this.terminalStore.terminalID, this.lodash.map(invoices, i => i.$invoiceID));

      if(opResult.status === "success"){
        let $dictionaries = await this.$api.dictionaries.$getTransactionDictionaries();
        this.lodash.forEach(invoices, i => {
          i.selected = false;
          i.$status = 'sending';
          i.status = $dictionaries.invoiceStatusEnum[i.$status];
        }); 
      }
    },
    switchSelectAll(){
      this.selectAll = !this.selectAll;
      for(var i of this.invoices){
        if(i.$status == 'initial'){
          this.$set(i, 'selected', this.selectAll);
        }
      }
    }
  },
  computed: {
    ...mapState({
      terminalStore: state => state.settings.terminal,
    }),
    canLoadMore() {
      return (
        this.numberOfRecords > 0 &&
        this.invoicesFilter.take + this.invoicesFilter.skip <
          this.numberOfRecords
      );
    }
  },
  async mounted() {
    await this.getDataFromApi();
    const vm = this;
    this.$store.commit("ui/changeHeader", {
      value: {
        threeDotMenu: [
          {
            text: this.$t("Create"),
            fn: () => {
              this.$router.push({ name: "CreateInvoice" });
            }
          },
          {
            text: this.$t("ResendInvoices"),
            fn: async () => {
              await vm.resendSelectedInvoices();
            }
          },
          {
            text: this.$t("SelectAll"),
            fn: () => {
              vm.switchSelectAll();
            }
          }
        ]
      }
    });
  }
};
</script>