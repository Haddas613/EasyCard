<template>
  <v-card flat class="my-2">
    <v-card-title class="px-2 py-3 ecdgray--text subtitle-3 text-uppercase">
      <v-row no-gutters>
        <v-col cols="6">{{$t("TopItems")}}</v-col>
        <v-col cols="6" class="text-none text-end body-2">{{$t("Today")}}</v-col>
      </v-row>
    </v-card-title>
    <v-divider></v-divider>
    <v-card-text class="px-0">
      <ec-list class="px-2" v-if="items && items.length > 0" :items="items" dense dashed>
        <template v-slot:prepend="{index}">
          {{index + 1}}.
        </template>
        <template v-slot:left="{ item }">
          <v-col cols="12" class="text-align-initial text-oneline">
            <span class="body-1">{{item.itemName}}</span>
          </v-col>
        </template>
        <template v-slot:right="{ item }">
          <v-col cols="12" class="text-end font-weight-bold subtitle-2">
            {{item.price | currency(item.$currency)}}
          </v-col>
        </template>
      </ec-list>
    </v-card-text>
  </v-card>
</template>

<script>
export default {
  components: {
    EcList: () => import("../ec/EcList"),
  },
  data() {
    return {
      items: null
    }
  },
  async mounted(){
    this.items = (await this.$api.items.getItems({take: 5})).data;
  }
};
</script>

<style lang="sass" scoped>

</style>