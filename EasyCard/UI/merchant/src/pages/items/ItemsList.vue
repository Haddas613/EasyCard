<template>
  <div>
    <items-list ref="itemsListComponent"></items-list>
  </div>
</template>

<script>
import EcList from "../../components/ec/EcList";
import ReIcon from "../../components/misc/ResponsiveIcon";
import EcMoney from "../../components/ec/EcMoney";
import ItemsList from "../../components/items/ItemsList";

export default {
  components: { EcList, ReIcon, EcMoney, ItemsList },
  data() {
    return {
      totalAmount: 0,
      items: [],
      dictionaries: {},
      loading: false,
      itemsFilter: {
        take: 100,
        skip: 0
      },
    };
  },
  methods: {
    async deleteSelected(){
      let selectedItems = this.$refs.itemsListComponent.getSelectedItems();
      let selected = this.lodash.map(this.lodash.filter(selectedItems, i => i.selected), e => e.$itemID);
      if(selected.length == 0){
        return;
      }
      await this.$api.items.bulkDeleteItems(selected);
      await this.$refs.itemsListComponent.refresh();
    },
  },
  async mounted() {
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
