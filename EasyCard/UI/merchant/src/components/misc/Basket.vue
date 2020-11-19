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
      :color="model.totalAmount > 0 ? 'primary' : 'error darken-2'"
      class="text-none complete-btn v-btn--flat"
      height="48px"
      @click="ok()"
      block
      :disabled="model.totalAmount == 0"
      :fixed="$vuetify.breakpoint.smAndDown"
    >
      {{$t(btnText)}}
      <ec-money :amount="model.totalAmount" class="px-1"></ec-money>
    </v-btn>
    <v-spacer style="height: 48px" v-if="$vuetify.breakpoint.smAndDown"></v-spacer>
    <v-flex class="white text-center align-stretch px-3">
      <v-list :two-line="false" :dense="true" subheader class="py-0 fill-height body-2">
        <v-list-item>
          <v-list-item-content class="text-normal">
            <v-row no-gutters>
              <v-col cols="6" class="text-start">{{$t("Discount")}}</v-col>
              <v-col cols="6" class="text-end">{{totalDiscount | currency(currencyStore.code)}}</v-col>
            </v-row>
          </v-list-item-content>
        </v-list-item>
        <v-list-item>
          <v-list-item-content class="text-normal">
            <v-row no-gutters>
              <v-col cols="6" class="text-start">{{$t("VAT")}}</v-col>
              <v-col cols="3" class="text-end">17%</v-col>
              <v-col cols="3" class="text-end">{{model.vatTotal | currency(currencyStore.code)}}</v-col>
            </v-row>
          </v-list-item-content>
        </v-list-item>
      </v-list>
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
            <ec-money v-if="item.discount" :amount="-item.discount" :currency="item.currency"></ec-money>
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
import itemPricingService from "../../helpers/item-pricing";

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
        totalAmount: 0,
        netTotal: 0,
        vatTotal: 0,
        items: [...this.items]
      },
      selectedItem: null,
      itemPriceDialog: false,
      search: null,
      vatRate: 0.17
    };
  },
  props: {
    btnText: {
      type: String,
      default: "OK"
    },
    items: {
      type: Array,
      required: true
    }
  },
  mounted () {
    itemPricingService.total.calculate(this.model, { vatRate: this.vatRate});
  },
  computed: {
    totalDiscount() {
      return this.lodash.sumBy(this.model.items, "discount");
    },
    ...mapState({
      currencyStore: state => state.settings.currency
    })
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
        this.selectedItem = { idx: idx, ...entry };
        this.itemPriceDialog = true;
      }
    },
    async saveItem(item) {
      let entry = this.model.items[item.idx];
      if (entry) {
        this.$set(this.model.items, item.idx, item);
      }
      itemPricingService.total.calculate(this.model, {vatRate: this.vatRate});
      this.itemPriceDialog = false;
    },
    deleteItem(idx) {
      if (this.model.items[idx]) this.model.items.splice(idx, 1);
      itemPricingService.total.calculate(this.model, {vatRate: this.vatRate});
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