<template>
  <v-card class="mx-auto" outlined>
    <v-card-title>{{$t('Transactions List')}}</v-card-title>
    <v-divider></v-divider>
    <div>
      <v-data-table 
          :headers="headers"
          :items="transactions"
          :options.sync="options"
          :server-items-length="totalAmount"
          :loading="loading"
          class="elevation-1"></v-data-table>
    </div>
  </v-card>
</template>

<script>
export default {
  name: "TransactionsList",
  data() {
    return {
      totalAmount: 0,
      transactions: [],
      loading: true,
      options: {},
      pagination: {},
      headers: []
    }
  },
  watch: {
    options: {
      handler() {
        this.getDataFromApi().then(data => {
          this.transactions = data.data;
          this.totalDesserts = data.numberOfRecords;
          this.loading = false;

          if(!this.headers || this.headers.length === 0){
            this.headers = data.headers;
          }
        });
      },
      deep: true
    }
  },
  methods: {
    async getDataFromApi() {
      this.loading = true;
      return await this.$api.transactions.get({ ...this.options });
    }
  }
};
</script>

<style lang="scss" scoped>
</style>