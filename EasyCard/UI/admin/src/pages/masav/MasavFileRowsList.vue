<template>
  <div>
    <masav-file-details oneline class="mx-2" v-if="masavFile" :model="masavFile"></masav-file-details>
    <v-card class="mx-auto" outlined>
      <!-- <v-expansion-panels :flat="true">
      <v-expansion-panel>
        <v-expansion-panel-header>{{$t('Filters')}}</v-expansion-panel-header>
        <v-expansion-panel-content>
          <masavFiles-filter :filter-data="masavFilesFilter" v-on:apply="applyFilter($event)"></masavFiles-filter>
        </v-expansion-panel-content>
      </v-expansion-panel>
      </v-expansion-panels>-->
      <v-divider></v-divider>
      <div>
        <v-data-table
          :headers="headers"
          :items="masavFiles"
          :options.sync="options"
          :server-items-length="totalAmount"
          :loading="loading"
          :header-props="{ sortIcon: null }"
          class="elevation-1"
        >
          <template v-slot:item.amount="{ item }">
            <b class="text-center">{{item.amount | currency(item.currency)}}</b>
          </template>
          <template v-slot:item.smsSent="{ item }">{{item.smsSent ? $t("Yes") : $t("No")}}</template>
          <template v-slot:item.paymentTransactionID="{ item }">
            <router-link
              v-if="item.$paymentTransactionID"
              class="primary--text"
              link
              :to="{name: 'Transaction', params: {id: item.$paymentTransactionID}}"
            >
              <small>{{item.paymentTransactionID | guid}}</small>
            </router-link>
          </template>
          <!-- <template v-slot:item.terminalName="{ item }">
          <router-link class="text-decoration-none" link :to="{name: 'EditTerminal', params: {id: item.terminalID}}">
            {{item.terminalName || item.terminalID}}
          </router-link>
          </template>-->
        </v-data-table>
      </div>
    </v-card>
  </div>
</template>

<script>
export default {
  components: {
    ReIcon: () => import("../../components/misc/ResponsiveIcon"),
    MasavFileDetails: () => import("../../components/details/MasavFileDetails")
  },
  props: {
    filters: {
      default: () => {
        return {
          notTransmitted: false
        };
      }
    }
  },
  data() {
    return {
      totalAmount: 0,
      masavFiles: [],
      masavFile: null,
      loading: false,
      options: {},
      pagination: {},
      headers: [],
      masavFilesFilter: {
        ...this.filters,
        masavFileID: this.$route.params.id
      }
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
  async mounted() {
    this.$store.commit("ui/setRefreshHandler", { value: this.getDataFromApi });
    this.masavFile = await this.$api.masavFiles.getMasavFile(
      this.$route.params.id
    );
  },
  methods: {
    async getDataFromApi() {
      if(this.loading) { return; }
      this.loading = true;
      try{
        let data = await this.$api.masavFiles.getRows({
          ...this.masavFilesFilter,
          ...this.options
        });
        this.masavFiles = data.data;
        this.totalAmount = data.numberOfRecords;

        if (!this.headers || this.headers.length === 0) {
          this.headers = data.headers;
          // this.headers = [...data.headers, { value: "actions", text: this.$t("Actions"), sortable: false  }];
        }
      }finally{
        this.loading = false;
      }
    },
    async applyFilter(filter) {
      this.options.page = 1;
      this.masavFilesFilter = filter;
      await this.getDataFromApi();
    }
  }
};
</script>