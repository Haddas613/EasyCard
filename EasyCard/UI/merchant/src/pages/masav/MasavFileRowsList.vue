<template>
  <v-flex>
    <masav-file-details v-if="masavFile" :model="masavFile"></masav-file-details>
    <v-card class="mx-auto" flat :loading="loading">
      <v-card-text class="px-0" v-if="masavFileRows">
        <ec-list :items="masavFileRows">
          <template v-slot:prepend="{ item }">
            <router-link
              v-if="item.$paymentTransactionID"
              class="primary--text"
              link
              :to="{name: 'Transaction', params: {id: item.$paymentTransactionID}}"
            >
              <small>{{item.paymentTransactionID | guid}}</small>
            </router-link>
          </template>
          <template v-slot:left="{ item }">
            <v-col cols="12" md="6" lg="6" class="pt-1 caption ecgray--text">
              {{item.$masavFileDate | ecdate('DD/MM/YYYY')}}
            </v-col>
            <v-col cols="12" md="6" lg="6" class="pt-1 caption">
              {{item.bankcode}}:{{item.branchNumber}}:{{item.accountNumber}}
            </v-col>
          </template>

          <template v-slot:right="{ item }">
            <!-- <v-col cols="12" md="6" lg="6" class="text-end body-2">
              <v-btn outlined color="success" x-small :disabled="!item.storageReference" :title="$t('ClickToDownload')" @click="downloadMasavFile(item.$invoiceID)">
                <v-icon right color="red" size="1rem">mdi-file-outline</v-icon>
              </v-btn>
            </v-col> -->
            <v-col cols="12" md="6" lg="6" class="text-end body- 2">
              <router-link
              v-if="item.consumerID"
              class="primary--text"
              link
              :to="{name: 'Customer', params: {id: item.consumerID}}"
            >
              <small>{{item.consumerName}}</small>
            </router-link>
            </v-col>
            <v-col
              cols="12"
              md="6"
              lg="6"
              class="text-end font-weight-bold button"
            >{{item.amount | currency(item.currency)}}</v-col>
          </template>
        </ec-list>
        <p
          class="ecgray--text text-center"
          v-if="masavFileRows && masavFileRows.length === 0"
        >{{$t("NothingToShow")}}</p>

        <v-flex class="text-center" v-if="canLoadMore">
          <v-btn outlined color="primary" :loading="loading" @click="loadMore()">{{$t("LoadMore")}}</v-btn>
        </v-flex>
      </v-card-text>
    </v-card>
  </v-flex>
</template>

<script>
export default {
  components: {
    ReIcon: () => import("../../components/misc/ResponsiveIcon"),
    MasavFileDetails: () => import("../../components/details/MasavFileDetails"),
    EcList: () => import("../../components/ec/EcList")
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
      numberOfRecords: 0,
      masavFileRows: [],
      masavFile: null,
      loading: true,
      options: {},
      pagination: {},
      masavFileRowsFilter: {
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
    this.masavFile = await this.$api.masavFiles.getMasavFile(
      this.$route.params.id
    );
    await this.getDataFromApi();
  },
  methods: {
    async getDataFromApi() {
      this.loading = true;
      let data = await this.$api.masavFiles.getRows({
        ...this.masavFileRowsFilter,
        ...this.options
      });
      this.masavFileRows = data.data;
      this.numberOfRecords = data.numberOfRecords;
      this.loading = false;
    },
    async applyFilter(filter) {
      this.options.page = 1;
      this.masavFileRowsFilter = filter;
      await this.getDataFromApi();
    }
  },
  computed: {
    canLoadMore() {
      return (
        this.numberOfRecords > 0 &&
        this.masavFileRowsFilter.take + this.masavFileRowsFilter.skip <
          this.numberOfRecords
      );
    }
  },
};
</script>