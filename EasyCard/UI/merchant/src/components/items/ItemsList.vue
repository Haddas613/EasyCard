<template>
  <v-card width="100%" flat>
    <v-card-title>
      <v-row no-gutters>
        <v-col cols="12">
          <v-text-field
            class="py-0 input-simple"
            single-line
            :hide-details="true"
            solo
            :label="$t('Search')"
            prepend-icon="mdi-magnify"
            append-outer-icon="mdi-refresh"
            @click:append-outer="refresh()"
            v-model="search"
            clearable
          ></v-text-field>
          <v-divider></v-divider>
        </v-col>
      </v-row>
    </v-card-title>
    <v-card-text>
      <ec-list :items="items">
        <template v-slot:prepend="{ item }">
          <v-checkbox
            v-model="item.selected"
          ></v-checkbox>
        </template>

        <template v-slot:left="{ item }">
          <v-col cols="12" md="6" lg="6" class="pt-1 caption ecgray--text">{{item.itemID}}</v-col>
          <v-col cols="12" md="6" lg="6">{{item.itemName}}</v-col>
        </template>

        <template v-slot:right="{ item }">
          <v-col cols="12" md="6" lg="6" class="text-end font-weight-bold button">
          </v-col>
          <v-col cols="12" md="6" lg="6" class="text-end font-weight-bold button">
            {{item.price | currency(item.$currency)}}
          </v-col>
        </template>

        <template v-slot:append="{ item }">
          <v-btn icon :to="{ name: 'Item', params: { id: item.$itemID } }">
            <re-icon>mdi-chevron-right</re-icon>
          </v-btn>
        </template>
      </ec-list>
    </v-card-text>
    <v-card-actions class="text-center" v-if="canLoadMore">
        <v-btn class="my-4" outlined color="primary" :loading="loading" @click="loadMore()">{{$t("LoadMore")}}</v-btn>
    </v-card-actions>
  </v-card>
</template>

<script>
import Avatar from "vue-avatar";
import { mapState } from "vuex";

export default {
  components: {
    Avatar,
    EcList: () => import("../ec/EcList"),
    ReIcon: () => import("../misc/ResponsiveIcon"),
  },
  data() {
    return {
      items: [],
      dictionaries: {},
      loading: false,
      search: null,
      searchTimeout: null,
      totalAmount: 0,
      paging: {
        take: this.$appConstants.config.ui.defaultTake,
        skip: 0
      }
    };
  },
  async mounted() {
    await this.getItems();
  },
  methods: {
    async refresh(){
      this.loading = true;
      await this.getItems();
      this.loading = false;
    },
    async getItems(extendData) {
      if(this.loading){ return; }
      let searchApply = this.search && this.search.trim().length >= 3;

      let data = await this.$api.items.getItems(this.filterParams);
      if (data) {
        this.totalAmount = data.numberOfRecords;
        if(extendData){
          this.items = [...this.items, ...data.data || []];
        }else{
          this.items = data.data || [];
        }
        this.totalAmount = data.numberOfRecords || 0;
      }
    },
    async loadMore() {
      this.paging.skip += this.paging.take;
      await this.getItems(true);
    },

    getSelectedItems() {
      return this.lodash.filter(this.items, i => i.selected);
    }
  },
  watch: {
    async search(newValue, oldValue) {
      if (this.searchTimeout) clearTimeout(this.searchTimeout);

      let searchWasAppliable = oldValue && oldValue.trim().length > 3;
      let searchApply = newValue && newValue.trim().length > 3;

      if (!searchWasAppliable && !searchApply) {
        return;
      }
      this.paging.skip = 0;
      this.searchTimeout = setTimeout(
        (async () => {
          await this.getItems();
        }).bind(this),
        1000
      );
    },
    async 'terminalStore.terminalID'(newValue){
      await this.getItems();
    },
    async 'showDeletedItems'(){
      await this.getItems();
    }
  },
  computed: {
    ...mapState({
      terminalStore: state => state.settings.terminal,
      showDeletedItems: state => state.ui.showDeletedItems,
    }),
    canLoadMore() {
      return (this.paging.take + this.paging.skip) < this.totalAmount;
    },
    filterParams(){
      return {
          search: this.search && this.search.trim().length >= 3 ? this.search : null,
          terminalID: this.terminalStore.terminalID,
          showDeleted: this.$showDeleted(this.showDeletedItems),
          ...this.paging
      };
    },
  }
};
</script>