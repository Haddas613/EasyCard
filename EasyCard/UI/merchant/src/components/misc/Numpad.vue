<template>
  <v-flex fill-height>
    <ec-dialog :dialog.sync="itemCntDialog">
      <template v-slot:title>{{$t('EditItemCount')}}</template>
      <template>
        <div class="d-flex px-2 pt-4 justify-end">
          <v-btn
            color="red"
            class="white--text"
            :block="$vuetify.breakpoint.smAndDown"
            @click="selectedItem.amount = 0; itemCntChanged()"
          >
            <v-icon left>mdi-delete</v-icon>
            {{$t("Delete")}}
          </v-btn>
        </div>
        <v-row no-gutters>
          <v-col cols="12">
            <v-select
              class="mx-2 mt-4"
              outlined
              v-if="selectedItem"
              :items="lodash.range(101)"
              v-model="selectedItem.amount"
              @change="itemCntChanged()"
              :label="$t('Count')"
            ></v-select>
          </v-col>
          <v-col cols="12" md="4">
            <v-text-field
              class="mx-2 mt-4"
              v-if="selectedItem"
              :value="selectedItem.price"
              outlined
              readonly
              :label="$t('InitialPrice')"
              hide-details="true"
            ></v-text-field>
          </v-col>
          <v-col cols="12" md="4">
            <v-text-field
              class="mx-2 mt-4"
              v-if="selectedItem"
              v-model.number="selectedItem.discount"
              outlined
              :label="$t('Discount')"
              hide-details="true"
            >
              <template v-slot:append >
                <v-btn
                  color="primary"
                  icon
                  style="margin-top:-0.5rem"
                  @click="calculateItemPercentage(selectedItem)"
                  :title="$t('ApplyAsPercentage')"
                >
                  <v-icon>mdi-percent</v-icon>
                </v-btn>
              </template>
            </v-text-field>
          </v-col>
          <v-col cols="12" md="4">
            <v-text-field
              class="mx-2 mt-4"
              v-if="selectedItem"
              :value="(selectedItem.price - selectedItem.discount).toFixed(2)"
              outlined
              readonly
              :label="$t('DiscountedPrice')"
              hide-details="true"
            ></v-text-field>
          </v-col>
        </v-row>
      </template>
    </ec-dialog>
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
        <ec-list class="pb-1" :items="items" dense>
          <template v-slot:left="{ item }">
            <v-col cols="12" class="text-align-initial">
              <span class="body-2">{{item.itemName}}</span>
            </v-col>
          </template>

          <template v-slot:right="{ item }">
            <v-col cols="12" md="6" lg="6" class="text-end caption">
              <span>{{selectedItemsCnt[item.$itemID]}}</span>
            </v-col>
            <v-col cols="12" md="6" lg="6" class="text-end font-weight-bold subtitle-2">
              <ec-money :amount="item.price" :currency="item.$currency"></ec-money>
            </v-col>
          </template>

          <template v-slot:append="{ item }">
            <v-btn v-on:click="itemSelected(item)" icon v-if="!selectedItemsCnt[item.$itemID]">
              <v-icon>mdi-plus</v-icon>
            </v-btn>
            <v-btn icon v-if="selectedItemsCnt[item.$itemID]" @click="editItemCnt(item)">
              <v-icon>mdi-pencil</v-icon>
            </v-btn>
          </template>
        </ec-list>
        <v-spacer class="py-8" v-if="$vuetify.breakpoint.smAndDown"></v-spacer>
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
    EcDialog: () => import("../../components/ec/EcDialog")
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
      //TODO: Remove, fuze selected data with api data instead
      selectedItemsCnt: {},
      selectedItem: null,
      search: null,
      searchTimeout: null,
      itemCntDialog: false
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
      // if (
      //   this.activeInput == "discount" &&
      //   this.model.amount < this.model.discount + "" + d
      // ) {
      //   return;
      // }
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
      if (data) {
        this.items = data.data || [];
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
    resetItems() {
      this.model.items = [];
      this.$set(this, "selectedItemsCnt", {});
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
          itemName: item.itemName,
          price: item.price,
          discount: 0,
          currency: item.$currency,
          amount: 1
        });
      }
      this.total += item.price;
      this.$set(this.selectedItemsCnt, item.$itemID, entry ? entry.amount : 1);
    },
    editItemCnt(item) {
      let entry = this.lodash.find(
        this.model.items,
        i => i.itemID === item.$itemID
      );
      if (entry) {
        this.selectedItem = entry;
        this.itemCntDialog = true;
      }
    },
    itemCntChanged() {
      let prevCount = this.selectedItemsCnt[this.selectedItem.itemID];
      let recalc =
        -(this.selectedItem.price * prevCount) +
        this.selectedItem.amount * this.selectedItem.price;

      if (this.selectedItem.amount) {
        this.$set(
          this.selectedItemsCnt,
          this.selectedItem.itemID,
          this.selectedItem.amount
        );
      } else {
        this.lodash.remove(
          this.model.items,
          i => i.itemID === this.selectedItem.itemID
        );
        this.$set(this.selectedItemsCnt, this.selectedItem.itemID, null);
      }

      this.total += recalc;
      this.itemCntDialog = false;
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
    },
    calculateItemPercentage(item) {
      if (!item.price) return;
      if (item.discount >= 100) {
        return this.$toasted.show(
          this.$t("PercentageShouldBeLessThanOneHundred"),
          { type: "error" }
        );
      }
      item.discount = ((item.price / 100) * item.discount).toFixed(2);
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