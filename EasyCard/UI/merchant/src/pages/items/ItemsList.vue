<template>
  <div>
    <items-list-component ref="itemsListComponent"></items-list-component>
  </div>
</template>

<script>
import { mapState } from "vuex";

export default {
  name: "ItemsList",
  components: { 
    ItemsListComponent: () => import("../../components/items/ItemsList")
  },
  data() {
    return {
      totalAmount: 0,
      items: [],
      dictionaries: {},
      loading: false,
      itemsFilter: {
        take: this.$appConstants.config.ui.defaultTake,
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
    },
    /** Header is initialized in mounted but since components are cached (keep-alive) it's required to
    manually update menu on route change to make sure header has correct value*/
    $route (to, from){
      /** only update header if we returned to the same (cached) page */
      if(to.name == this.$route.name){
        this.initThreeDotMenu();
      }
    },
  },
};
</script>
