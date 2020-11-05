<template>
  <ec-dialog :dialog.sync="visible">
    <template v-slot:title>{{$t('EditItem')}}</template>
    <template>
      <div class="d-flex px-2 pt-4 justify-end">
        <v-btn
          color="red"
          class="white--text"
          :block="$vuetify.breakpoint.smAndDown"
          @click="model.amount = 0; ok()"
        >
          <v-icon left>mdi-delete</v-icon>
          {{$t("Delete")}}
        </v-btn>
      </div>
      <v-row no-gutters>
        <v-col cols="12" md="6">
          <v-text-field
            class="mx-2 mt-4"
            v-if="model"
            :value="model.price.toFixed(2)"
            outlined
            readonly
            :label="$t('Price')"
            hide-details="true"
          ></v-text-field>
        </v-col>
        <v-col cols="12" md="6">
          <v-text-field
            class="mx-2 mt-4"
            v-if="model"
            v-model.number="model.amount"
            outlined
            type="number"
            :label="$t('Quantity')"
            @change="amountChanged()"
            hide-details="true"
          ></v-text-field>
        </v-col>
        <v-col cols="12" md="4">
          <v-text-field
            class="mx-2 mt-4"
            v-if="model"
            :value="(model.price * model.amount).toFixed(2)"
            outlined
            readonly
            :label="$t('NetAmount')"
            hide-details="true"
          ></v-text-field>
        </v-col>
        <v-col cols="12" md="4">
          <v-text-field
            class="mx-2 mt-4"
            v-if="model"
            v-model.number="discount"
            outlined
            :label="$t('Discount')"
            hide-details="true"
            @input="calculateItemDiscount()"
          >
            <template v-slot:append>
              <v-btn
                class="shadow-none"
                :color="percentageMode ? 'primary' : 'eclgray'"
                fab
                style="margin-top:-4px"
                x-small
                @click="percentageMode = !percentageMode; calculateItemDiscount()"
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
            v-if="model"
            :value="totalAmount"
            outlined
            readonly
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
            :value="(totalAmount * 0.17).toFixed(2)"
            outlined
            readonly
            :label="$t('VAT')"
            hide-details="true"
          ></v-text-field>
        </v-col>
        <v-col cols="12" md="6">
          <v-text-field
            class="mx-2 mt-4"
            v-if="model"
            value="17%"
            readonly
            outlined
            :label="$t('VATPercent')"
            @change="amountChanged()"
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
    </template>
  </ec-dialog>
</template>

<script>
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
      discount: this.item.discount
    };
  },
  watch: {
    show(newValue, oldValue) {
      if (newValue) {
        this.$set(this, 'model', this.item);
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
    totalAmount: {
      get: function(){
        return ((this.model.price * this.model.amount) - this.model.discount).toFixed(2);
      }
    }
  },
  methods: {
    amountChanged() {
      if (this.model && !this.model.amount) {
        this.ok();
      } else {
        this.calculateItemDiscount();
      }
    },
    async ok() {
      //TODO: validation
      this.$emit("ok", this.model);
    },
    calculateItemDiscount() {
      if (this.percentageMode) {
        if (this.discount >= 100) {
          return this.$toasted.show(
            this.$t("PercentageShouldBeLessThanOneHundred"),
            { type: "error" }
          );
        }

        this.model.discount = (
          ((this.model.price * this.model.amount) / 100) *
          this.discount
        ).toFixed(2);
      } else {
        this.model.discount = this.discount;
      }
    }
  }
};
</script>