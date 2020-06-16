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
        @keydown.native.space.prevent
      ></v-text-field>
    </div>
    <template v-if="showPreviouslyCharged && (!search || search.length < 2)">
      <p
        class="pt-4 pb-0 px-4 body-2 font-weight-medium text-uppercase"
        v-if="previouslyCharged"
      >{{$t('PreviouslyCharged')}}</p>
      <v-list two-line subheader class="pt-0 fill-height">
        <v-list-item
          v-for="customer in previouslyCharged"
          :key="customer.customerId"
          @click="selectCustomer(customer)"
        >
          <v-list-item-avatar>
            <avatar :username="customer.fullName" :rounded="true"></avatar>
          </v-list-item-avatar>
          <v-list-item-content>
            <v-list-item-title v-text="customer.fullName"></v-list-item-title>
            <v-list-item-subtitle
              class="caption"
              v-text="customer.email + ' ● ' + customer.phoneNumber"
            ></v-list-item-subtitle>
          </v-list-item-content>
        </v-list-item>
      </v-list>
    </template>
    <div v-for="(value, key) in groupedCustomers" :key="key">
      <p class="pt-4 pb-0 px-4 body-2 font-weight-medium text-uppercase">{{key}}</p>
      <v-list two-line subheader class="pt-0 fill-height">
        <v-list-item
          v-for="customer in value"
          :key="customer.customerId"
          @click="selectCustomer(customer)"
        >
          <v-list-item-avatar>
            <avatar :username="customer.fullName" :rounded="true"></avatar>
          </v-list-item-avatar>
          <v-list-item-content>
            <v-list-item-title v-text="customer.fullName"></v-list-item-title>
            <v-list-item-subtitle
              class="caption"
              v-text="customer.email + ' ● ' + customer.phoneNumber"
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
      groupedCustomers: {}
    };
  },
  async mounted() {
    //TODO: real data
    let data = [
      {
        fullName: "John Doe",
        customerId: "test1",
        email: "john@mail.com",
        phoneNumber: "0547876543",
        lastActivity: new Date("2020-06-15")
      },
      {
        fullName: "Mitch Bar",
        customerId: "test2",
        email: "mike@mail.com",
        phoneNumber: "0547876544",
        lastActivity: new Date("2020-06-12")
      },
      {
        fullName: "Mirth Monst",
        customerId: "test22",
        email: "mirte@mail.com",
        phoneNumber: "0547876514",
        lastActivity: new Date("2020-06-12")
      },
      {
        fullName: "Brendan Fry",
        customerId: "test31",
        email: "bred@mail.com",
        phoneNumber: "0547876545",
        lastActivity: new Date("2020-06-14")
      },
      {
        fullName: "Bartholomew Beggins",
        customerId: "test54",
        email: "bart@mail.com",
        phoneNumber: "0547876345",
        lastActivity: new Date("2020-04-11")
      },
      {
        fullName: "Amy Doe",
        customerId: "test4",
        email: "amy@mail.com",
        phoneNumber: "0547876546",
        lastActivity: new Date("2020-06-11")
      },
      {
        fullName: "Liana Erzt",
        customerId: "test5",
        email: "lerzt@mail.com",
        phoneNumber: "0547876547",
        lastActivity: new Date("2020-04-15")
      },
      {
        fullName: "Antony Dire",
        customerId: "test434",
        email: "andi@mail.com",
        phoneNumber: "0547846546",
        lastActivity: new Date("2020-06-11")
      }
    ];
    this.groupCustomersAlphabetically(data.concat(), "fullName");
    this.previouslyCharged = this.sort(data.concat(), "lastActivity").slice(
      0,
      5
    );
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
    //TODO: helpers?, infinite scroll
    groupCustomersAlphabetically(arr) {
      arr = this.sort(arr, "fullName");
      for (var i = 0; i < arr.length; i++) {
        var c = arr[i].fullName[0].toUpperCase();
        if (this.customers[c] && this.customers[c].length >= 0) this.customers[c].push(arr[i]);
        else {
          this.customers[c] = [];
          this.customers[c].push(arr[i]);
        }
      }
    }
  }
};
</script>

<style lang="scss" scoped>
</style>