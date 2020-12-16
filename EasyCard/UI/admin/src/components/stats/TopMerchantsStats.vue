<template>
  <v-card flat class="my-2">
    <v-card-title class="px-2 py-3 ecdgray--text subtitle-3 text-uppercase">
      <v-row no-gutters>
        <v-col cols="6">{{$t("TopMerchants")}}</v-col>
        <v-col cols="6" class="text-none text-end body-2">{{$t("Today")}}</v-col>
      </v-row>
    </v-card-title>
    <v-divider></v-divider>
    <v-card-text class="px-0">
      <ec-list class="px-2" v-if="merchants && merchants.length > 0" :items="merchants" dense dashed>
        <template v-slot:prepend="{index}">
          {{index + 1}}.
        </template>
        <template v-slot:left="{ merchant }">
          <v-col cols="12" class="text-align-initial text-oneline">
            <span class="body-1">{{merchant.merchantName}}</span>
          </v-col>
        </template>
        <template v-slot:right="{ merchant }">
          <v-col cols="12" class="text-end font-weight-bold subtitle-2">
            {{merchant.price | currency(merchant.$currency)}}
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
      merchants: null
    }
  },
  async mounted(){
    
  }
};
</script>