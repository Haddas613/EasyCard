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

      //server errors will be displayed automatically
      if(!result)
        return;

      if(result.status === "success"){
        this.$router.push({ name: "Items"})
      }else{
        this.$toasted.show(result.message, { type: 'error' });
      }
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

<style lang="scss" scoped>
</style>