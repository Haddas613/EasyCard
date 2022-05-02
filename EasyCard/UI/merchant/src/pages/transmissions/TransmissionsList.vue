<template>
  <v-flex>
    <transmissions-filter-dialog
      :show.sync="showDialog"
      :filter="transmissionsFilter"
      :key="transmissionsFilter.notTransmitted"
      v-on:ok="applyFilters($event)"
    ></transmissions-filter-dialog>
    <v-card class="my-2" width="100%" flat>
      <v-card-title class="pb-0">
        <v-row class="py-0" no-gutters>
          <v-col cols="8">{{$t("Overview")}}</v-col>
          <v-col cols="3" class="text-end">
            <v-btn
              class="button"
              color="primary"
              outlined
              @click="showDialog = true;"
            >{{$t('Filter')}}</v-btn>
          </v-col>
          <v-col cols="1" class="text-end">
            <v-btn icon @click="refresh()" :loading="loading">
              <v-icon color="primary">mdi-refresh</v-icon>
            </v-btn>
          </v-col>
        </v-row>
      </v-card-title>
      <v-card-text class="body-2">
        <v-row no-gutters class="py-1">
          <v-col cols="12" md="6">
            <v-row no-gutters>
              <v-col cols="12">{{$t("TotalAmount")}}:</v-col>
              <v-col cols="12" class="mt-1 font-weight-bold">
                <!-- <p class="my-1">{{ totalAmountILS | currency('ILS') }}</p>
                <p class="my-1" v-if="totalAmountUSD">{{ totalAmountUSD | currency('USD') }}</p>
                <p class="my-1" v-if="totalAmountUSD">{{ totalAmountEUR | currency('EUR') }}</p> -->
                <v-chip color="primary" small>{{ totalAmountILS | currency('ILS') }}</v-chip>
                <v-chip class="mx-2" color="success" small>{{ totalAmountUSD | currency('USD') }}</v-chip>
                <v-chip color="secondary" small>{{ totalAmountEUR | currency('EUR') }}</v-chip>
              </v-col>
            </v-row>
          </v-col>
          <v-col cols="12" md="3">
            <v-row no-gutters>
              <v-col cols="12">{{$t("PeriodShown")}}:</v-col>
              <v-col cols="12" class="font-weight-bold">
                <span dir="ltr">
                  <template v-if="transmissionsFilter.dateFrom">
                    {{transmissionsFilter.dateFrom | ecdate("L")}}
                  </template>
                  <template v-else>-</template>
                  <span>/</span>
                  <template v-if="transmissionsFilter.dateTo">
                    {{transmissionsFilter.dateTo | ecdate("L")}}
                  </template>
                  <template v-else>-</template>
                </span>
              </v-col>
            </v-row>
          </v-col>
          <v-col cols="12" md="3">
            <v-row no-gutters>
              <v-col cols="12">{{$t("OperationsCountTotal")}}:</v-col>
              <v-col cols="12" class="font-weight-bold">
                {{numberOfRecords || '-'}}
              </v-col>
            </v-row>
          </v-col>
        </v-row>
        <v-row no-gutters class="px-1 body-2">
          <v-switch 
            v-model="transmissionsFilter.success" 
            @change="getDataFromApi(false)"
            persistent-hint
            :hint="$t('SuccessfulTransmissionsSwitchTip')"
            color="success">
            <template v-slot:label>
              <small>{{$t('Successful')}}</small>
            </template>
          </v-switch>
        </v-row>
      </v-card-text>
    </v-card>
    <v-card width="100%" flat :loading="!transmissions">
      <v-card-text class="px-0">
        <transmissions-list-component :key="loading" :transmissions="transmissions" :select-limit="selectLimit" :selectable="transmissionsFilter.notTransmitted" ref="transmissionsList"></transmissions-list-component>
        <v-flex class="text-center" v-if="canLoadMore">
          <v-btn outlined color="primary" :loading="loading" @click="loadMore()">{{$t("LoadMore")}}</v-btn>
        </v-flex>
      </v-card-text>
    </v-card>
  </v-flex>
</template>

