<template>
  <ec-dialog :dialog.sync="visible">
    <template v-slot:title>{{$t('PauseBillingDeal')}}</template>
    <template>
      <v-form ref="form" lazy-validation>
        <v-row>
          <date-from-to-filter class="px-3" v-model="model" from-today required></date-from-to-filter>
        </v-row>
        <v-row>
          <v-col cols="12">
            <v-text-field
              v-model="model.reason"
              :label="$t('Reason(Optional)')"
              :rules="[vr.primitives.stringLength(1, 250)]"
              :counter="250"
              outlined
            ></v-text-field>
          </v-col>
        </v-row>
      </v-form>
      <div class="d-flex justify-end">
        <v-btn @click="cancel()" :loading="loading" class="mx-4">{{$t("Cancel")}}</v-btn>
        <v-btn @click="unpauseBilling()" :loading="loading" color="primary">{{$t("Unpause")}}</v-btn>
        <v-btn
          class="mx-1"
          color="success"
          @click="pauseBilling()"
          :loading="loading"
        >{{$t("Save")}}</v-btn>
      </div>
    </template>
  </ec-dialog>
</template>

<script>
import ValidationRules from "../../helpers/validation-rules";

export default {
  props: {
    show: {
      type: Boolean,
      default: false,
      required: true
    },
    billing: {
      type: Object,
      required: true
    }
  },
  components: {
    EcDialog: () => import("../../components/ec/EcDialog"),
    DateFromToFilter: () => import("../filtering/DateFromToFilter")
  },
  data() {
    return {
      model: {
        dateFrom: null,
        dateTo: null,
        reason: null
      },
      vr: ValidationRules,
      loading: false
    };
  },
  mounted() {
    if (this.billing) {
      this.model.dateFrom = this.billing.$pausedFrom || this.billing.pausedFrom;
      this.model.dateTo = this.billing.$pausedTo || this.billing.pausedFrom;
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
    }
  },
  methods: {
    async pauseBilling() {
      if (!this.$refs.form.validate()) {
        return;
      }
      var operation = await this.$api.billingDeals.pauseBillingDeal((this.billing.$billingDealID || this.billing.billingDealID), this.model);
      
      if(operation.status != "success"){
        this.$toasted.show(operation ? operation.message : this.$t("SomethingWentWrong"), {
          type: "error"
        });
        return;
      }else{
        let $eventData = { ...this.model, paused: true };
        //this.reset();
        this.visible = false;
        this.$emit('ok', $eventData);
      }
    },
    async unpauseBilling(){
      var operation = await this.$api.billingDeals.unpauseBillingDeal(this.billing.$billingDealID || this.billing.billingDealID);
      if(operation.status != "success"){
        this.$toasted.show(operation ? operation.message : this.$t("SomethingWentWrong"), {
          type: "error"
        });
        return;
      }
      else{
        this.reset();
        let $eventData = { ...this.model, paused: false };
        this.visible = false;
        this.$emit('ok', $eventData);
      }
    },
    cancel() {
      //this.reset();
      this.visible = false;
    },
    reset() {
      this.model = {
        dateFrom: null,
        dateTo: null,
        reason: null
      };
    }
  }
};
</script>