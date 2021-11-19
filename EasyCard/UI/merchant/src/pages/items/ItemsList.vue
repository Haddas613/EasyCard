<template>
  <div>
    <items-list ref="itemsListComponent"></items-list>
  </div>
</template>

<script>
import { mapState } from "vuex";

export default {
  components: { 
    ItemsList: () => import("../../components/items/ItemsList")
  },
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
  computed: {
    ...mapState({
      showDeletedItems: state => state.ui.showDeletedItems,
    })
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
    initThreeDotMenu(){
      let tdm = [
        {
          text: this.$t("CreateItem"),
          fn: () => this.$router.push({name: 'CreateItem'})
        },
        {
          text: this.$t("DeleteSelected"),
          fn: () => this.deleteSelected(),
          disabled: this.showDeletedItems
        },
        {
          text: this.showDeletedItems ? this.$t("ShowActive") : this.$t("ShowDeleted"),
          fn: () => this.$store.commit("ui/setShowDeletedItems", !this.showDeletedItems)
        }
      ];
      this.$store.commit("ui/changeHeader", {
        value: {
          threeDotMenu: tdm,
          text: { translate: true, value: this.showDeletedItems ? "DeletedItems" : "Items" }
        }
      });
    }
  },
  async mounted() {
    this.initThreeDotMenu();
  },
  watch: {
    showDeletedItems(newValue, oldValue) {
      this.initThreeDotMenu();
    }
  },
};
</script>
