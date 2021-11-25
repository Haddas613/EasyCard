<template>
  <v-flex fill-height>
    <item-pricing-dialog
      v-if="selectedItem"
      :key="model.vatRate + '.' + selectedItem.itemID"
      :item="selectedItem"
      :basket="model"
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
      v-if="!embed"
    >
      {{$t(btnText)}}
      <ec-money :amount="totalAmount" class="px-1"></ec-money>
    </v-btn>
    <v-spacer style="height: 48px" v-if="!embed && $vuetify.breakpoint.smAndDown"></v-spacer>
    <v-flex class="text-center align-stretch px-3" v-bind:class="{'white': !embed}">
      <v-list :two-line="false" :dense="true" subheader class="py-0 fill-height body-2" :color="embed ? 'ecbg' : null"> 
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
            <v-row no-gutters align="center">
              <v-col cols="3" class="text-start">{{$t("VAT")}}</v-col>
              <v-col cols="3" class="text-initial">
                <v-switch class="pt-0 mt-0" v-model="vatExempt" :disabled="!model.vatRate" dense hide-details>
                  <template v-slot:label>
                    <small>{{$t('VATExempt')}}</small>
                  </template>
                </v-switch>
              </v-col>
              <v-col cols="3" class="text-end">{{(model.vatRate * 100).toFixed(0)}}%</v-col>
              <v-col cols="3" class="text-end">{{model.vatTotal | currency(currencyStore.code)}}</v-col>
            </v-row>
          </v-list-item-content>
        </v-list-item>
        <v-list-item v-if="embed">
          <v-list-item-content class="text-normal">
            <v-row no-gutters>
              <v-col cols="6" class="text-start">{{$t("Total")}}</v-col>
              <v-col cols="3" class="text-end">{{model.dealDetails.items.length}}</v-col>
              <v-col cols="3" class="text-end">{{totalAmount | currency(currencyStore.code)}}</v-col>
            </v-row>
          </v-list-item-content>
        </v-list-item>
      </v-list>
      <v-divider></v-divider>
      <ec-list v-if="model.dealDetails.items" class="pb-1" :items="model.dealDetails.items" dense :color="embed ? 'ecbg' : null">
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
              :amount="item.price"
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
    ItemPricingDialog: () => import("../../components/items/ItemPricingDialog")
  },
  data() {
    return {
      model: {
        totalAmount: 0,
        netTotal: 0,
        vatTotal: 0,
        vatRate: 0,
        ...this.data
      },
      selectedItem: null,
      itemPriceDialog: false,
      search: null,
      vatExempt: this.data.vatRate === 0,
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
    embed: {
      type: Boolean,
      default: false
    }
  },
  mounted () {
    if(this.model.vatRate === null){
      this.model.vatRate = this.terminalStore.settings.vatExempt ? null : this.terminalStore.settings.vatRate;
    }
    this.vatExempt = !this.model.vatRate;
    //itemPricingService.total.calculate(this.model, { vatRate: this.model.vatRate});
  },
  computed: {
    totalDiscount() {
      return this.lodash.sumBy(this.model.dealDetails.items, "discount");
    },
    totalAmount() {
      return this.lodash.sumBy(this.model.dealDetails.items, e => e.price - e.discount);
    },
    ...mapState({
      currencyStore: state => state.settings.currency,
      terminalStore: state => state.settings.terminal
    })
  },
  watch: {
    vatExempt(newValue, oldValue) {
      this.model.vatRate = newValue ?  0 : this.terminalStore.settings.vatRate;
      this.recalculate();
    }
  },
  methods: {
    ok() {
      itemPricingService.total.calculate(this.model, {vatRate: this.model.vatRate});
      this.$emit("ok", {
        silent: true,
        ...this.model
      });
    },
    editItem(idx) {
      let entry = this.model.dealDetails.items[idx];

      if (entry) {
        this.selectedItem = { idx: idx, ...entry };
        this.itemPriceDialog = true;
      }
    },
    async saveItem(item) {
      let entry = this.model.dealDetails.items[item.idx];
      if (entry) {
        this.$set(this.model.dealDetails.items, item.idx, item);
      }
      this.recalculate();
      this.itemPriceDialog = false;
    },
    deleteItem(idx) {
      if (this.model.dealDetails.items[idx]) this.model.dealDetails.items.splice(idx, 1);
      this.recalculate();
    },
    recalculate(){
       for(var item of this.model.dealDetails.items){
         itemPricingService.item.calculate(item, { vatRate: this.model.vatRate });
       }
       itemPricingService.total.calculate(this.model, {vatRate: this.model.vatRate});
       this.$emit('update', this.model);
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