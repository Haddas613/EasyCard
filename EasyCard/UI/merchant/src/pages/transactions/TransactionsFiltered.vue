<template>
  <v-flex>
    <transactions-filter-dialog :show.sync="showDialog" :filter="transactionsFilter"></transactions-filter-dialog>
    <v-card class="my-2" width="100%" flat>
      <v-card-title class="pb-0">
        <v-row class="py-0" no-gutters>
          <v-col cols="9">{{$t("Overview")}}</v-col>
          <v-col cols="2" class="text-end">
            <span class="body-1 primary--text cursor-pointer" @click="showDialog = true;">{{$t('Filter')}}</span>
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
              <v-col cols="12">{{$t("Period")}}:</v-col>
              <v-col cols="12" class="font-weight-bold">
                <!-- {{datePeriod || '-'}} -->
              </v-col>
            </v-row>
          </v-col>
          <v-col cols="12" md="3" lg="3" xl="3">
            <v-row no-gutters>
              <v-col cols="12">{{$t("OperationsCount")}}:</v-col>
              <v-col cols="12" class="font-weight-bold">
                <!-- {{totalOperationsCount || '-'}} -->
              </v-col>
            </v-row>
          </v-col>
        </v-row>
      </v-card-text>
    </v-card>
    <v-card width="100%" flat :loading="!transactions">
      <v-card-text class="px-0">
        <ec-list :items="transactions" v-if="transactions">
          <template v-slot:prepend>
            <v-icon>mdi-credit-card-outline</v-icon>
          </template>

          <template v-slot:left="{ item }">
            <v-col
              cols="12"
              md="6"
              lg="6"
              class="pt-1 caption ecgray--text"
            >{{item.paymentTransactionID}}</v-col>
            <v-col cols="12" md="6" lg="6">{{item.transactionTimestamp}}</v-col>
          </template>

          <template v-slot:right="{ item }">
            <v-col
              cols="12"
              md="6"
              lg="6"
              class="text-end body-2"
              v-bind:class="quickStatusesColors[item.quickStatus]"
            >{{$t(item.quickStatus)}}</v-col>
            <v-col
              cols="12"
              md="6"
              lg="6"
              class="text-end font-weight-bold button"
            >{{item.currency}}{{item.transactionAmount}}</v-col>
          </template>

          <template v-slot:append>
            <re-icon>mdi-chevron-right</re-icon>
          </template>
        </ec-list>
        <p
          class="ecgray--text text-center"
          v-if="transactions && transactions.length === 0"
        >{{$t("NothingToShow")}}</p>
      </v-card-text>
    </v-card>
  </v-flex>
</template>

<script>
import moment from "moment";

export default {
  components: {
    EcList: () => import("../../components/ec/EcList"),
    ReIcon: () => import("../../components/misc/ResponsiveIcon"),
    TransactionsFilterDialog: () => import("../../components/transactions/TransactionsFilterDialog"),
    EcDialogInvoker: () => import("../../components/ec/EcDialogInvoker"),
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
      transactions: null,
      quickStatusesColors: {
        Pending: "ecgray--text",
        None: "",
        Completed: "success--text",
        Failed: "error--text"
      },
      customerInfo: null,
      moment: moment,
      loading: false,
      transactionsFilter: {
        take: 50,
        skip: 0,
        ...this.filters
      },
      showDialog: this.showFiltersDialog
    };
  },
  methods: {
    async getDataFromApi() {
      this.loading = true;
      let data = await this.$api.transactions.get({
        ...this.transactionsFilter
      });
      if (data) {
        this.transactions = data.data || [];
        this.totalAmount = data.numberOfRecords || 0;
      }
      this.loading = false;
    }
  },
  async mounted() {
    await this.getDataFromApi();
  }
};
</script>

<style lang="scss" scoped>
</style>