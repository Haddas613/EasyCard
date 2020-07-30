<template>
  <v-flex fill-height>
    <v-btn
      color="primary"
      class="mb-1 text-none"
      height="48px"
      @click="ok()"
      block
      :disabled="model.amount == 0"
    >{{btntext}} <ec-money :amount="model.amount" class="px-1"></ec-money>
    </v-btn>
    <v-flex class="white text-center align-stretch px-3">
      <v-row>
        <v-col cols="4" class="py-1">
          <span class="subtitle-1 ecgray--text" style="line-height:2.5rem;">{{$t('AddNote')}}</span>
        </v-col>
        <v-col cols="8" class="pt-3">
          <input inputmode="decimal" 
            min="0.01" 
            v-model.lazy="model.amount" 
            style="text-align:right;"
            v-money="{precision: 2}"
          >
        </v-col>
      </v-row>
      <v-row>
        <v-col
          v-for="n in [1,2,3,4,5,6,7,8,9]"
          v-bind:key="n"
          cols="4"
          class="numpad-btn numpad-num"
          @click="model.amount += n"
        >{{n}}</v-col>
      </v-row>
      <v-row>
        <v-col cols="4" class="numpad-btn numpad-num" @click="model.amount = 0">C</v-col>
        <v-col cols="4" class="numpad-btn numpad-num" @click="model.amount += 0">0</v-col>
        <v-col cols="4" class="numpad-btn numpad-num accent--text">+</v-col>
      </v-row>
      <v-row>
        <v-col cols="6" class="numpad-btn primary--text py-5">{{$t('Calculator')}}</v-col>
        <v-col cols="6" class="numpad-btn py-5">{{$t('ItemsList')}}</v-col>
      </v-row>
    </v-flex>
  </v-flex>
</template>

<script>
import EcMoney from "../misc/EcMoney";

export default {
  components: {
    EcMoney,
  },
  data() {
    return {
      model: {
        amount: 0,
        note: null,
        items: []
      }
    };
  },
  props: {
    btntext: {
      type: String,
      default: null
    }
  },
  methods: {
    ok() {
      this.model.amount = parseFloat(this.model.amount)
      this.$emit("ok", this.model);
    }
  }
};
</script>

<style lang="scss" scoped>
.numpad-btn {
  border: 1px solid var(--v-ecgray-lighten2);
}
.numpad-num {
  font-size: 2rem;
  font-weight: 300;
}
</style>