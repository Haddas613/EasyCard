<template>
  <v-card class="mx-auto" outlined>
    <v-expansion-panels :flat="true">
      <v-expansion-panel>
        <v-expansion-panel-header>{{$t('Filters')}}</v-expansion-panel-header>
        <v-expansion-panel-content>
          <tokens-filter  :filter-data="tokensFilter" v-on:apply="applyFilter($event)"></tokens-filter>
        </v-expansion-panel-content>
      </v-expansion-panel>
    </v-expansion-panels>
    <v-divider></v-divider>
    <div>
      <v-data-table
        :headers="headers"
        :items="tokens"
        :options.sync="options"
        :server-items-length="totalAmount"
        :loading="loading"
        class="elevation-1"
      >
        <template v-slot:item.merchantName="{ item }">
          <router-link class="text-decoration-none" link :to="{name: 'Merchant', params: {id: item.merchantID}}">
            {{item.merchantName || item.merchantID}}
          </router-link>
        </template>    
        <template v-slot:item.terminalName="{ item }">
          <router-link class="text-decoration-none" link :to="{name: 'EditTerminal', params: {id: item.terminalID}}">
            {{item.terminalName || item.terminalID}}
          </router-link>
        </template> 
        <template v-slot:item.actions="{ item }">
          <v-btn class="mx-1" color="error" outlined small @click="deleteToken(item)">
            <v-icon small>mdi-delete</v-icon>
          </v-btn>
        </template>
      </v-data-table>
    </div>
  </v-card>
</template>

<script>
export default {
  components: {
    TokensFilter: () => import("../../components/ctokens/CardTokensFilter")
  },
  data() {
    return {
      totalAmount: 0,
      tokens: [],
      loading: true,
      options: {},
      pagination: {},
      headers: [],
      tokensFilter: {},
      isBillingAdmin: false,
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
  async mounted () {
    this.isBillingAdmin = await this.$oidc.isBillingAdmin();
  },
  methods: {
    async getDataFromApi() {
      this.loading = true;
      let data = await this.$api.cardTokens.get({
        ...this.tokensFilter,
        ...this.options
      });
      this.tokens = data.data;
      this.totalAmount = data.numberOfRecords;
      this.loading = false;

      if (!this.headers || this.headers.length === 0) {
        this.headers = [...data.headers, { value: "actions", text: this.$t("Actions") }];
      }
    },
    async applyFilter(filter) {
      this.options.page = 1;
      this.tokensFilter = filter;
      await this.getDataFromApi();
    },
    async deleteToken(token){
      await this.$api.cardTokens.deleteCardToken(token.$creditCardTokenID);
      await this.getDataFromApi();
    }
  }
};
</script>