<template>
  <v-card width="100%" flat>
    <v-card-title class="hidden-sm-and-down">{{$t('Items')}}</v-card-title>

    <v-card-text>
      <ec-list :items="items">
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
          <v-btn icon :to="{ name: 'EditItem', params: { id: item.$itemID } }">
            <re-icon>mdi-chevron-right</re-icon>
          </v-btn>
        </template>
      </ec-list>
    </v-card-text>
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
      dictionaries: {}
    };
  },
  methods: {
    async getDataFromApi() {
      let data = await this.$api.items.getItems();
      if (data) {
        this.items = data.data || [];
        this.totalAmount = data.numberOfRecords || 0;
      }
    }
  },
  async mounted() {
    await this.getDataFromApi();
  }
};
</script>
