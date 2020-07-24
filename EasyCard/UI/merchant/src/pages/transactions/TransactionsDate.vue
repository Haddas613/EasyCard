<template>
  <v-flex>
    <v-overlay :value="loading">
      <v-progress-circular indeterminate size="64"></v-progress-circular>
    </v-overlay>

    <v-card class="mt-4" width="100%" flat>
      <v-card-title class="subtitle-2 px-4">
        <router-link class="text-decoration-none" :to="{name: 'Transactions'}">
          <re-icon class="primary--text">mdi-chevron-left</re-icon>
        </router-link>
        {{date | ecdate}}
      </v-card-title>
      <v-card-text class="px-0">
        <ec-list :items="transactions">
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
            >{{item.currency}}{{item.transactionAmount}}</v-col>
          </template>

          <template v-slot:append>
            <re-icon>mdi-chevron-right</re-icon>
          </template>
        </ec-list>
      </v-card-text>
    </v-card>
  </v-flex>
</template>

<script>
import EcList from "../../components/ec/EcList";
import ReIcon from "../../components/misc/ResponsiveIcon";
import moment from "moment";

export default {
  components: {
    EcList,
    ReIcon
  },
  data() {
    return {
      loading: false,
      date: null,
      transactions: [],
      quickStatusesColors: {
        Pending: "ecgray--text",
        None: "",
        Completed: "success--text",
        Failed: "error--text"
      },
      moment: moment
    };
  },
  methods: {
    async getDataFromApi() {
      let timeout = setTimeout(
        (() => {
          this.loading = true;
        }).bind(this),
        1000
      );
      let data = await this.$api.transactions.get({
        dateFrom: moment(this.date)
          .startOf("day")
          .format(),
        dateTo: moment(this.date)
          .endOf("day")
          .format()
      });
      if (data) {
        this.transactions = data.data || [];
        this.totalAmount = data.numberOfRecords || 0;
      }
      clearTimeout(timeout);
      this.loading = false;
    }
  },
  async mounted() {
    if (!this.$route.params.date) {
      this.$router.push("/admin/transactions/list");
    }
    this.$store.commit("ui/changeHeader", {
      value: {
        text: { translate: true, value: "Transactions" }
      }
    });

    this.date = this.$route.params.date;
    await this.getDataFromApi();
  }
};
</script>

<style lang="scss" scoped>
</style>