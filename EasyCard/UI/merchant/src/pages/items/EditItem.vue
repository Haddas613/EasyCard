<template>
  <v-card width="100%" flat color="ecbg">
    <v-card-title class="hidden-sm-and-down">{{$t("EditItem")}}</v-card-title>
    <v-card-text>
      <item-form :data="model" v-on:ok="updateItem($event)" v-if="model"></item-form>
    </v-card-text>
  </v-card>
</template>

<script>
import ItemForm from "../../components/items/ItemForm";

export default {
  components: { ItemForm },
  data() {
    return {
      model: null
    };
  },
  methods: {
    async updateItem(data) {
      let result = await this.$api.items.updateItem(this.$route.params.id, data);

      if (!this.$apiSuccess(result)) return;

      this.$router.push({ name: "Item",  params: { id: this.$route.params.id}})
    }
  },
  async mounted () {
    let result = await this.$api.items.getItem(this.$route.params.id);

    if(!result){
      this.$router.push({ name: "Items"})
    }
    
    this.model = result;
  },
};
</script>