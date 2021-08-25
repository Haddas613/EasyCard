<template>
  <v-flex>
    <ec-dialog-invoker :clickable="!disabled" v-on:click="paymentTypeDialog = true" class="py-2">
      <template v-slot:left>
        <div v-bind:class="{'ecgray--text' : disabled }" class="font-weight-medium">{{$t("PaymentType")}}</div>
      </template>
      <template v-slot:right>
        <div>{{paymentType ? paymentType.description : $t('PleaseSelect')}}</div>
      </template>
      <template v-slot:append>
        <re-icon v-if="!disabled">mdi-chevron-right</re-icon>
        <v-icon small class="py-1" v-else>mdi-lock-outline</v-icon>
      </template>
    </ec-dialog-invoker>
    <ec-dialog :dialog.sync="paymentTypeDialog">
      <template v-slot:title>{{$t('PaymentType')}}</template>
      <template>
        <ec-radio-group
          :data="paymentTypesFiltered"
          labelkey="description"
          valuekey="code"
          return-object
          :model.sync="paymentType"
          v-on:change="paymentTypeChanged()"
        ></ec-radio-group>
      </template>
    </ec-dialog>
  </v-flex>
</template>

<script>
export default {
  components: {
    ReIcon: () => import("../../components/misc/ResponsiveIcon"),
    EcDialog: () => import("../../components/ec/EcDialog"),
    EcDialogInvoker: () => import("../../components/ec/EcDialogInvoker"),
    EcRadioGroup: () => import("../../components/inputs/EcRadioGroup")
  },
  model: {
    prop: "val",
    event: "change"
  },
  props: {
    val: {
      required: true
    },
    required: {
      type: Boolean,
      default: false
    },
    excludeTypes: {
      type: Array,
      default: () => [],
      required: false
    },
    all: {
      type: Boolean,
      default: false
    },
    disabled: {
      type: Boolean,
      default: false
    }
  },
  data() {
    return {
      dictionaries: {},
      paymentTypeDialog: false,
      paymentTypesFiltered: null,
      paymentType: null,
      value: this.val
    };
  },
  async mounted() {
    this.dictionaries = await this.$api.dictionaries.getTransactionDictionaries();

    let filtered = this.lodash.filter(
      this.dictionaries.paymentTypeEnum,
      e => this.excludeTypes.indexOf(e.code) === -1
    );

    if (this.all){
      this.paymentTypesFiltered = [{description: this.$t("All"), code: null}, ...filtered];
    }else{
      this.paymentTypesFiltered = filtered;
    }
    if (!this.val) {
      this.paymentType = this.paymentTypesFiltered[0];
    }else{
      this.paymentType = this.lodash.filter(filtered, t => t.code == this.val)[0];
    }
    
    this.paymentTypeChanged();
  },
  methods: {
    paymentTypeChanged() {
      this.value = this.paymentType.code;
      this.$emit("change", this.value);
      this.paymentTypeDialog = false;
    }
  }
};
</script>