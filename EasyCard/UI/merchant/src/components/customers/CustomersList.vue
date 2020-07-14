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
    <template v-if="showPreviouslyCharged && (!search || search.length < 2)">
      <p
        class="pt-4 pb-0 px-4 body-2 font-weight-medium text-uppercase"
        v-if="previouslyCharged"
      >{{$t('PreviouslyCharged')}}</p>
      <v-list two-line subheader class="py-0 fill-height">
        <v-list-item
          v-for="customer in previouslyCharged"
          :key="customer.customerId"
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
    <div v-for="(value, key) in groupedCustomers" :key="key">
      <p class="pt-4 pb-0 px-4 body-2 font-weight-medium text-uppercase">{{key}}</p>
      <v-list two-line subheader class="py-0 fill-height">
        <v-list-item
          v-for="customer in value"
          :key="customer.customerId"
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
  </div>
</template>

<script>
import Avatar from "vue-avatar";
export default {
  components: {
    Avatar
  },
  props: {
    showPreviouslyCharged: {
      type: Boolean,
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
      searchTimeout: null
    };
  },
  async mounted() {
    await this.getCustomers(false);
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
    //TODO: helpers?
    groupCustomersAlphabetically(arr) {
      this.groupedCustomers = {};
      arr = this.sort(arr, "consumerName");
      for (var i = 0; i < arr.length; i++) {
        var c = arr[i].consumerName[0].toUpperCase();
        if (this.groupedCustomers[c] && this.groupedCustomers[c].length >= 0) this.groupedCustomers[c].push(arr[i]);
        else {
          this.groupedCustomers[c] = [];
          this.groupedCustomers[c].push(arr[i]);
        }
      }
    },
    async getCustomers(search){
      let customers = await this.$api.consumers.getConsumers();
      this.customers = customers.data;

      /**Only show alphabetically grouped customers if total count is <= 100 */
      if(!search && customers.numberOfRecords <= 100){
        this.showGrouped = true;
        this.groupCustomersAlphabetically(customers.data);
      }else{
        this.showGrouped = false;
      }
    }
  },
  watch: {
    async search(newValue, oldValue) {
      if(this.searchTimeout)
          clearTimeout(this.searchTimeout);

      if(!newValue || newValue.trim().length < 3){
        return;
      }
      this.searchTimeout = setTimeout((async () => {
        await this.getCustomers(true);
      }).bind(this), 1000);
    }
  },
};
</script>

<style lang="scss" scoped>
</style>