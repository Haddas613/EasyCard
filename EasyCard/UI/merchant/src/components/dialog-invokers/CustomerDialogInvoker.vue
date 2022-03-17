<template>
  <div>
    <customer-create-dialog
      :terminal="terminal"
      :show.sync="customerCreateDialog"
      v-on:ok="processCustomer($event)"
    ></customer-create-dialog>
    <ec-dialog :dialog.sync="visible" color="ecbg">
      <template v-slot:title>{{$t('Customers')}}</template>
      <template>
        <div class="d-flex pb-2 justify-end">
          <v-row justify="space-between" no-gutters>
            <v-spacer class="hidden-sm-and-down"></v-spacer>
            <!-- <v-col cols="12" md="4" lg="3">
              <v-btn
                color="red"
                small
                block
                class="white--text px-1"
                :disabled="selectedCustomer == null"
                @click="selectedCustomer = null; customersDialog = false;"
              >
                <v-icon left>mdi-delete</v-icon>
                {{$t("CancelSelection")}}
              </v-btn>
            </v-col> -->
            <v-col cols="12" md="4" lg="3">
              <v-btn
                color="success"
                small
                block
                class="px-1"
                @click="customerCreateDialog = true;"
              >
                <v-icon left>mdi-plus</v-icon>
                {{$t("CreateCustomer")}}
              </v-btn>
            </v-col>
          </v-row>
        </div>
        <customers-list
          :key="terminal"
          :show-previously-charged="true"
          :filter-by-terminal="terminal"
          v-on:ok="processCustomer($event)"
        ></customers-list>
      </template>
    </ec-dialog>
    <ec-dialog-invoker v-if="!dialogOnly" v-on:click="showDialog()">
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
    },
    //component can function as dialog invoker or purely as dialog dependent on dialogOnly prop
    //if dialogOnly is specified, show prop is required as well
    dialogOnly:{
      type: Boolean,
      default: false
    },
    show: {
      type: Boolean,
      default: false
    },
    fullCustomerInfo: {
      type: Boolean,
      default: false,
    }
  },
  components: {
    EcDialog: () => import("../../components/ec/EcDialog"),
    EcDialogInvoker: () => import("../../components/ec/EcDialogInvoker"),
    ReIcon: () => import("../../components/misc/ResponsiveIcon"),
    CustomersList: () => import("../../components/customers/CustomersList"),
    CustomerCreateDialog: () =>
      import("../../components/customers/CustomerCreateDialog")
  },
  data() {
    return {
      selectedCustomer: null,
      customersDialog: false,
      customerCreateDialog: false
    };
  },
  async mounted() {
    if (this.customerId) {
      let customer = await this.$api.consumers.getConsumer(this.customerId);
      this.processCustomer(customer);
    }
  },
  computed: {
    visible: {
      get: function() {
        return this.dialogOnly ? this.show : this.customersDialog;
      },
      set: function(val) {
        if(this.dialogOnly){
          this.$emit("update:show", val);
        }else{
          this.customersDialog = val;
        }
      }
    }
  },
  methods: {
    async processCustomer(data) {
      if (this.fullCustomerInfo){
        data = await this.$api.consumers.getConsumer(data.consumerID);
      }
      this.selectedCustomer = data;
      this.$emit("update", data);
      this.visible = false;
      this.customerCreateDialog = false;
    },
    showDialog() {
      this.visible = true;
    }
  }
};
</script>