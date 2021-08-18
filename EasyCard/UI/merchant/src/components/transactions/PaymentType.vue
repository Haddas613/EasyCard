<template>
  <v-flex>
    <ec-dialog-invoker v-on:click="paymentTypeDialog = true" class="py-2">
      <template v-slot:left>
        <div class="font-weight-medium">{{$t("PaymentType")}}</div>
      </template>
      <template v-slot:right>
        <div>{{paymentType ? paymentType.description : $t('PleaseSelect')}}</div>
      </template>
      <template v-slot:append>
        <re-icon>mdi-chevron-right</re-icon>
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

    this.paymentTypesFiltered = this.lodash.filter(
      this.dictionaries.paymentTypeEnum,
      e => this.excludeTypes.indexOf(e.code) === -1
    );
    if (!this.paymentType) {
      this.paymentType = this.paymentTypesFiltered[0];
      this.paymentTypeChanged();
    }
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