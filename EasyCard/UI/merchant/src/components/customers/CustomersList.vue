<template>
  <div>
    <customer-create-dialog
      :terminal="terminalID"
      :show.sync="customerCreateDialog"
      v-on:ok="selectCustomer($event)"
      v-if="supportCreate"
    ></customer-create-dialog>
    <v-row class="my-1" justify="space-between" no-gutters v-if="supportCreate">
      <v-spacer class="hidden-sm-and-down"></v-spacer>
      <v-col cols="12" md="4" lg="3">
        <v-btn
          color="success"
          small
          block
          class="px-1"
          @click="customerCreateDialog = true"
        >
          <v-icon left>mdi-plus</v-icon>
          {{ $t("CreateCustomer") }}
        </v-btn>
      </v-col>
    </v-row>
    <v-card class="my-2" width="100%" flat v-if="overview">
      <v-card-title class="pb-0">
        <v-row class="py-0" no-gutters>
          <v-col cols="10">{{ $t("Overview") }}</v-col>
          <v-col cols="2" class="text-end">
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
              <v-col cols="12">{{ $t("Total") }}:</v-col>
              <v-col cols="12" class="font-weight-bold">
                {{ numberOfRecords }}
                <span v-if="customers.length"
                  >({{
                    $t("@Displayed").replace("@amount", customers.length)
                  }})</span
                >
              </v-col>
            </v-row>
          </v-col>
        </v-row>
      </v-card-text>
    </v-card>
    <div class="white">
      <v-text-field
        class="py-0 px-5 input-simple"
        single-line
        :hide-details="true"
        solo
        :label="$t('EnterNameEmailPhoneOrNationalID')"
        prepend-icon="mdi-magnify"
        v-model="search"
        clearable
      ></v-text-field>
    </div>
    <template v-if="displayTable">
      <v-data-table
        :headers="headers"
        :items="customers"
        :options.sync="options"
        :server-items-length="numberOfRecords"
        :loading="loading"
        :header-props="{ sortIcon: null }"
        :footer-props="{
          'items-per-page-options': [10, 25, 50, 100],
        }"
        class="elevation-1"
      >
        <template v-slot:item.actions="{ item }">
          <v-btn
            class="mx-1"
            color="primary"
            outlined
            small
            link
            :to="{
              name: 'Customer',
              params: { id: item.consumerID },
            }"
          >
            <re-icon small>mdi-arrow-right</re-icon>
          </v-btn>
        </template>
        <template v-slot:item.active="{ item }">
          <span
            :class="{
              'error--text': !item.active,
              'success--text': item.active,
            }"
          >
            {{ item.active ? $t("Yes") : $t("No") }}
          </span>
        </template>
      </v-data-table>
    </template>
    <template v-else>
      <template
        v-if="
          showPreviouslyCharged &&
          previouslyCharged.length > 0 &&
          (!search || search.length < 2)
        "
      >
        <p class="pt-4 pb-0 px-4 body-2 font-weight-medium text-uppercase">
          {{ $t("PreviouslyCharged") }}
        </p>
        <v-list two-line subheader class="py-0 fill-height">
          <v-list-item
            v-for="customer in previouslyCharged"
            :key="customer.consumerID"
            @click="selectCustomer(customer)"
          >
            <v-list-item-avatar>
              <avatar
                :username="customer.consumerName"
                :rounded="true"
              ></avatar>
            </v-list-item-avatar>
            <v-list-item-content>
              <v-list-item-title
                v-text="customer.consumerName"
              ></v-list-item-title>
              <v-list-item-subtitle
                class="caption"
                v-text="
                  (customer.consumerEmail || '-') +
                  (customer.consumerPhone ? ' ● ' + customer.consumerPhone : '')
                "
              ></v-list-item-subtitle>
            </v-list-item-content>
          </v-list-item>
        </v-list>
      </template>
      <template v-if="customers.length > 0 && showGrouped">
        <div v-for="(value, key) in groupedCustomers" :key="key">
          <p class="pt-4 pb-0 px-4 body-2 font-weight-medium text-uppercase">
            {{ key }}
          </p>
          <v-list two-line subheader class="py-0 fill-height">
            <v-list-item
              v-for="customer in value"
              :key="customer.consumerID"
              @click="selectCustomer(customer)"
            >
              <v-list-item-avatar>
                <avatar
                  :username="customer.consumerName"
                  :rounded="true"
                ></avatar>
              </v-list-item-avatar>
              <v-list-item-content>
                <v-list-item-title
                  v-text="customer.consumerName"
                ></v-list-item-title>
                <v-list-item-subtitle
                  class="caption"
                  v-text="
                    (customer.consumerEmail || '-') +
                    (customer.consumerPhone
                      ? ' ● ' + customer.consumerPhone
                      : '')
                  "
                ></v-list-item-subtitle>
              </v-list-item-content>
            </v-list-item>
          </v-list>
        </div>
      </template>
      <template v-if="customers.length > 0 && !showGrouped">
        <p class="pt-4 pb-0 px-4 body-2 font-weight-medium text-uppercase">
          {{ $t("AllCustomers") }}
        </p>
        <v-list two-line subheader class="py-0 fill-height">
          <v-list-item
            v-for="customer in customers"
            :key="customer.consumerID"
            @click="selectCustomer(customer)"
          >
            <v-list-item-avatar>
              <avatar
                :username="customer.consumerName"
                :rounded="true"
              ></avatar>
            </v-list-item-avatar>
            <v-list-item-content>
              <v-list-item-title
                v-text="customer.consumerName"
              ></v-list-item-title>
              <v-list-item-subtitle
                class="caption"
                v-text="
                  (customer.consumerEmail || '-') +
                  (customer.consumerPhone ? ' ● ' + customer.consumerPhone : '')
                "
              ></v-list-item-subtitle>
            </v-list-item-content>
          </v-list-item>
        </v-list>
        <v-flex class="text-center" v-if="canLoadMore">
          <v-btn class="my-4" outlined color="primary" @click="loadMore()">{{
            $t("LoadMore")
          }}</v-btn>
        </v-flex>
      </template>
      <p v-if="customers.length === 0" class="pt-4 pb-0 px-4 body-2">
        {{ $t("NothingToShow") }}
      </p>
    </template>
  </div>
