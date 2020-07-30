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
    <v-spacer style="height: 48px"></v-spacer>
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
            @click="setActiveArea('calc')"
            v-bind:class="{'primary--text': (activeArea == 'calc')}"
          >{{$t('Calculator')}}</v-col>
          <v-col
            cols="6"
            class="numpad-btn py-5"
            @click="setActiveArea('items')"
            v-bind:class="{'primary--text': (activeArea == 'items')}"
          >{{$t('ItemsList')}}</v-col>
        </v-row>
      </v-footer>
      <ec-list :items="items" v-if="activeArea === 'items'">
        <template v-slot:left="{ item }">
          <v-col cols="12" md="6" lg="6" class="pt-1 caption ecgray--text">{{item.itemID}}</v-col>
          <v-col cols="12" md="6" lg="6">{{item.itemName}}</v-col>
        </template>

        <template v-slot:right="{ item }">
          <v-col cols="12" md="6" lg="6" class="text-end font-weight-bold button"></v-col>
          <v-col cols="12" md="6" lg="6" class="text-end font-weight-bold button">
            <ec-money :amount="item.price" :currency="item.$currency"></ec-money>
          </v-col>
        </template>

        <template v-slot:append="{ item }">
          <v-btn icon :to="{ name: 'EditItem', params: { id: item.$itemID } }">
            <v-icon>mdi-plus</v-icon>
          </v-btn>
        </template>
      </ec-list>
    </v-flex>
  </v-flex>
</template>

<script>
import EcMoney from "../ec/EcMoney";
import EcList from "../ec/EcList";
import ReIcon from "../misc/ResponsiveIcon";

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
      activeArea: "calc"
    };
  },
  props: {
    btntext: {
      type: String,
      default: null
    }
  },
  computed: {
    totalAmount() {
      return this.total + parseFloat(this.model.amount);
    }
  },
  async mounted() {
    let data = await this.$api.items.getItems();
    if (data) {
      this.items = data.data || [];
      // this.totalAmount = data.numberOfRecords || 0;
    }
  },
  methods: {
    ok() {
      this.model.amount = this.totalAmount;
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
      }
    },
    setActiveArea(areaType) {
      this.activeArea = areaType;
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