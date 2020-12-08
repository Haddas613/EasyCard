<template>
  <div>
    <div class="white">
      <v-text-field
        class="py-0 px-5 input-simple"
        single-line
        :hide-details="true"
        solo
        :label="$t('EnterNameEmailOrPhone')"
        prepend-icon="mdi-magnify"
        v-model="search"
        clearable
      ></v-text-field>
    </div>
    <template
      v-if="(showPreviouslyCharged && previouslyCharged.length > 0) && (!search || search.length < 2)"
    >
      <p class="pt-4 pb-0 px-4 body-2 font-weight-medium text-uppercase">{{$t('PreviouslyCharged')}}</p>
      <v-list two-line subheader class="py-0 fill-height">
        <v-list-item
          v-for="customer in previouslyCharged"
          :key="customer.consumerID"
          @click="selectCustomer(customer)"
        >
          <v-list-item-avatar>
            <avatar :username="customer.consumerName" :rounded="true"></avatar>
          </v-list-item-avatar>
          <v-list-item-content>
            <v-list-item-title v-text="customer.consumerName"></v-list-item-title>
            <v-list-item-subtitle
              class="caption"
              v-text="customer.consumerEmail + (customer.consumerPhoneNumber ? ' ● ' + customer.consumerPhoneNumber : '')"
            ></v-list-item-subtitle>
          </v-list-item-content>
        </v-list-item>
      </v-list>
    </template>
    <template v-if="customers.length > 0 && showGrouped">
      <div v-for="(value, key) in groupedCustomers" :key="key">
        <p class="pt-4 pb-0 px-4 body-2 font-weight-medium text-uppercase">{{key}}</p>
        <v-list two-line subheader class="py-0 fill-height">
          <v-list-item
            v-for="customer in value"
            :key="customer.consumerID"
            @click="selectCustomer(customer)"
          >
            <v-list-item-avatar>
              <avatar :username="customer.consumerName" :rounded="true"></avatar>
            </v-list-item-avatar>
            <v-list-item-content>
              <v-list-item-title v-text="customer.consumerName"></v-list-item-title>
              <v-list-item-subtitle
                class="caption"
                v-text="customer.consumerEmail + (customer.consumerPhoneNumber ? ' ● ' + customer.consumerPhoneNumber : '')"
              ></v-list-item-subtitle>
            </v-list-item-content>
          </v-list-item>
        </v-list>
      </div>
    </template>
    <template v-if="customers.length > 0 &&  !showGrouped">
      <p class="pt-4 pb-0 px-4 body-2 font-weight-medium text-uppercase">{{$t('AllCustomers')}}</p>
      <v-list two-line subheader class="py-0 fill-height">
        <v-list-item
          v-for="customer in customers"
          :key="customer.consumerID"
          @click="selectCustomer(customer)"
        >
          <v-list-item-avatar>
            <avatar :username="customer.consumerName" :rounded="true"></avatar>
          </v-list-item-avatar>
          <v-list-item-content>
            <v-list-item-title v-text="customer.consumerName"></v-list-item-title>
            <v-list-item-subtitle
              class="caption"
              v-text="customer.consumerEmail + (customer.consumerPhoneNumber ? ' ● ' + customer.consumerPhoneNumber : '')"
            ></v-list-item-subtitle>
          </v-list-item-content>
        </v-list-item>
      </v-list>
      <v-flex class="text-center" v-if="canLoadMore">
        <v-btn outlined color="primary" @click="loadMore()">{{$t("LoadMore")}}</v-btn>
      </v-flex>
    </template>
    <p v-if="customers.length === 0" class="pt-4 pb-0 px-4 body-2">{{$t('NothingToShow')}}</p>
  </div>
</template>

<script>
import Avatar from "vue-avatar";
import { mapState } from "vuex";

export default {
  components: {
    Avatar
  },
  props: {
    showPreviouslyCharged: {
      type: Boolean,
      default: false
    },
    filterByTerminal: {
      default: false
    }
  },
  data() {
    return {
      search: null,
      customers: [],
      previouslyCharged: [],
      groupedCustomers: {},
      showGrouped: false,
      searchTimeout: null,
      totalAmount: 0,
      paging: {
        take: 100,
        skip: 0
      }
    };
  },
  async mounted() {
    if (
      this.showPreviouslyCharged &&
      this.lastChargedCustomersStore.length > 0
    ) {
      this.previouslyCharged = await this.$api.consumers.getLastChargedConsumers(
        this.lastChargedCustomersStore,
        this.terminalStore.terminalID
      );
    }

    await this.getCustomers();
  },
  methods: {
    selectCustomer(customer) {
      this.$emit("ok", customer);
    },
    sort(arr, by) {
      return arr.sort((a, b) => {
        if (a[by] > b[by]) return 1;
        if (a[by] < b[by]) return -1;
        return 0;
      });
    },
    groupCustomersAlphabetically(arr) {
      this.groupedCustomers = {};
      arr = this.sort(arr, "consumerName");
      for (var i = 0; i < arr.length; i++) {
        var c = arr[i].consumerName[0].toUpperCase();
        if (this.groupedCustomers[c] && this.groupedCustomers[c].length >= 0)
          this.groupedCustomers[c].push(arr[i]);
        else {
          this.groupedCustomers[c] = [];
          this.groupedCustomers[c].push(arr[i]);
        }
      }
    },
    async getCustomers(extendData) {
      let searchApply = this.search && this.search.trim().length >= 3;

      let terminalID = null;
      if(this.filterByTerminal){
        terminalID = typeof(this.filterByTerminal) === 'string' ? this.filterByTerminal : this.terminalStore.terminalID;
      }

      let customers = await this.$api.consumers.getConsumers({
        search: searchApply ? this.search : "",
        ...this.paging,
        terminalID: terminalID
      });
      this.customers = customers.data;
      this.totalAmount = customers.numberOfRecords;

      if (extendData) {
        this.customers = [...this.customers, ...customers.data];
      } else {
        this.customers = customers.data;
      }
      /**Only show alphabetically grouped customers if total count is <= 100 and it is not search mode */
      if (!searchApply && customers.numberOfRecords <= 100) {
        this.showGrouped = true;
        this.groupCustomersAlphabetically(customers.data);
      } else {
        this.showGrouped = false;
        this.groupedCustomers = {};
      }
    },
    async loadMore() {
      this.paging.skip += this.paging.take;
      await this.getCustomers();
    }
  },
  watch: {
    async search(newValue, oldValue) {
      if (this.searchTimeout) clearTimeout(this.searchTimeout);

      let searchWasAppliable = oldValue && oldValue.trim().length >= 3;
      let searchApply = newValue && newValue.trim().length >= 3;

      if (!searchWasAppliable && !searchApply) {
        return;
      }

      this.searchTimeout = setTimeout(
        (async () => {
          await this.getCustomers();
        }).bind(this),
        1000
      );
    },
    async 'terminalStore.terminalID'(newValue){
      await this.getCustomers();
    }
  },
  computed: {
    ...mapState({
      lastChargedCustomersStore: state => state.payment.lastChargedCustomers,
      terminalStore: state => state.settings.terminal
    }),
    canLoadMore() {
      return this.totalAmount > 0 && this.paging.take < this.totalAmount && this.paging.skip < this.totalAmount;
    }
  }
};
</script>

<style lang="scss" scoped>
</style>