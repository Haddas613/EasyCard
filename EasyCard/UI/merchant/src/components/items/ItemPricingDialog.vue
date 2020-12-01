<template>
  <ec-dialog :dialog.sync="visible">
    <template v-slot:title>{{$t('EditItem')}}</template>
    <template>
      <v-flex class="px-2">
        <v-row no-gutters>
          <v-col cols="12">
            <v-text-field
              class="mx-2 mt-4"
              v-model="model.itemName"
              outlined
              :label="$t('Name')"
              hide-details="true"
            ></v-text-field>
          </v-col>
        </v-row>
        <v-row no-gutters>
          <v-col cols="12" md="6">
            <v-text-field
              class="mx-2 mt-4"
              v-model="model.price"
              outlined
              :label="$t('Price')"
              hide-details="true"
              @input="calculateItemPricing()"
            ></v-text-field>
          </v-col>
          <v-col cols="12" md="6">
            <v-text-field
              class="mx-2 mt-4"
              v-if="model"
              :value="model.netAmount"
              outlined
              readonly
              disabled
              :label="$t('NetAmount')"
              hide-details="true"
            ></v-text-field>
          </v-col>
          <v-col cols="12" md="6">
            <v-text-field
              class="mx-2 mt-4"
              v-if="model"
              v-model.number="discount"
              outlined
              :label="$t('Discount')"
              hide-details="true"
              @input="calculateItemPricing()"
            >
              <template v-slot:append>
                <v-btn
                  class="shadow-none"
                  :color="percentageMode ? 'primary' : 'eclgray'"
                  fab
                  style="margin-top:-4px"
                  x-small
                  @click="percentageMode = !percentageMode; calculateItemPricing()"
                  :title="$t('ApplyAsPercentage')"
                >
                  <v-icon>mdi-percent</v-icon>
                </v-btn>
              </template>
            </v-text-field>
          </v-col>
          <v-col cols="12" md="6">
            <v-text-field
              class="mx-2 mt-4"
              v-if="model"
              :value="model.amount"
              outlined
              readonly
              disabled
              :label="$t('Total')"
              hide-details="true"
            ></v-text-field>
          </v-col>
        </v-row>
        <v-row no-gutters>
          <v-col cols="12" md="6">
            <v-text-field
              class="mx-2 mt-4"
              v-if="model"
              :value="model.vat"
              outlined
              readonly
              disabled
              :label="$t('VAT')"
              hide-details="true"
            ></v-text-field>
          </v-col>
          <v-col cols="12" md="6">
            <v-text-field
              class="mx-2 mt-4"
              v-if="model"
              :value="`${(terminalStore.settings.vatRate * 100).toFixed(0)}%`"
              readonly
              disabled
              outlined
              :label="$t('VATPercent')"
              hide-details="true"
            ></v-text-field>
          </v-col>
        </v-row>
        <div class="d-flex px-2 pt-4 justify-end">
          <v-btn
            color="primary"
            class="white--text"
            :block="$vuetify.breakpoint.smAndDown"
            @click="ok()"
          >{{$t("OK")}}</v-btn>
        </div>
      </v-flex>
    </template>
  </ec-dialog>
</template>

<script>
import itemPricingService from "../../helpers/item-pricing";
import { mapState } from "vuex";
export default {
  props: {
    item: {
      type: Object,
      required: true
    },
    show: {
      type: Boolean,
      default: false,
      required: true
    }
  },
  components: {
    EcDialog: () => import("../../components/ec/EcDialog")
  },
  data() {
    return {
      model: { ...this.item },
      percentageMode: false,
      discount: this.item.discount,
    };
  },
  watch: {
    show(newValue, oldValue) {
      if (newValue) {
        this.$set(this, "model", this.item);
        this.discount = this.item.discount;
        this.percentageMode = false;
      }
    }
  },
  computed: {
    visible: {
      get: function() {
        return this.show;
      },
      set: function(val) {
        this.$emit("update:show", val);
      }
    },
    ...mapState({
      terminalStore: state => state.settings.terminal
    })
  },
  methods: {
    async ok() {
      //TODO: validation
      let result = { ...this.model };
      result.amount = result.price * result.quantity - result.discount;
      this.$emit("ok", result);
    },
    calculateItemPricing() {
      if (this.percentageMode) {
        if (this.discount >= 100) {
          return this.$toasted.show(
            this.$t("PercentageShouldBeLessThanOneHundred"),
            { type: "error" }
          );
        }

        let discountTemp =
          ((this.model.price * this.model.quantity) / 100) * this.discount;
        if (discountTemp) {
          discountTemp = discountTemp.toFixed(2);
        }

        this.model.discount = parseFloat(discountTemp);
      } else {
        this.model.discount = this.discount
          ? parseFloat(this.discount.toFixed(2))
          : 0;
      }

      itemPricingService.item.calculate(this.model, { vatRate: this.terminalStore.settings.vatRate });
    }
  }
};
</script>