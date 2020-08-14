<template>
  <v-flex>
    <v-card width="100%" flat class="hidden-sm-and-down">
      <v-card-title>{{$t('Transactions')}}</v-card-title>
    </v-card>

    <v-card class="mt-2" width="100%" flat>
      <v-card-title class="pb-0">
        <v-row class="py-0" no-gutters>
          <v-col cols="10">
            {{$t("Overview")}}
          </v-col>
          <v-col cols="1" class="text-end hidden-sm-and-down">
            <v-btn class="button" color="primary" :to="{name: 'TransactionsFiltered'}" outlined>{{$t("Filter")}}</v-btn>
          </v-col>
          <v-col cols="2" lg="1"  class="text-end">
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
              <v-col cols="12" class="font-weight-bold">{{datePeriod || '-'}}</v-col>
            </v-row>
          </v-col>
          <v-col cols="12" md="3" lg="3" xl="3">
            <v-row no-gutters>
              <v-col cols="12">{{$t("OperationsCount")}}:</v-col>
              <v-col cols="12" class="font-weight-bold">
                {{totalOperationsCount || '-'}} 
                <span v-if="totalOperationsCountShown">
                  ({{$t("@Displayed").replace("@amount", totalOperationsCountShown)}})
                </span>
              </v-col>
            </v-row>
          </v-col>
        </v-row>
      </v-card-text>
      <v-card-actions class="hidden-md-and-up">
        <v-btn class="button" color="primary" :to="{name: 'TransactionsFiltered', params:{ showFiltersDialog: true }}" block outlined>{{$t("Filter")}}</v-btn>
      </v-card-actions>
    </v-card>

    <v-card
      class="mt-4"
      width="100%"
      flat
      v-for="groupedTransaction in groupedTransactions"
      v-bind:key="groupedTransaction.groupValue.transactionDate"
    >
      <v-card-title class="subtitle-2">{{groupedTransaction.groupValue.transactionDate | ecdate}}</v-card-title>
      <v-card-text class="px-0">
        <ec-list :items="groupedTransaction.data">
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
            <v-col cols="12" md="6" lg="6">{{item.cardOwnerName}}</v-col>
          </template>

          <template v-slot:right="{ item }">
            <v-col
              cols="12"
              md="6"
              lg="6"
              class="text-end body-2"
              v-bind:class="quickStatusesColors[item.quickStatus]"
            >{{$t(item.quickStatus)}}</v-col>
            <v-col cols="12" md="6" lg="6" class="text-end font-weight-bold button">
              <ec-money :amount="item.transactionAmount" :currency="item.$currency"></ec-money>
            </v-col>
          </template>

          <template v-slot:append="{ item }">
            <v-btn icon :to="{ name: 'Transaction', params: { id: item.$paymentTransactionID } }">
              <re-icon>mdi-chevron-right</re-icon>
            </v-btn>
          </template>
        </ec-list>
        <!-- TODO: config -->
        <v-card-actions
          class="justify-center"
          v-if="groupedTransaction.groupValue.numberOfRecords > 10"
        >
          <router-link
            class="primary--text"
            link
            :to="{name: 'TransactionsDate', params: {date: groupedTransaction.groupValue.transactionDate}}"
          >{{$t("SeeMore")}} {{groupedTransaction.groupValue.numberOfRecords}}</router-link>
        </v-card-actions>
      </v-card-text>
    </v-card>
  </v-flex>
</template>

<script>
import EcList from "../../components/ec/EcList";
import ReIcon from "../../components/misc/ResponsiveIcon";
import EcMoney from "../../components/ec/EcMoney";

export default {
  name: "TransactionsList",
  components: { EcList, ReIcon, EcMoney },
  data() {
    return {
      totalAmount: 0,
      groupedTransactions: [],
      options: {},
      pagination: {},
      headers: [],
      transactionsFilter: {},
      dictionaries: {},
      quickStatusesColors: {
        Pending: "ecgray--text",
        None: "",
        Completed: "success--text",
        Failed: "error--text"
      },
      datePeriod: null,
      totalOperationsCount: null,
      totalOperationsCountShown: null,
      loading: false
    };
  },
  methods: {
    async applyFilter(filter) {
      this.transactionsFilter = filter;
      await this.getGroupedDataFromApi();
    },
    async refresh(){
      this.totalOperationsCount = null;
      this.totalOperationsCountShown = null;
      this.datePeriod = null;
      this.groupedTransactions = [];
      await this.getGroupedDataFromApi();
    },
    async getGroupedDataFromApi() {
      this.loading = true;
      let data = await this.$api.transactions.getGrouped({
        ...this.transactionsFilter,
        ...this.options
      });
      if (data) {
        this.groupedTransactions = this.groupedTransactions.concat(data);
        let newest = this.groupedTransactions[0].groupValue.transactionDate;
        let oldest =
          this.groupedTransactions.length > 1
            ? this.groupedTransactions[this.groupedTransactions.length - 1]
                .groupValue.transactionDate
            : null;
        this.totalOperationsCount = this.lodash.sumBy(
          this.groupedTransactions,
          t => t.groupValue.numberOfRecords
        );
        this.totalOperationsCountShown = this.lodash.sumBy(
          this.groupedTransactions,
          t => t.data.length
        );
        this.datePeriod =
          this.$options.filters.ecdate(newest, "L") +
          (oldest ? ` - ${this.$options.filters.ecdate(oldest, "L")}` : "");
      }
      this.loading = false;
    }
  },
  async mounted() {
    await this.getGroupedDataFromApi();

    this.$store.commit("ui/changeHeader", {
      value: {
        threeDotMenu: [
          {
            text: this.$t("Charge"),
            fn: () => {
              this.$router.push({ name: "Charge" });
            }
          }
        ]
      }
    });
  }
};
</script>
