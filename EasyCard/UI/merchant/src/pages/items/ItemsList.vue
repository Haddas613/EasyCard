<template>
  <v-card width="100%" flat>
    <v-card-title>
      <v-row no-gutters>
        <v-col class="hidden-sm-and-down" cols="12" md="6" lg="6" xl="6">
          {{$t('Items')}}
        </v-col>
        <v-col cols="12" md="6" lg="6" xl="6" class="text-end">
          <v-btn icon @click="refresh()" :loading="loading">
            <v-icon color="primary">mdi-refresh</v-icon>
          </v-btn>
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
            <ec-money :amount="item.price" :currency="item.$currency"></ec-money>
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
        <v-btn outlined color="primary" :loading="loading" @click="loadMore()">{{$t("LoadMore")}}</v-btn>
    </v-card-actions>
  </v-card>
</template>

<script>
import EcList from "../../components/ec/EcList";
import ReIcon from "../../components/misc/ResponsiveIcon";
import EcMoney from "../../components/ec/EcMoney";

export default {
  name: "ItemsList",
  components: { EcList, ReIcon, EcMoney },
  data() {
    return {
      totalAmount: 0,
      items: [],
      dictionaries: {},
      loading: false,
      itemsFilter: {
        take: 100,
        skip: 0
      }
    };
  },
  methods: {
    async refresh(){
      this.loading = true;
      await this.getDataFromApi();
      this.loading = false;
    },
    async getDataFromApi(extendData) {
      let data = await this.$api.items.getItems(this.itemsFilter);
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
    async deleteSelected(){
      let selected = this.lodash.map(this.lodash.filter(this.items, i => i.selected), e => e.$itemID);
      await this.$api.items.bulkDeleteItems(selected);
      await this.getDataFromApi();
    },
    async loadMore() {
      this.itemsFilter.skip += this.itemsFilter.take;
      await this.getDataFromApi(true);
    }
  },
  computed: {
    canLoadMore() {
      return this.totalAmount > 0 && this.itemsFilter.skip < this.totalAmount;
    }
  },
  async mounted() {
    await this.getDataFromApi();

    this.$store.commit("ui/changeHeader", {
      value: {
        threeDotMenu: [
          {
            text: this.$t("CreateItem"),
            fn: () => {this.$router.push({name: 'CreateItem'});}
          },
          {
            text: this.$t("DeleteSelected"),
            fn: this.deleteSelected.bind(this)
          }
        ]
      }
    });
  }
};
</script>
