<template>
  <v-card flat class="my-2">
    <v-card-title class="px-2 py-3 ecdgray--text subtitle-3 text-uppercase">
      <v-row no-gutters>
        <v-col cols="6">{{$t("Information")}}</v-col>
        <v-col cols="6" class="text-none text-end body-2">{{$t("Today")}}</v-col>
      </v-row>
    </v-card-title>
    <v-divider></v-divider>
    <v-card-text>
      <ec-list class="" v-if="data && data.length > 0" :items="data" dense dashed>
        <template v-slot:left="{ item }">
          <v-col cols="12" class="text-align-initial text-oneline">
            <span class="body-1">{{item.name}}</span>
          </v-col>
        </template>
        <template v-slot:right="{ item }">
          <v-col cols="12" class="text-end font-weight-bold subtitle-2">{{item.value}}</v-col>
        </template>
      </ec-list>
    </v-card-text>
  </v-card>
</template>

<script>
export default {
  components: {
    EcList: () => import("../ec/EcList")
  },
  data() {
    return {
      data: [
        {
          name: "Average Spend",
          value: "$23"
        },
        {
          name: "Total Customer",
          value: "354"
        },
        {
          name: "New Customers",
          value: "237(63%)"
        },
        {
          name: "Repeating Customers",
          value: "127(37%)"
        }
      ]
    };
  }
};
</script>

<style lang="sass" scoped>

</style>