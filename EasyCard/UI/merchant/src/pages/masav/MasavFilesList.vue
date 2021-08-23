<template>
  <v-flex>
    <v-card class="mx-auto" outlined :loading="loading">
      <v-card-text class="px-0" v-if="masavFiles">
        <ec-list :items="masavFiles">
          <template v-slot:prepend="{ item }">
            <small>{{item.masavFileID}}</small>
          </template>
          <template v-slot:left="{ item }">
            <v-col cols="12" md="6" lg="6" class="pt-1 caption ecgray--text">
              {{item.$masavFileDate | ecdate('DD/MM/YYYY')}}
            </v-col>
            <v-col cols="12" md="6" lg="6" class="pt-1 caption">
              <span class="success--text" v-if="item.$payedDate">{{item.$payedDate | ecdate('DD/MM/YYYY HH:mm')}}</span>
              <span class="ecgray--text" v-else>-</span>
            </v-col>
          </template>

          <template v-slot:right="{ item }">
            <!-- <v-col cols="12" md="6" lg="6" class="text-end body-2">
              <v-btn outlined color="success" x-small :disabled="!item.storageReference" :title="$t('ClickToDownload')" @click="downloadMasavFile(item.$invoiceID)">
                <v-icon right color="red" size="1rem">mdi-file-outline</v-icon>
              </v-btn>
            </v-col> -->
            <v-col cols="12" md="6" lg="6" class="text-end body- 2">
              {{item.terminalName || item.terminalID}}
            </v-col>
            <v-col
              cols="12"
              md="6"
              lg="6"
              class="text-end font-weight-bold button"
            >{{item.totalAmount | currency(item.currency)}}</v-col>
          </template>

          <template v-slot:append="{ item }">
            <v-btn icon :to="{name: 'MasavFileRows', params: {id: item.masavFileID}}">
              <re-icon>mdi-chevron-right</re-icon>
            </v-btn>
          </template>
        </ec-list>
        <p
          class="ecgray--text text-center"
          v-if="masavFiles && masavFiles.length === 0"
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
    EcList: () => import("../../components/ec/EcList")
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
      masavFiles: null,
      loading: true,
      options: {},
      pagination: {},
      masavFilesFilter: {
        ...this.filters
      },
    };
  },
  async mounted () {
    await this.getDataFromApi();
  },
  methods: {
    async getDataFromApi() {
      this.loading = true;
      let data = await this.$api.masavFiles.get({
        ...this.masavFilesFilter,
        ...this.options
      });
      this.masavFiles = data.data;
      this.numberOfRecords = data.numberOfRecords;
      this.loading = false;
    },
    async applyFilter(filter) {
      this.options.page = 1;
      this.masavFilesFilter = filter;
      await this.getDataFromApi();
    },
    async downloadMasavFile(link){
      window.open(link, '_blank');
    }
  },
  computed: {
    canLoadMore() {
      return (
        this.numberOfRecords > 0 &&
        this.masavFilesFilter.take + this.masavFilesFilter.skip <
          this.numberOfRecords
      );
    }
  },
};
</script>