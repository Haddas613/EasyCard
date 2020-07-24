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
          <v-col cols="12" class="text-end font-weight-bold button">{{item.currency}}{{item.price}}</v-col>
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

export default {
  name: "ItemsList",
  components: { EcList, ReIcon },
  data() {
    return {
      totalAmount: 0,
      items: [],
      dictionaries: {}
    };
  },
  methods: {
    async getDataFromApi() {
      let timeout = setTimeout(
        (() => {
          this.loading = true;
        }).bind(this),
        1000
      );
      let data = await this.$api.items.getItems();
      if (data) {
        this.items = data.data || [];
        this.totalAmount = data.numberOfRecords || 0;
        this.loading = false;
      }
      clearTimeout(timeout);
      this.loading = false;
    }
  },
  async mounted() {
    await this.getDataFromApi();
  }
};
</script>