</template>

<script>
import Avatar from "vue-avatar";
import { mapState } from "vuex";

export default {
  components: {
    Avatar,
    CustomerCreateDialog: () =>
      import("../../components/customers/CustomerCreateDialog"),
    ReIcon: () => import("../misc/ResponsiveIcon"),
  },
  props: {
    showPreviouslyCharged: {
      type: Boolean,
      default: false,
    },
    filterByTerminal: {
      default: false,
    },
    allowShowDeleted: {
      default: false,
      type: Boolean,
    },
    overview: {
      type: Boolean,
      default: false,
    },
    supportCreate: {
      type: Boolean,
      default: false,
    },
    supportTable: {
      type: Boolean,
      default: false,
    },
  },
  data() {
    return {
      search: null,
      customers: [],
      headers: [],
      options: {
        take: this.$appConstants.config.ui.defaultTake,
        skip: 0,
      },
      defaultFilter: {
        take: this.$appConstants.config.ui.defaultTake,
        skip: 0,
      },
      previouslyCharged: [],
      groupedCustomers: {},
      showGrouped: false,
      searchTimeout: null,
      numberOfRecords: 0,
      paging: {
        take: this.$appConstants.config.ui.defaultTake,
        skip: 0,
      },
      loading: false,
      customerCreateDialog: false,
    };
  },
  async mounted() {
    if (
      this.showPreviouslyCharged &&
      this.lastChargedCustomersStore.length > 0
    ) {
      let terminalID =
        typeof this.filterByTerminal === "string"
          ? this.filterByTerminal
          : this.terminalStore.terminalID;
      this.previouslyCharged =
        await this.$api.consumers.getLastChargedConsumers(
          this.lastChargedCustomersStore,
          terminalID
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
      if (this.loading) {
        return;
      }
      this.loading = true;
      try {
        let searchApply = this.search && this.search.trim().length >= 3;

        let params = {
          search: searchApply ? this.search : "",
          ...this.paging,
          terminalID: this.terminalID,
        };

        if (this.displayTable) {
          params = {
            ...params,
            ...this.options,
            showDeleted: this.showDeletedCustomers ?
              this.$showDeleted(this.showDeletedCustomers)
              : this.displayTable ? 2 : 0,
          };
        } else {
          params.showDeleted= this.allowShowDeleted
            ? this.$showDeleted(this.showDeletedCustomers)
            : false;
        }

        let customers = await this.$api.consumers.getConsumers(params);
        this.numberOfRecords = customers.numberOfRecords;

        if (!this.headers || this.headers.length === 0) {
          this.headers = [
            ...customers.headers,
            { value: "actions", text: this.$t("Actions"), sortable: false },
          ];
        }

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
      } finally {
        this.loading = false;
      }
    },
    async loadMore() {
      this.paging.skip += this.paging.take;
      await this.getCustomers(true);
    },
    async refresh() {
      await this.getCustomers(false);
    },
  },
  watch: {
    async search(newValue, oldValue) {
      if (this.searchTimeout) clearTimeout(this.searchTimeout);

      let searchWasAppliable = oldValue && oldValue.trim().length > 3;
      let searchApply = newValue && newValue.trim().length > 3;

      if (!searchWasAppliable && !searchApply) {
        return;
      }
      this.paging.skip = 0;
      this.searchTimeout = setTimeout(
        (async () => {
          await this.getCustomers();
        }).bind(this),
        1000
      );
    },
    async "terminalStore.terminalID"(newValue) {
      await this.getCustomers();
    },
    async showDeletedCustomers() {
      if (!this.allowShowDeleted) {
        return;
      }
      await this.getCustomers();
    },
    options: {
      handler: async function () {
        await this.getCustomers(false);
      },
      deep: true,
    },
    async displayTable() {
      await this.getCustomers(false);
    },
  },
  computed: {
    ...mapState({
      lastChargedCustomersStore: (state) => state.payment.lastChargedCustomers,
      terminalStore: (state) => state.settings.terminal,
      showDeletedCustomers: (state) => state.ui.showDeletedCustomers,
    }),
    canLoadMore() {
      return this.paging.take + this.paging.skip < this.numberOfRecords;
    },
    terminalID() {
      return typeof this.filterByTerminal === "string"
        ? this.filterByTerminal
        : this.terminalStore.terminalID;
    },
    displayTable() {
      return this.$vuetify.breakpoint.mdAndUp && this.supportTable;
    },
  },
};
</script>

<style lang="scss" scoped>
</style>