<template>
  <v-card class="mx-auto" outlined>
    <v-divider></v-divider>
    <div>
      <v-data-table
        :headers="headers"
        :items="users"
        :options.sync="options"
        :server-items-length="totalAmount"
        :loading="loading"
        class="elevation-1"
      >
        <template v-slot:item.merchantID="{ item }">
          <router-link
            class="text-decoration-none"
            link
            :to="{name: 'Merchant', params: {id: item.$merchantID}}"
          >
            {{item.merchantID}}
          </router-link>
        </template>
        <template v-slot:item.actions="{ item }">
          <!-- <v-btn color="primary" outlined x-small link :to="{name: 'Merchant', params: {id: item.$userID}}">
            <v-icon small>mdi-eye</v-icon>
          </v-btn>
          <v-btn class="mx-1" color="secondary" outlined x-small link :to="{name: 'EditMerchant', params: {id: item.$userID}}">
            <v-icon small>mdi-pencil</v-icon>
          </v-btn>-->
          <!-- <router-link class="text-decoration-none" link :to="{name: 'EditMerchant', params: {id: item.$userID}}">
              <v-icon small color="secondary" class="mr-2">mdi-pencil</v-icon>
          </router-link>
          <router-link class="text-decoration-none" link :to="{name: 'Merchant', params: {id: item.$userID}}">
              <v-icon small color="primary" class="mr-2">mdi-eye</v-icon>
          </router-link>-->
          <!-- <v-icon small @click="deleteItem(item)">mdi-delete</v-icon> -->
        </template>
      </v-data-table>
    </div>
  </v-card>
</template>

<script>
export default {
  components: {},
  data() {
    return {
      totalAmount: 0,
      users: [],
      loading: true,
      options: {},
      pagination: {},
      headers: [],
      usersFilter: {}
    };
  },
  watch: {
    options: {
      handler: async function() {
        await this.getDataFromApi();
      },
      deep: true
    }
  },
  methods: {
    async getDataFromApi() {
      this.loading = true;
      let data = await this.$api.users.get({
        ...this.usersFilter,
        ...this.options
      });
      this.users = data.data;
      this.totalAmount = data.numberOfRecords;
      this.loading = false;

      if (!this.headers || this.headers.length === 0) {
        this.headers = [
          ...data.headers,
          { value: "actions", text: this.$t("Actions") }
        ];
      }
    },
    //TODO
    async applyFilter(filter) {
      this.usersFilter = filter;
      await this.getDataFromApi();
    }
  }
};
</script>