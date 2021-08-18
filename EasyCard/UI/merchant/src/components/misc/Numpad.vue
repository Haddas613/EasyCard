<template>
  <v-flex fill-height>
    <v-row no-gutters>
      <v-col :cols="supportQuickCharge ? 6 : 12">
        <v-btn
          :color="totalAmount > model.discount ? 'primary' : 'error darken-2'"
          class="text-none complete-btn v-btn--flat"
          height="48px"
          @click="ok()"
          block
          tile
          :disabled="totalAmount == 0 && model.discount == 0"
        >
          {{$t(btnText)}}
          <ec-money :amount="totalAmount - model.discount" class="px-1" :currency="currencyStore.code"></ec-money>
        </v-btn>
      </v-col>
      <v-col v-if="supportQuickCharge" cols="6">
        <v-btn
          :color="totalAmount > model.discount ? 'secondary' : 'error darken-2'"
          class="complete-btn text-none v-btn--flat"
          height="48px"
          @click="ok(true)"
          block
          tile
          :dark="totalAmount > 0"
          :disabled="totalAmount == 0 && model.discount == 0"
        >
          {{$t(`Quick${btnText}`)}}
        </v-btn>
      </v-col>
    </v-row>
    <!-- <v-spacer style="height: 48px" v-if="$vuetify.breakpoint.smAndDown"></v-spacer> -->
    <v-flex class="white text-center align-stretch px-3">
      <template v-if="activeArea === 'calc'">
        <v-row dir="ltr" class="text-end">
          <v-col cols="12">
            <span>{{defaultItem.price}}</span>
          </v-col>
        </v-row>
        <v-row dir="ltr">
          <v-col
            v-for="n in [1,2,3,4,5,6,7,8,9]"
            v-bind:key="n"
            cols="4"
            class="numpad-btn numpad-num"
            @click="addDigit(n)"
          >{{n}}</v-col>
        </v-row>
        <v-row dir="ltr">
          <v-col cols="4" class="numpad-btn numpad-num" @click="reset()">C</v-col>
          <v-col cols="4" class="numpad-btn numpad-num" @click="addDigit(0)">0</v-col>
          <v-col cols="2" class="numpad-btn numpad-num secondary--text" @click="addDot()">.</v-col>
          <v-col cols="2" class="numpad-btn numpad-num accent--text" @click="stash()">+</v-col>
        </v-row>
      </template>
      <v-footer :fixed="$vuetify.breakpoint.smAndDown" :padless="true" color="white">
        <v-row dir="ltr">
          <v-col cols="6" class="numpad-btn py-5" @click="activeArea = 'calc'">
            <v-icon v-bind:class="{'primary--text': (activeArea == 'calc')}">mdi-calculator-variant</v-icon>
          </v-col>
          <v-col cols="6" class="numpad-btn py-5" @click="activeArea = 'items'">
            <v-icon
              v-bind:class="{'primary--text': (activeArea == 'items')}"
            >mdi-format-list-bulleted-square</v-icon>
          </v-col>
        </v-row>
      </v-footer>
      <div v-if="activeArea === 'items'">
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
        <ec-list class="pb-1" :items="apiItems" dense>
          <template v-slot:left="{ item }">
            <v-col cols="12" class="text-align-initial">
              <span class="body-2">{{item.itemName}}</span>
            </v-col>
          </template>

          <template v-slot:right="{ item }">
            <v-col cols="12" md="6" lg="6" class="text-end caption">
              <span>{{item.quantity || ''}}</span>
            </v-col>
            <v-col cols="12" md="6" lg="6" class="text-end font-weight-bold subtitle-2">
              <ec-money
                :amount="item.price * (item.quantity || 1) - (item.discount ? item.discount : 0)"
                :currency="item.$currency"
              ></ec-money>
            </v-col>
          </template>

          <template v-slot:append="{ item }">
            <v-btn v-on:click="itemSelected(item)" icon v-if="!item.quantity">
              <v-icon>mdi-plus</v-icon>
            </v-btn>
            <v-btn icon v-if="item.quantity" @click="editItemCnt(item)">
              <v-icon>mdi-pencil</v-icon>
            </v-btn>
          </template>
        </ec-list>
      </div>
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
  props: {
    btnText: {
      type: String,
      default: "OK"
    },
    data: {
      type: Object,
      required: true,
    },
    supportQuickCharge: {
      type: Boolean,
      default: false
    }
  },
  data() {
    return {
      total: 0,
      apiItems: [],
      model: {
        amount: "0",
        discount: "0",
        vatRate: null,
        ...this.data
      },
      defaultItem: {
        price: 0,
        discount: 0,
        amount: 0,
        itemName: null,
        currency: null,
        quantity: 1,
        externalReference: null
      },
      activeArea: "calc",
      search: null,
      searchTimeout: null,
    };
  },
  watch: {
    async search(newValue, oldValue) {
      if (this.searchTimeout) clearTimeout(this.searchTimeout);

      let searchWasAppliable = oldValue && oldValue.trim().length >= 3;
      let searchApply = newValue && newValue.trim().length >= 3;
      if (!searchWasAppliable && !searchApply) {
        return;
      }

      this.searchTimeout = setTimeout(
        (async () => {
          await this.getItems();
        }).bind(this),
        1000
      );
    }
  },
  computed: {
    totalAmount() {
      return (
        parseFloat(this.defaultItem.price) +
        this.lodash.sumBy(this.model.dealDetails.items, "amount")
      );
    },
    ...mapState({
      currencyStore: state => state.settings.currency,
      terminalStore: state => state.settings.terminal
    })
  },
  async mounted() {
    this.defaultItem.currency = this.currencyStore.code;
    this.defaultItem.itemName = this.terminalStore.settings.defaultItemName || 'Custom Charge';
    if(this.model.vatRate === null){
      this.model.vatRate = this.terminalStore.settings.vatRate
    }
    await this.getItems();
    this.$emit('update', this.model);

    window.addEventListener("keypress", this.handleKeyPress);
  },
  destroyed () {
    window.removeEventListener("keypress", this.handleKeyPress);
  },
  methods: {
    handleKeyPress($event){
      if(!isNaN($event.key)){
        this.addDigit($event.key);
        return;
      }
      switch($event.key){
        case ".":
        case ",":
          this.addDot();
          break;
        case "+":
          this.stash();
          break;
      }
    },
    addDigit(d) {
      if (this.defaultItem.price == "0") this.defaultItem.price = d;
      else if (this.defaultItem.price < 100000) {
        if (
          `${this.defaultItem.price}`.indexOf(".") > -1 &&
          `${this.defaultItem.price}`.split(".")[1].length == 2
        ) {
          return;
        }
        //TODO: config for max allowed transaction amount
        this.defaultItem.price += "" + d;
      }
    },
    addDot() {
      if (!this.defaultItem.price) {
        this.defaultItem.price = "0.";
      } else if (
        this.defaultItem.price &&
        `${this.defaultItem.price}`.indexOf(".") === -1
      ) {
        this.defaultItem.price += ".";
      }
    },
    async getItems() {
      let searchApply = this.search && this.search.trim().length >= 3;

      let data = await this.$api.items.getItems({
        search: searchApply ? this.search : "",
        currency: this.currencyStore.code
      });
      if (data && data.data) {
        for (let itm of data.data) {
          itm.amount = itm.price;
          itm.discount = 0;
        }
        this.apiItems = data.data;
      } else {
        this.apiItems = [];
      }
    },
    ok(quickCharge) {
      this.stash();
      this.$emit("ok", {
        ...this.getData(),
        quickCharge
      });
    },
    getData(){
      this.stash();
      itemPricingService.total.calculate(this.model, { vatRate: this.model.vatRate});
      return this.model
    },
    stash() {
      if (!this.defaultItem.price || this.defaultItem.price == "0") {
        return;
      }
      this.model.dealDetails.items.push(this.prepareItem());
      this.defaultItem.price = "0";
      this.defaultItem.amount = this.defaultItem.discount = "0";
    },
    prepareItem() {
      let item = {
        ...this.defaultItem,
        price: parseFloat(this.defaultItem.price),
        discount: parseFloat(this.defaultItem.discount)
      };
      this.calculatePricingForItem(item);
      return item;
    },
    calculatePricingForItem(item){
      itemPricingService.item.calculate(item, { vatRate: this.model.vatRate});
    },
    reset() {
      this.defaultItem.price = 0;
      this.defaultItem.discount = 0;
    },
    async resetItems() {
      this.model.dealDetails.items = [];
      await this.getItems();
    },
    itemSelected(item) {
      let newItem = {
        itemID: item.$itemID,
        itemName: item.itemName,
        price: item.price,
        discount: 0,
        currency: item.$currency,
        quantity: 1,
        externalReference: item.externalReference
      };

      this.calculatePricingForItem(newItem);
      this.model.dealDetails.items.push(newItem);
    }
  }
};
</script>

<style lang="scss" scoped>
.numpad-btn {
  border: 1px solid var(--v-ecgray-lighten2);
  cursor: pointer;
}
.numpad-num {
  font-size: 2rem;
  font-weight: 300;
  min-height: 4rem;
  line-height: 2.5rem;
}
button.complete-btn {
  z-index: 2;
  &[disabled] {
    background-color: var(--v-ecdgray-lighten5) !important;
  }
}
</style>