<template>
  <v-flex>
    <v-card width="100%" flat>
      <!-- <v-card-title class="subtitle-2 px-4">
        {{customerInfo.consumerName}}
      </v-card-title> -->
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

          <template v-slot:append="{ item }">
            <v-btn icon :to="{ name: 'Transaction', params: { id: item.$paymentTransactionID } }">
              <re-icon>mdi-chevron-right</re-icon>
            </v-btn>
          </template>
        </ec-list>
        <p class="ecgray--text text-center" v-if="transactions && transactions.length === 0">{{$t("NothingToShow")}}</p>
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
  props:{
    customer: {
      default: null
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
      moment: moment
    };
  },
  methods: {
    async getDataFromApi() {

      //TODO: get transactions grouped?
      let data = await this.$api.transactions.get({
        consumerID: this.$route.params.id
      });
      if (data) {
        this.transactions = data.data || [];
        this.totalAmount = data.numberOfRecords || 0;
      }
    }
  },
  async mounted() {
    if (!this.$route.params.id) {
      return this.$router.push({name: "Transactions"});
    }

    this.customerInfo = this.customer || await this.$api.consumers.getConsumer(this.$route.params.id);

    if(!this.customerInfo){
      return this.$router.push({name: "Transactions"});
    }

    this.$store.commit("ui/changeHeader", {
      value: {
        text: { translate: false, value: this.$t("@TransactionsOf").replace("@customer", this.customerInfo.consumerName) }
      }
    });

    await this.getDataFromApi();
  }
};
</script>

<style lang="scss" scoped>
</style>