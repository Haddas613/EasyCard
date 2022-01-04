<template>
  <ec-dialog :dialog.sync="visible">
    <template v-slot:title>{{$t('FilterTransmissions')}}</template>
    <template v-slot:right>
      <v-btn color="primary" @click="apply()" :disabled="!formIsValid">{{$t('Apply')}}</v-btn>
    </template>
    <template>
      <div class="d-flex px-4 py-2 justify-end">
        <v-btn
          color="primary"
          :block="$vuetify.breakpoint.smAndDown"
          outlined
          @click="resetFilter()"
        >{{$t("Reset")}}</v-btn>
      </div>
      <div class="px-4 py-2">
        <v-form ref="form" v-model="formIsValid">
          <v-row>
            <!-- <v-col cols="12" md="12" class="pb-2 pt-0">
              <customer-dialog-invoker 
              :key="model.consumerID" 
              :terminal="model.terminalID" 
              :customer-id="model.consumerID"
              @update="processCustomer($event)"></customer-dialog-invoker>
            </v-col> -->
            <!-- <v-col cols="12" md="6" class="py-0">
              <v-select
                :items="dictionaries.quickDateFilterTypeEnum"
                item-text="description"
                item-value="code"
                v-model="model.quickDateFilter"
                :label="$t('QuickDate')"
                outlined
                clearable
              ></v-select>
            </v-col> -->
            <date-from-to-filter class="px-3" v-model="model"></date-from-to-filter>
            <v-col cols="12">
              <terminal-select v-model="model.terminalID" clearable></terminal-select>
            </v-col>
          </v-row>
        </v-form>
      </div>
    </template>
  </ec-dialog>
</template>

<script>
import { mapState } from "vuex";
import ValidationRules from "../../helpers/validation-rules";

export default {
  components: {
    EcRadioGroup: () => import("../../components/inputs/EcRadioGroup"),
    EcDialog: () => import("../../components/ec/EcDialog"),
    CustomerDialogInvoker: () => import("../../components/dialog-invokers/CustomerDialogInvoker"),
    DateFromToFilter: () => import("../filtering/DateFromToFilter"),
  },
  props: {
    show: {
      type: Boolean,
      default: false,
      required: true
    },
    filter: {
      type: Object,
      default: null,
      required: true
    }
  },
  data() {
    return {
      model: { 
        consumerID: null,
        terminalID: null,
        ...this.filter
       },
      dictionaries: {},
      customersDialog: false,
      selectedCustomer: null,
      formIsValid: true,
      vr: ValidationRules,
      paymentTypesFiltered: null
    };
  },
  async mounted() {
    this.dictionaries = await this.$api.dictionaries.getTransactionDictionaries();
    this.paymentTypesFiltered = this.lodash.filter(this.dictionaries.paymentTypeEnum, e => e.code == "bank" || e.code == "card");

    if(!this.model.terminalID){
      this.model.terminalID = this.terminalStore.terminalID;
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
    }),
  },
  methods: {
    apply() {
      if(!this.$refs.form.validate()){
        return;
      }

      this.$emit("ok", this.model);
      this.visible = false;
    },
    processCustomer(data) {
      this.$set(this.model, 'terminalID', data.terminalID);
      this.model.consumerID = data.consumerID;
    },
    resetFilter(){
      this.model = {};
      this.selectedCustomer = null;
    }
  }
};
</script>