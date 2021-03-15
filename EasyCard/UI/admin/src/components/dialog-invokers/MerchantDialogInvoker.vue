<template>
  <div>
    <ec-dialog :dialog.sync="merchantsDialog" color="ecbg">
      <template v-slot:title>{{$t('Merchants')}}</template>
      <template>
        <div class="d-flex pb-2 justify-end">
          <v-btn
            color="red"
            class="white--text"
            :disabled="selectedMerchant == null"
            :block="$vuetify.breakpoint.smAndDown"
            @click="selectedMerchant = null; merchantsDialog = false;"
          >
            <v-icon left>mdi-delete</v-icon>
            {{$t("CancelSelection")}}
          </v-btn>
        </div>
        <merchants-list
          :key="terminal"
          :show-previously-charged="true"
          :filter-by-terminal="terminal"
          v-on:ok="processMerchant($event)"
        ></merchants-list>
      </template>
    </ec-dialog>
    <ec-dialog-invoker v-on:click="merchantsDialog = true">
      <template v-slot:prepend>
        <v-icon>mdi-account</v-icon>
      </template>
      <template v-slot:left>
        <div v-if="!selectedMerchant">{{$t("ChooseMerchant")}}</div>
        <div v-if="selectedMerchant">
          <span class="primary--text">{{selectedMerchant.consumerName}}</span>
        </div>
      </template>
      <template v-slot:append>
        <re-icon>mdi-chevron-right</re-icon>
      </template>
    </ec-dialog-invoker>
  </div>
</template>

<script>
export default {
  props: {
    merchantID: {
      type: String,
      default: null
    },
    terminal: {
        default: null
    }
  },
  components: {
    EcDialog: () => import("../../components/ec/EcDialog"),
    EcDialogInvoker: () => import("../../components/ec/EcDialogInvoker"),
    ReIcon: () => import("../../components/misc/ResponsiveIcon"),
    MerchantsList: () => import("../../components/merchants/MerchantsList")
  },
  data() {
    return {
      selectedMerchant: null,
      merchantsDialog: false
    };
  },
  async mounted() {
    if (this.merchantID) {
      this.processMerchant(
        await this.$api.merchants.getMerchant(this.merchantID)
      );
    }
  },
  methods: {
    processMerchant(data) {
      this.selectedMerchant = data;
      this.$emit("update", data);
      this.merchantsDialog = false;
    }
  }
};
</script>