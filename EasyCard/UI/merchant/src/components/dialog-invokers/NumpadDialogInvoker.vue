<template>
  <div>
    <ec-dialog :dialog.sync="numpadDialog" color="ecbg">
      <template v-slot:title>{{$t('SelectItems')}}</template>
      <template>
        <v-flex fluid fill-height>
          <numpad
            btn-text="OK"
            v-on:ok="processAmount($event);"
            ref="numpadRef"
            :items="model.items"
          ></numpad>
        </v-flex>
      </template>
    </ec-dialog>
    <ec-dialog-invoker v-on:click="numpadDialog = true">
      <template v-slot:prepend>
        <v-icon>mdi-basket</v-icon>
      </template>
      <template v-slot:left>
        <div v-if="!model.items.length">{{$t("SelectItems")}}</div>
        <div v-else>
          <span
            class="primary--text"
          >{{$t("@ItemsSelected").replace("@amount", model.items.length)}}</span>
        </div>
      </template>
      <template v-slot:append>
        <re-icon>mdi-chevron-right</re-icon>
      </template>
    </ec-dialog-invoker>
  </div>
</template>

<script>
import { mapState } from "vuex";
import itemPricingService from "../../helpers/item-pricing";

export default {
  props: {
    data: {
      type: Array,
      default: () => []
    },
    terminal: {
      default: null
    }
  },
  components: {
    EcDialog: () => import("../../components/ec/EcDialog"),
    EcDialogInvoker: () => import("../../components/ec/EcDialogInvoker"),
    ReIcon: () => import("../../components/misc/ResponsiveIcon"),
    Numpad: () => import("../../components/misc/Numpad")
  },
  data() {
    return {
      model: {
        items: [...this.data],
        amount: 0,
        discount: 0,
        netTotal: 0,
        totalAmount: 0,
        vatRate: 0,
        vatTotal: 0
      },
      numpadDialog: false
    };
  },
  async mounted() {
    itemPricingService.total.calculate(this.model, {
      vatRate: this.terminalStore.settings.vatRate
    });
  },
  computed: {
    ...mapState({
      currencyStore: state => state.settings.currency,
      terminalStore: state => state.settings.terminal
    })
  },
  methods: {
    processAmount(data) {
      this.model = data;
      itemPricingService.total.calculate(this.model, {
        vatRate: this.terminalStore.settings.vatRate
      });
      this.$emit("ok", data);
      this.numpadDialog = false;
    }
  }
};
</script>