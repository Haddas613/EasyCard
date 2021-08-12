<template>
  <v-card flat color="ecbg">
    <v-card-title class="hidden-sm-and-down">{{$t("CreateItem")}}</v-card-title>
    <v-card-text>
      <item-form :data="model" v-on:ok="createItem($event)"></item-form>
    </v-card-text>
  </v-card>
</template>

<script>
import ItemForm from "../../components/items/ItemForm";

export default {
  components: { ItemForm },
  data() {
    return {
      model: {
        itemName: null,
        price: 0.0,
        currency: null
      },
    };
  },
  methods: {
    async createItem(data) {
      let result = await this.$api.items.createItem(data);

      if (!this.$apiSuccess(result)) return;

      this.$router.push({ name: "Item",  params: { id: result.entityReference}})
    }
  },
};
</script>