<script>
import moment from "moment";
import { mapState } from "vuex";
export default {
  name: "TransmissionsList",
  components: {
    EcList: () => import("../../components/ec/EcList"),
    ReIcon: () => import("../../components/misc/ResponsiveIcon"),
    TransmissionsListComponent: () => import("../../components/transmissions/TransmissionsList"),
    TransmissionsFilterDialog: () =>
      import("../../components/transmissions/TransmissionsFilterDialog"),
    EcDialogInvoker: () => import("../../components/ec/EcDialogInvoker")
  },
  props: {
    filters: {
      default: () => {
        return {
          success: true
        }
      },
    },
    showFiltersDialog: {
      type: Boolean,
      default: false,
      required: false
    }
  },
  data() {
    return {
      transmissions: null,
      customerInfo: null,
      moment: moment,
      loading: false,
      transmissionsFilter: {
        ...this.filters
      },
      defaultFilter: {
        take: this.$appConstants.config.ui.defaultTake,
        skip: 0,
      },
      showDialog: this.showFiltersDialog,
      numberOfRecords: 0,
      totalAmountILS: null,
      totalAmountUSD: null,
      totalAmountEUR: null,
      selectAll: false,
      selectLimit: 1000 // TODO: from config
    };
  },
  methods: {
    async getDataFromApi(extendData) {
      if (this.loading) { return; }
      this.loading = true;
      let data = await this.$api.reporting.transmissions.get({
        ...this.transmissionsFilter
      });
      if (data) {
        let transmissions = data.data || [];
        this.transmissions = extendData ? [...this.transmissions, ...transmissions] : transmissions;
        this.numberOfRecords = data.numberOfRecords || 0;
        this.totalAmountILS = data.totalAmountILS;
        this.totalAmountUSD = data.totalAmountUSD;
        this.totalAmountEUR = data.totalAmountEUR;
      }
      this.selectAll = false;
      this.loading = false;
    },
    async applyFilters(data) {
      this.transmissionsFilter = {
        ...this.filters,
        ...this.defaultFilter,
        ...data
      };
      await this.getDataFromApi();
    },
    async refresh(){
      this.transmissionsFilter.skip = 0;
      await this.getDataFromApi();
    },
    async loadMore() {
      this.transmissionsFilter.skip += this.transmissionsFilter.take;
      await this.getDataFromApi(true);
    },
    switchSelectAll(){
      if(!this.transmissionsFilter.notTransmitted){
        return this.$toasted.show(this.$t("PleaseEnableManualModeFirst"), { type: "error" });
      }

      if(this.transmissions.length > this.selectLimit){
        return this.$toasted.show(this.$t("@MaxSelectionCount").replace("@count", this.selectLimit), { type: "error" });
      }

      this.selectAll = !this.selectAll;
      for(var i of this.transmissions){
          this.$set(i, 'selected', this.selectAll);
      }
    },
    async initThreeDotMenu(){
      this.$store.commit("ui/changeHeader", {
        value: {
          threeDotMenu: [
            {
              text: this.$t("Charge"),
              fn: () => {
                this.$router.push({ name: "Charge" });
              }
            },
            // {
            //   text: this.$t("Transmit"),
            //   fn: async () => await this.transmitSelected()
            // },
            // {
            //   text: this.$t("SelectAll"),
            //   fn: () => {
            //     this.switchSelectAll();
            //   }
            // }
          ]
        }
      });
    },
  },
  computed: {
    canLoadMore() {
      return this.numberOfRecords > 0 
        && (this.transmissionsFilter.take + this.transmissionsFilter.skip) < this.numberOfRecords;
    },
    ...mapState({
      terminalStore: state => state.settings.terminal,
      currencyStore: state => state.settings.currency
    })
  },
  async mounted() {
    await this.applyFilters({
      terminalID: this.terminalStore.terminalID,
    });
    this.initThreeDotMenu();
  },
  /** Header is initialized in mounted but since components are cached (keep-alive) it's required to
    manually update menu on route change to make sure header has correct value*/
  beforeRouteEnter(to, from, next) {
    next(vm => {
      vm.initThreeDotMenu();
    });
  },
};
</script>