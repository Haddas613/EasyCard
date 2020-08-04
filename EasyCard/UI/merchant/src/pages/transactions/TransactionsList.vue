<template>
  <v-flex>
    <v-card width="100%" flat class="hidden-sm-and-down">
      <v-card-title>{{$t('Transactions')}}</v-card-title>
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
            <v-col
              cols="12"
              md="6"
              lg="6"
              class="text-end font-weight-bold button"
            ><ec-money :amount="item.transactionAmount" :currency="item.$currency"></ec-money></v-col>
          </template>

          <template v-slot:append>
            <re-icon>mdi-chevron-right</re-icon>
          </template>
        </ec-list>
        <!-- TODO: config -->
        <v-card-actions class="justify-center" v-if="groupedTransaction.groupValue.numberOfRecords > 10">
          <router-link class="primary--text" link :to="{name: 'TransactionsDate', params: {date: groupedTransaction.groupValue.transactionDate}}">
            {{$t("SeeMore")}} {{groupedTransaction.groupValue.numberOfRecords}}
            </router-link>
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
      transactions: [],
      groupedTransactions: {},
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
    };
  },
  methods: {
    async applyFilter(filter) {
      this.transactionsFilter = filter;
      await this.getGroupedDataFromApi();
    },
    async getGroupedDataFromApi() {
      let data = await this.$api.transactions.getGrouped({
        ...this.transactionsFilter,
        ...this.options
      });
      if (data) {
        this.groupedTransactions = data;

        if (!this.headers || this.headers.length === 0) {
          this.headers = data.headers;
        }
      }
    }
  },
  async mounted() {
    await this.getGroupedDataFromApi();

    this.$store.commit("ui/changeHeader", {
      value: {
        threeDotMenu: [
          {
            text: this.$t("Charge"),
            fn: () => {this.$router.push({name: 'Charge'})}
          },
        ],
      }
    });
  }
};
</script>
