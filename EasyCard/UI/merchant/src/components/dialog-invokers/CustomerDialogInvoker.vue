<template>
  <div>
    <ec-dialog :dialog.sync="customersDialog" color="ecbg">
      <template v-slot:title>{{$t('Customers')}}</template>
      <template>
        <div class="d-flex pb-2 justify-end">
          <v-btn
            color="red"
            class="white--text"
            :disabled="selectedCustomer == null"
            :block="$vuetify.breakpoint.smAndDown"
            @click="selectedCustomer = null; customersDialog = false;"
          >
            <v-icon left>mdi-delete</v-icon>
            {{$t("CancelSelection")}}
          </v-btn>
        </div>
        <customers-list
          :key="terminal"
          :show-previously-charged="true"
          :filter-by-terminal="terminal"
          v-on:ok="processCustomer($event)"
        ></customers-list>
      </template>
    </ec-dialog>
    <ec-dialog-invoker v-on:click="customersDialog = true">
      <template v-slot:prepend>
        <v-icon>mdi-account</v-icon>
      </template>
      <template v-slot:left>
        <div v-if="!selectedCustomer">{{$t("ChooseCustomer")}}</div>
        <div v-if="selectedCustomer">
          <span class="primary--text">{{selectedCustomer.consumerName}}</span>
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
    customerId: {
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
    CustomersList: () => import("../../components/customers/CustomersList")
  },
  data() {
    return {
      selectedCustomer: null,
      customersDialog: false
    };
  },
  async mounted() {
    if (this.customerId) {
      this.processCustomer(
        await this.$api.consumers.getConsumer(this.customerId)
      );
    }
  },
  methods: {
    processCustomer(data) {
      this.selectedCustomer = data;
      this.$emit("update", data);
      this.customersDialog = false;
    }
  }
};
</script>