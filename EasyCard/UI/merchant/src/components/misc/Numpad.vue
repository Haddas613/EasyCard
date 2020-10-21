<template>
  <v-flex fill-height>
    <item-pricing-dialog 
      v-if="selectedItem"
      :key="selectedItem.$itemID"
      :item="selectedItem" 
      :show.sync="itemPriceDialog" 
      v-on:ok="saveItem($event)"></item-pricing-dialog>
    <v-btn
      :color="totalAmount > model.discount ? 'primary' : 'error darken-2'"
      class="text-none charge-btn v-btn--flat"
      height="48px"
      @click="ok()"
      block
      :disabled="totalAmount == 0 && model.discount == 0"
      :fixed="$vuetify.breakpoint.smAndDown"
    >
      {{$t(btnText)}}
      <ec-money :amount="totalAmount - model.discount" class="px-1"></ec-money>
    </v-btn>
    <v-spacer style="height: 48px" v-if="$vuetify.breakpoint.smAndDown"></v-spacer>
    <v-flex class="white text-center align-stretch px-3">
      <template v-if="activeArea === 'calc'">
        <v-row dir="ltr">
          <v-col cols="6">
            <span @click="setActiveInput('amount')">
              <v-text-field
                class="py-0 px-0"
                :value="model.amount"
                readonly
                outlined
                :label="$t('Amount')"
                hide-details="true"
                :disabled="activeInput != 'amount'"
              ></v-text-field>
            </span>
            <!-- <span>{{model.amount}}</span> -->
          </v-col>
          <v-col cols="6">
            <span @click="setActiveInput('discount')">
              <v-text-field
                class="py-0 px-0"
                :value="model.discount"
                readonly
                outlined
                :label="$t('Discount')"
                hide-details="true"
                :disabled="activeInput != 'discount'"
                :error="totalAmount < model.discount"
              ></v-text-field>
            </span>
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
          <v-col
            cols="2"
            class="numpad-btn numpad-num accent--text"
            @click="stash()"
            v-if="activeInput == 'amount'"
          >+</v-col>
          <v-col
            cols="2"
            class="numpad-btn numpad-num accent--text"
            @click="calculatePercentage()"
            v-if="activeInput == 'discount'"
          >%</v-col>
        </v-row>
      </template>
      <v-footer :fixed="$vuetify.breakpoint.smAndDown" :padless="true" color="white">
        <v-row dir="ltr">
          <v-col
            cols="6"
            class="numpad-btn py-5"
            @click="activeArea = 'calc'"
            v-bind:class="{'primary--text': (activeArea == 'calc')}"
          >{{$t('Calculator')}}</v-col>
          <v-col
            cols="6"
            class="numpad-btn py-5"
            @click="activeArea = 'items'"
            v-bind:class="{'primary--text': (activeArea == 'items')}"
          >
            <span v-if="model.items.length === 0">{{$t('ItemsList')}}</span>
            <span
              v-if="model.items.length > 0"
            >{{$t('@ItemsSelected').replace('@amount', totalItemsAmount)}}</span>
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
        <v-flex class="d-flex justify-end">
          <v-switch
            class="pb-1 mt-0 px-4"
            v-model="showOnlySelectedItems"
            hide-details="true"
          >
            <template v-slot:label>
              <small>{{$t('Selected')}}</small>
            </template>
          </v-switch>
        </v-flex>
        <ec-list class="pb-1" :items="showOnlySelectedItems ? model.items : items" dense>
          <template v-slot:left="{ item }">
            <v-col cols="12" class="text-align-initial">
              <span class="body-2">{{item.itemName}}</span>
            </v-col>
          </template>

          <template v-slot:right="{ item }">
            <v-col cols="12" md="6" lg="6" class="text-end caption">
              <span>{{item.amount || ''}}</span>
            </v-col>
            <v-col cols="12" md="6" lg="6" class="text-end font-weight-bold subtitle-2">
              <ec-money :amount="item.price * (item.amount || 1) - (item.discount ? item.discount : 0)" :currency="item.$currency"></ec-money>
            </v-col>
          </template>

          <template v-slot:append="{ item }">
            <v-btn v-on:click="itemSelected(item)" icon v-if="!item.amount">
              <v-icon>mdi-plus</v-icon>
            </v-btn>
            <v-btn icon v-if="item.amount" @click="editItemCnt(item)">
              <v-icon>mdi-pencil</v-icon>
            </v-btn>
          </template>
        </ec-list>
        <!-- <v-spacer class="py-8" v-if="$vuetify.breakpoint.smAndDown"></v-spacer> -->
      </div>
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
      total: 0,
      items: [],
      model: {
        amount: "0",
        discount: "0",
        note: null,
        items: []
      },
      activeInput: "amount",
      activeArea: "calc",
      selectedItem: null,
      search: null,
      searchTimeout: null,
      itemPriceDialog: false,
      showOnlySelectedItems: false
    };
  },
  props: {
    btnText: {
      type: String,
      default: "OK"
    }
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
      return this.total + parseFloat(this.model.amount);
    },
    totalItemsAmount() {
      return this.lodash.sumBy(this.model.items, i => i.amount);
    },
    ...mapState({
      currencyStore: state => state.settings.currency
    })
  },
  async mounted() {
    await this.getItems();
  },
  methods: {
    addDigit(d) {
      if (this.model[this.activeInput] == "0") this.model[this.activeInput] = d;
      else if (this.model[this.activeInput] < 100000) {
        if (
          `${this.model[this.activeInput]}`.indexOf(".") > -1 &&
          `${this.model[this.activeInput]}`.split(".")[1].length == 2
        ) {
          return;
        }
        //TODO: config for max allowed transaction amount
        this.model[this.activeInput] += "" + d;
      }
    },
    addDot() {
      if (!this.model[this.activeInput]) {
        this.model[this.activeInput] = "0.";
      } else if (
        this.model[this.activeInput] &&
        `${this.model[this.activeInput]}`.indexOf(".") === -1
      ) {
        this.model[this.activeInput] += ".";
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
          if (this.model.items.length === 0) {
            itm.amount = 0;
            itm.discount = 0;
          } else {
            let entry = this.lodash.find(
              this.model.items,
              i => i.itemID === itm.$itemID
            );
            itm.amount = entry ? entry.amount : 0;
            itm.discount = entry ? entry.discount : 0;
          }
        }
        this.items = data.data;
      } else {
        this.items = [];
      }
    },
    ok() {
      if (this.totalAmount <= this.model.discount) {
        this.activeInput = "discount";
        return;
      }
      this.$emit("ok", {
        ...this.model,
        amount: parseFloat(this.totalAmount) - parseFloat(this.discount)
      });
    },
    stash() {
      if (this.model.activeInput == "discount") return;
      this.total += parseFloat(this.model.amount);
      this.model.amount = "0";
    },
    reset() {
      if (
        parseFloat(this.model[this.activeInput]) > 0 ||
        (typeof this.model[this.activeInput] == "string" &&
          this.model[this.activeInput].indexOf(".") > -1)
      ) {
        this.model[this.activeInput] = 0;
      } else {
        this.total = 0;
        this.model.discount = 0;
        //todo: should also clear items?
        this.resetItems();
      }
    },
    async resetItems() {
      this.model.items = [];
      await this.getItems();
    },
    itemSelected(item) {
      let entry = this.lodash.find(
        this.model.items,
        i => i.itemID === item.$itemID
      );
      if (entry) {
        entry.amount++;
      } else {
        this.model.items.push({
          itemID: item.$itemID,
          $itemID: item.$itemID,
          itemName: item.itemName,
          price: item.price,
          discount: 0,
          currency: item.$currency,
          amount: 1
        });
      }
      item.amount++;
      this.total += item.price - item.discount;
    },
    editItemCnt(item) {
      let entry = this.lodash.find(
        this.model.items,
        i => i.itemID === item.$itemID
      );
      if (entry) {
        this.selectedItem = this.lodash.cloneDeep(entry);
        this.itemPriceDialog = true;
      }
    },
    async saveItem(item) {
      let entry = this.lodash.find(
        this.model.items,
        i => i.itemID === item.$itemID
      );

      let recalc = 0;
      if(item.amount){
        recalc = (item.price * item.amount) - item.discount;
      }
      if (entry){
        recalc -= ((entry.price * entry.amount) - entry.discount ); 
      }

      if (item.amount) {
        if (entry) {
          entry.amount = item.amount;
          entry.discount = item.discount;
        }
      } else {
        let idx = this.model.items.findIndex(i => i.itemID === item.$itemID);
        if(idx > -1){
          this.model.items.splice(idx, 1);
        }
      }

      this.total += recalc;
      this.itemPriceDialog = false;
      await this.getItems();
    },
    setActiveInput(type) {
      this.activeInput = type;
    },
    calculatePercentage() {
      if (this.totalAmount == 0) {
        return (this.activeInput = "amount");
      }
      if (this.model.discount >= 100) {
        return this.$toasted.show(
          this.$t("PercentageShouldBeLessThanOneHundred"),
          { type: "error" }
        );
      }
      this.model.discount = (
        (this.totalAmount / 100) *
        this.model.discount
      ).toFixed(2);
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
}
button.charge-btn {
  z-index: 2;
  &[disabled] {
    background-color: var(--v-ecdgray-lighten5) !important;
  }
}
</style>