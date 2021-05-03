<template>
  <v-flex>
    <payment-requests-filter-dialog
      :show.sync="showDialog"
      :filter="paymentRequestsFilter"
      v-on:ok="applyFilters($event)"
    ></payment-requests-filter-dialog>
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
    <v-card width="100%" flat :loading="!paymentRequests">
      <v-card-text class="px-0">
        <ec-list :items="paymentRequests" v-if="paymentRequests">
          <template v-slot:left="{ item }">
            <v-col cols="12" md="6" lg="6" class="pt-1 caption ecgray--text">{{item.$paymentRequestTimestamp | ecdate('DD/MM/YYYY HH:mm')}}</v-col>
            <v-col cols="12" md="6" lg="6">{{item.cardOwnerName || (item.consumerEmail || '-')}}</v-col>
          </template>

          <template v-slot:right="{ item }">
            <v-col
              cols="12"
              md="6"
              lg="6"
              class="text-end body-2"
              v-bind:class="quickStatusColors[item.$quickStatus]"
            >{{$t(item.quickStatus || 'None')}}</v-col>
            <v-col
              cols="12"
              md="6"
              lg="6"
              class="text-end font-weight-bold button"
            >{{item.paymentRequestAmount | currency(item.$currency)}}</v-col>
          </template>

          <template v-slot:append="{ item }">
            <v-btn icon :to="{ name: 'PaymentRequest', params: { id: item.$paymentRequestID } }">
              <re-icon>mdi-chevron-right</re-icon>
            </v-btn>
          </template>
        </ec-list>
        <p
          class="ecgray--text text-center"
          v-if="paymentRequests && paymentRequests.length === 0"
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

export default {
  components: {
    EcList: () => import("../../components/ec/EcList"),
    ReIcon: () => import("../../components/misc/ResponsiveIcon"),
    PaymentRequestsFilterDialog: () =>
      import("../../components/payment-requests/PaymentRequestsFilterDialog"),
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
      paymentRequests: null,
      quickStatusColors: {
        pending: "ecgray--text",
        none: "",
        completed: "success--text",
        failed: "error--text",
        canceled: "accent--text",
        overdue: "secondary--text",
        viewed: "teal--text",
      },
      customerInfo: null,
      moment: moment,
      loading: false,
      paymentRequestsFilter: {
        take: 100,
        skip: 0,
        ...this.filters
      },
      showDialog: this.showFiltersDialog,
      datePeriod: null,
      numberOfRecords: 0
    };
  },
  methods: {
    async getDataFromApi(extendData) {
      this.loading = true;
      let data = await this.$api.paymentRequests.get({
        ...this.paymentRequestsFilter
      });
      if (data) {
        let paymentRequests = data.data || [];
        this.paymentRequests = extendData ? [...this.paymentRequests, ...paymentRequests] : paymentRequests;
        this.numberOfRecords = data.numberOfRecords || 0;

        if (paymentRequests.length > 0) {
          let newest = this.paymentRequests[0].$paymentRequestTimestamp;
          let oldest = this.paymentRequests[this.paymentRequests.length - 1]
            .$paymentRequestTimestamp;
          this.datePeriod =
            this.$options.filters.ecdate(oldest, "L") +
            ` - ${this.$options.filters.ecdate(newest, "L")}`;
        } else {
          this.datePeriod = null;
        }
      }
      this.loading = false;
    },
    async applyFilters(data) {
      this.paymentRequestsFilter = {
        ...data,
        skip: 0,
        take: 100
      };
      await this.getDataFromApi();
    },
    async refresh() {
      this.paymentRequestsFilter.skip = 0;
      await this.getDataFromApi();
    },
    async loadMore() {
      this.paymentRequestsFilter.skip += this.paymentRequestsFilter.take;
      await this.getDataFromApi(true);
    }
  },
  computed: {
    canLoadMore() {
      return (
        this.numberOfRecords > 0 &&
        this.paymentRequestsFilter.take + this.paymentRequestsFilter.skip <
          this.numberOfRecords
      );
    }
  },
  async mounted() {
    await this.getDataFromApi();

    this.$store.commit("ui/changeHeader", {
      value: {
        threeDotMenu: [
          {
            text: this.$t("Create"),
            fn: () => {
              this.$router.push({ name: "CreatePaymentRequest" });
            }
          }
        ]
      }
    });
  }
};
</script>