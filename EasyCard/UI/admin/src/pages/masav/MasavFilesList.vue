<template>
  <v-card class="mx-auto" outlined>
    <!-- <v-expansion-panels :flat="true">
      <v-expansion-panel>
        <v-expansion-panel-header>{{$t('Filters')}}</v-expansion-panel-header>
        <v-expansion-panel-content>
          <masavFiles-filter :filter-data="masavFilesFilter" v-on:apply="applyFilter($event)"></masavFiles-filter>
        </v-expansion-panel-content>
      </v-expansion-panel>
    </v-expansion-panels>
    <v-divider></v-divider> -->
    <div>
      <v-data-table
        :headers="headers"
        :items="masavFiles"
        :options.sync="options"
        :server-items-length="numberOfRecords"
        :loading="loading"
        :header-props="{ sortIcon: null }"
        class="elevation-1"
      >
        <template v-slot:item.masavFileDate="{ item }">
          {{item.$masavFileDate | ecdate('DD/MM/YYYY')}}
        </template>
        <template v-slot:item.payedDate="{ item }">
          <span class="success--text">{{item.$payedDate | ecdate}}</span>
        </template>
        <template v-slot:item.numberOfRecords="{ item }">
          <b class="text-center">{{item.numberOfRecords | currency(item.currency)}}</b>
        </template>
        <template v-slot:item.terminalName="{ item }">
          <router-link class="text-decoration-none" link :to="{name: 'EditTerminal', params: {id: item.terminalID}}">
            {{item.terminalName || item.terminalID}}
          </router-link>
        </template>
        <template v-slot:item.actions="{ item }">
          <v-btn outlined color="success" small :title="$t('ClickToDownload')" @click="downloadMasavFile(item.masavFileID)">
            <v-icon color="red" size="1.25rem">mdi-file-outline</v-icon>
          </v-btn>
          <v-btn class="mx-1" color="primary" outlined small link :to="{name: 'MasavFileRows', params: {id: item.masavFileID}}">
            <re-icon small>mdi-arrow-right</re-icon>
          </v-btn>
        </template>
        <template v-slot:footer>
          <p class="text-end px-4 pt-4 mb-2 body-2">
            {{$t("TotalAmount")}}
          </p>
          <p class="text-end mx-4">
            <v-chip color="primary" small>{{ totalAmountILS | currency('ILS') }}</v-chip>
            <v-chip class="mx-2" color="success" small>{{ totalAmountUSD | currency('USD') }}</v-chip>
            <v-chip color="secondary" small>{{ totalAmountEUR | currency('EUR') }}</v-chip>
          </p>
        </template> 
      </v-data-table>
    </div>
  </v-card>
</template>

<script>
export default {
  components: {
    ReIcon: () => import("../../components/misc/ResponsiveIcon"),
  },
  props: {
    filters: {
      default: () => {
        return {
          notTransmitted: false
        }
      },
    }
  },
  data() {
    return {
      numberOfRecords: 0,
      totalAmountILS: null,
      totalAmountUSD: null,
      totalAmountEUR: null,
      masavFiles: [],
      loading: false,
      options: {},
      pagination: {},
      headers: [],
      masavFilesFilter: {
        ...this.filters
      },
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
    this.$store.commit("ui/setRefreshHandler", { value: this.getDataFromApi});
  },
  methods: {
    async getDataFromApi() {
      if(this.loading) { return; }
      this.loading = true;
      try{
        let data = await this.$api.masavFiles.get({
          ...this.masavFilesFilter,
          ...this.options
        });
        this.masavFiles = data.data;
        this.numberOfRecords = data.numberOfRecords;
        this.totalAmountILS = data.totalAmountILS;
        this.totalAmountUSD = data.totalAmountUSD;
        this.totalAmountEUR = data.totalAmountEUR;

        if (!this.headers || this.headers.length === 0) {
          this.headers = [...data.headers, { value: "actions", text: this.$t("Actions"), sortable: false  }];
        }
      }finally{
        this.loading = false;
      }
    },
    async applyFilter(filter) {
      this.options.page = 1;
      this.masavFilesFilter = filter;
      await this.getDataFromApi();
    },
    async downloadMasavFile(fileID) {
        var operation = await this.$api.masavFiles.downloadMasavFile(fileID);
        if(!this.$apiSuccess(operation)) return;
        window.open(operation.entityReference, "_blank");
    },
  }
};
</script>