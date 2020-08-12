<template>
  <v-flex fill-height>
    <v-btn
      color="primary"
      class="mb-1 text-none charge-btn v-btn--flat"
      height="48px"
      @click="ok()"
      block
      :disabled="totalAmount == 0"
      :fixed="$vuetify.breakpoint.smAndDown"
    >
      {{btntext}}
      <ec-money :amount="totalAmount" class="px-1"></ec-money>
    </v-btn>
    <v-spacer style="height: 48px" v-if="$vuetify.breakpoint.smAndDown"></v-spacer>
    <v-flex class="white text-center align-stretch px-3">
      <template v-if="activeArea === 'calc'">
        <v-row dir="ltr">
          <v-col cols="4" class="py-1">
            <span class="subtitle-1 ecgray--text" style="line-height:2.5rem;">{{$t('AddNote')}}</span>
          </v-col>
          <v-col cols="8" class="pt-3 text-right">
            <input
              inputmode="decimal"
              min="0.01"
              v-model.lazy="model.amount"
              v-money="{precision: 2}"
              class="text-right pr-4"
              disabled
            />
          </v-col>
        </v-row>
        <v-row dir="ltr">
          <v-col
            v-for="n in [1,2,3,4,5,6,7,8,9]"
            v-bind:key="n"
            cols="4"
            class="numpad-btn numpad-num"
            @click="model.amount += n"
          >{{n}}</v-col>
        </v-row>
        <v-row dir="ltr">
          <v-col cols="4" class="numpad-btn numpad-num" @click="reset()">C</v-col>
          <v-col cols="4" class="numpad-btn numpad-num" @click="model.amount += 0">0</v-col>
          <v-col cols="4" class="numpad-btn numpad-num accent--text" @click="stash()">+</v-col>
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
        <ec-list class="pb-1" :items="items" v-on:click="itemSelected($event)" dense clickable>
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
            <v-btn icon v-if="!selectedItemsCnt[item.$itemID]">
              <v-icon>mdi-plus</v-icon>
            </v-btn>
            <v-btn icon v-if="selectedItemsCnt[item.$itemID]">
              <v-icon>mdi-pencil</v-icon>
            </v-btn>
          </template>
        </ec-list>
      </div>
    </v-flex>
  </v-flex>
</template>

<script>
import EcMoney from "../ec/EcMoney";
import EcList from "../ec/EcList";
import ReIcon from "../misc/ResponsiveIcon";
import { mapState } from "vuex";

export default {
  components: {
    EcMoney,
    EcList,
    ReIcon
  },
  data() {
    return {
      total: 0,
      items: [],
      model: {
        amount: 0,
        note: null,
        items: []
      },
      activeArea: "calc",
      selectedItemsCnt: {},
      search: null,
      searchTimeout: null
    };
  },
  props: {
    btntext: {
      type: String,
      default: null
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
    }),
  },
  async mounted() {
    await this.getItems();
  },
  methods: {
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
      this.model.amount = parseFloat(this.model.amount);//this.totalAmount;
      this.$emit("ok", this.model);
    },
    stash() {
      this.total += parseFloat(this.model.amount);
      this.model.amount = 0;
    },
    reset() {
      if (parseFloat(this.model.amount) > 0) {
        this.model.amount = 0;
      } else {
        this.total = 0;
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
          currency: item.$currency,
          amount: 1
        });
      }
      this.total += item.price;
      this.$set(this.selectedItemsCnt, item.$itemID, entry ? entry.amount : 1);
    },
    decreaseAmount(item) {
      let entry = this.lodash.find(
        this.model.items,
        i => i.itemID === item.$itemID
      );
      if (entry) {
        entry.amount--;
      }
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