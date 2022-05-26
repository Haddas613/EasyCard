<template>
  <ec-dialog :dialog.sync="visible">
    <template v-slot:title>{{$t('EditItem')}}</template>
    <template>
      <v-flex class="px-2">
        <v-form ref="form" v-model="formIsValid">
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
            <v-col cols="12" md="12">
              <v-text-field
                class="mx-2 mt-4"
                v-model.number="model.quantity"
                outlined
                :label="$t('Quantity')"
                hide-details="true"
                min="1"
                max="100000"
                step="1"
                type="number"
                :rules="[vr.primitives.required, vr.primitives.numeric()]"
                @input="calculateItemPricing()"
              ></v-text-field>
            </v-col>
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
                :value="model.netAmount"
                outlined
                readonly
                disabled
                :label="$t('NetAmount')"
                hide-details="true"
              ></v-text-field>
            </v-col>
            <v-col cols="8" md="4">
              <v-text-field
                v-if="discountType == 'raw'"
                class="mx-2 mt-4"
                v-model.number="discount"
                outlined
                :label="$t('Discount')"
                hide-details="true"
                :rules="[vr.primitives.inRange(0, model.quantity * model.price), vr.primitives.positiveOnly, vr.primitives.precision(2)]"
                @input="calculateItemPricing()"
              >
              </v-text-field>
              <v-text-field
                class="mx-2 mt-4"
                v-else
                v-model.number="percentageDiscount"
                outlined
                :label="$t('Discount')"
                :hide-details="formIsValid"
                :rules="[vr.primitives.inRange(0.01, 100), vr.primitives.precision(2)]"
                @input="applyPercentageDiscount()"
              >
              </v-text-field>
            </v-col>
            <v-col cols="4" md="2">
              <v-btn-toggle class="mt-5" v-model="discountType" color="secondary" @change="onDiscountTypeChange()">
                <v-btn value="raw" icon>
                  <v-icon color="primary">mdi-cash</v-icon>
                </v-btn>
                <v-btn value="percent" icon>
                  <v-icon color="primary">mdi-percent</v-icon>
                </v-btn>
              </v-btn-toggle>
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
                :value="`${(basket.vatRate * 100).toFixed(0)}%`"
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
              :disabled="!formIsValid"
            >{{$t("OK")}}</v-btn>
          </div>
        </v-form>
      </v-flex>
    </template>
  </ec-dialog>
</template>

<script>
import itemPricingService from "../../helpers/item-pricing";
import ValidationRules from "../../helpers/validation-rules";
import { mapState } from "vuex";
export default {
  props: {
    item: {
      type: Object,
      required: true
    },
    basket:{
      type: Object,
      required: true
    },
    show: {
      type: Boolean,
      default: false,
      required: true
    },
    // currency: {
    //   type: String,
    //   default: false,
    //   required: true
    // }
  },
  components: {
    EcDialog: () => import("../../components/ec/EcDialog")
  },
  data() {
    return {
      model: { ...this.item },
      discountType: "raw",
      discount: this.item.discount,
      percentageDiscount: 0,
      vr: ValidationRules,
      formIsValid: false
    };
  },
  watch: {
    show(newValue, oldValue) {
      if (newValue) {
        this.$set(this, "model", this.item);
        this.discount = this.item.discount;
        this.discountType = "raw";
        this.percentageDiscount = 0;
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
      if (!this.$refs.form.validate()) return;
      let result = { ...this.model };
      
      this.$emit("ok", result);
    },
    onDiscountTypeChange(){
      if(this.discountType == "percent"){
        this.calculatePercentageDiscount();
      }
    },
    applyPercentageDiscount(){
      if (!this.$refs.form.validate()) return;
      this.discount = parseFloat((this.model.price * this.model.quantity * (this.percentageDiscount / 100)).toFixed(2));
      this.calculateItemPricing();
    },
    calculatePercentageDiscount(){
      if(this.discount >= this.model.price){
        this.discount = this.model.price;
        this.percentageDiscount = 100;
        this.calculateItemPricing();
      }else{
        this.percentageDiscount = parseFloat((this.discount / this.model.price * 100).toFixed(2));
      }
    },
    calculateItemPricing() {
      this.model.discount = this.discount
          ? parseFloat(this.discount.toFixed(2))
          : 0;
      itemPricingService.item.calculate(this.model, { vatRate: this.basket.vatRate });
    }
  }
};
</script>