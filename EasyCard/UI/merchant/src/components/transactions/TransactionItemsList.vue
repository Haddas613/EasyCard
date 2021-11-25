<template>
  <v-flex fill-height>
    <ec-list v-if="items" class="pb-1" :items="items" dense>
      <template v-slot:left="{ item }">
        <v-col cols="12" class="text-align-initial">
          <span class="body-2">{{item.itemName}}</span>
        </v-col>
        <!-- <v-col cols="12" md="6" class="text-align-initial">
          <span class="body-2">{{item.quantity}}</span>
        </v-col> -->
      </template>

      <template v-slot:right="{ item }">
        <v-col cols="12" md="6" lg="6" class="text-end caption">
          <ec-money v-if="item.discount" :amount="-item.discount" :currency="currency"></ec-money>
        </v-col>
        <v-col cols="12" md="6" lg="6" class="text-end font-weight-bold subtitle-2">
          <ec-money
            :amount="item.price - (item.discount ? item.discount : 0)"
            :currency="currency"
          ></ec-money>
        </v-col>
      </template>
    </ec-list>
  </v-flex>
  <!-- <v-simple-table>
    <template v-slot:default>
      <thead>
        <tr>
          <th class="text-left">{{$t("Name")}}</th>
          <th class="text-left">{{$t("Discount")}}</th>
          <th class="text-left">{{$t("Amount")}}</th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="item in items" :key="item.itemID">
          <td>{{ item.itemName }}</td>
          <td><ec-money v-if="item.discount" :amount="-item.discount" :currency="item.$currency"></ec-money></td>
          <td>
              <ec-money :amount="item.price - (item.discount ? item.discount : 0)" :currency="item.$currency"></ec-money>
          </td>
        </tr>
      </tbody>
    </template>
  </v-simple-table> -->
</template>

<script>
// import { mapState } from "vuex";

export default {
  components: {
    EcMoney: () => import("../ec/EcMoney"),
    EcList: () => import("../ec/EcList")
  },
  props: {
    items: {
      type: Array,
      required: true
    },
    currency: {
      type: String,
      required: false,
      default: null
    }
  },
//   computed: {
//     ...mapState({
//       currencyStore: state => state.settings.currency,
//       terminalStore: state => state.settings.terminal
//     })
//   }
};
</script>