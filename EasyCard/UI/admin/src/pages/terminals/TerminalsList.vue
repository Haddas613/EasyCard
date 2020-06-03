<template>
  <v-card class="mx-auto" outlined>
    <v-card-title>{{$t('TerminalsList')}}</v-card-title>
    <v-expansion-panels :flat="true">
      <v-expansion-panel>
        <v-expansion-panel-header class="primary white--text">
          {{$t('Filters')}}
        </v-expansion-panel-header>
        <v-expansion-panel-content>
          <div class="pt-4 pb-2">
            filter: work in progress
          </div>
        </v-expansion-panel-content>
      </v-expansion-panel>
    </v-expansion-panels>
    <v-divider></v-divider>
    <div>
      <v-data-table 
          :headers="headers"
          :items="terminals"
          :options.sync="options"
          :server-items-length="totalAmount"
          :loading="loading"
          class="elevation-1"></v-data-table>
    </div>
  </v-card>
</template>

<script>

export default {
  name: "TerminalsList",
  components: { },
  data() {
    return {
      totalAmount: 0,
      terminals: [],
      loading: true,
      options: {},
      pagination: {},
      headers: [],
      terminalsFilter: {}
    }
  },
  watch: {
    options: {
      handler: async function(){ await this.getDataFromApi() },
      deep: true
    }
  },
  methods: {
    async getDataFromApi() {
      this.loading = true;
      let data = await this.$api.terminals.get({ ...this.terminalsFilter, ...this.options });
      this.terminals = data.data;
      this.totalAmount = data.numberOfRecords;
      this.loading = false;

      if(!this.headers || this.headers.length === 0){
        this.headers = data.headers;
      }
    },
    //TODO
    async applyFilter(filter){
      this.terminalsFilter = filter;
      await this.getDataFromApi();
    }
  }
};
</script>