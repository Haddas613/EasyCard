<template>
  <v-flex fill-height>
    <item-pricing-dialog
      v-if="selectedItem"
      :key="selectedItem.itemID"
      :item="selectedItem"
      :show.sync="itemPriceDialog"
      v-on:ok="saveItem($event)"
    ></item-pricing-dialog>
    <v-btn
      :color="totalAmount > 0 ? 'primary' : 'error darken-2'"
      class="text-none complete-btn v-btn--flat"
      height="48px"
      @click="ok()"
      block
      :disabled="totalAmount == 0"
      :fixed="$vuetify.breakpoint.smAndDown"
    >
      {{$t(btnText)}}
      <ec-money :amount="totalAmount" class="px-1"></ec-money>
    </v-btn>
    <v-spacer style="height: 48px" v-if="$vuetify.breakpoint.smAndDown"></v-spacer>
    <v-flex class="white text-center align-stretch px-3">
      <v-text-field
        class="py-0 px-5 input-simple"
        single-line
        :hide-details="true"
        solo
        :label="$t('Search')"
        prepend-icon="mdi-magnify"
        v-model="search"
        clearable
      ></v-text-field>
      <v-divider></v-divider>
      <ec-list v-if="model.items" class="pb-1" :items="model.items" dense>
        <template v-slot:prepend="{ item, index }">
          <v-btn icon @click="deleteItem(index)">
            <v-icon>mdi-delete</v-icon>
          </v-btn>
        </template>

        <template v-slot:left="{ item }">
          <v-col cols="12" class="text-align-initial">
            <span class="body-2">{{item.itemName}}</span>
          </v-col>
        </template>

        <template v-slot:right="{ item }">
          <v-col cols="12" md="6" lg="6" class="text-end caption">
            <ec-money
              v-if="item.discount"
              :amount="-item.discount"
              :currency="item.$currency"
            ></ec-money>
          </v-col>
          <v-col cols="12" md="6" lg="6" class="text-end font-weight-bold subtitle-2">
            <ec-money
              :amount="item.price - (item.discount ? item.discount : 0)"
              :currency="item.$currency"
            ></ec-money>
          </v-col>
        </template>

        <template v-slot:append="{ index }">
          <v-btn icon @click="editItem(index)">
            <v-icon>mdi-pencil</v-icon>
          </v-btn>
        </template>
      </ec-list>
    </v-flex>
  </v-flex>
</template>

<script>
import { mapState } from "vuex";

export default {
  components: {
    EcMoney: () => import("../ec/EcMoney"),
    EcList: () => import("../ec/EcList"),
    ReIcon: () => import("../misc/ResponsiveIcon"),
    itemPricingDialog: () => import("../../components/items/ItemPricingDialog")
  },
  data() {
    return {
      model: {
        ...this.data
      },
      defaultItem: {
        price: 0,
        discount: 0,
        amount: 0,
        itemName: "Custom charge",
        currency: null,
        quantity: 1
      },
      selectedItem: null,
      itemPriceDialog: false,
      search: null
    };
  },
  props: {
    btnText: {
      type: String,
      default: "OK"
    },
    data: {
      type: Object,
      required: true 
    },
  },
  computed: {
    totalAmount() {
      return this.lodash.sumBy(this.model.items, "amount");
    },
    ...mapState({
      currencyStore: state => state.settings.currency
    })
  },
  async mounted() {
    this.defaultItem.currency = this.currencyStore.code;
  },
  methods: {
    ok() {
      this.$emit("ok", {
        ...this.model,
        amount: parseFloat(this.totalAmount) - parseFloat(this.discount || "0")
      });
    },
    calculateAmount(item) {
      return item.price - item.discount;
    },
    editItem(idx) {
      let entry = this.model.items[idx];

      if (entry) {
        this.selectedItem =  { idx: idx, ...entry };
        this.itemPriceDialog = true;
      }
    },
    async saveItem(item) {
      let entry = this.model.items[item.idx];
      if (entry) {
        this.$set(this.model.items, item.idx, item);
      }
      
      this.itemPriceDialog = false;
    },
    deleteItem(idx){
      if(this.model.items[idx])
        this.model.items.splice(idx, 1);
    }
  }
};
</script>

<style lang="scss" scoped>

button.complete-btn {
  z-index: 2;
  &[disabled] {
    background-color: var(--v-ecdgray-lighten5) !important;
  }
}
</style>