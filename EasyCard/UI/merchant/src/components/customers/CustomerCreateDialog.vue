<template>
  <ec-dialog :dialog.sync="visible" color="ecbg">
    <template v-slot:title>{{$t('CreateCustomer')}}</template>
    <template>
      <v-form class="pt-2" ref="form" v-model="valid" lazy-validation>
        <customer-form-fields
          :data="model"
          v-on:ok="createCustomer($event)"
          disable-terminal
          ref="customerFormFields"
          class="px-4"
          v-if="model"
        ></customer-form-fields>
        <v-row no-gutters>
          <v-col cols="12" class="d-flex justify-end" v-if="!$vuetify.breakpoint.smAndDown">
            <v-btn class="mx-1" color="white" @click="visible = false">{{$t('Cancel')}}</v-btn>
            <v-btn color="primary" @click="createCustomer()" :loading="loading">{{$t('Save')}}</v-btn>
          </v-col>
          <v-col class="px-2" cols="12" v-if="$vuetify.breakpoint.smAndDown">
            <v-btn block color="white" @click="visible = false">{{$t('Cancel')}}</v-btn>
            <v-spacer class="py-2"></v-spacer>
            <v-btn block color="primary" @click="createCustomer()" :loading="loading">{{$t('Save')}}</v-btn>
          </v-col>
        </v-row>
      </v-form>
    </template>
  </ec-dialog>
</template>

<script>
export default {
  props: {
    show: {
      type: Boolean,
      default: false,
      required: true
    },
    terminal: {
      default: null,
      required: true
    }
  },
  components: {
    EcDialog: () => import("../../components/ec/EcDialog"),
    EcDialogInvoker: () => import("../../components/ec/EcDialogInvoker"),
    ReIcon: () => import("../../components/misc/ResponsiveIcon"),
    CustomersList: () => import("../../components/customers/CustomersList"),
    CustomerFormFields: () =>
      import("../../components/customers/CustomerFormFields")
  },
  data() {
    return {
      selectedCustomer: null,
      model: {
        terminalID: this.terminal,
        consumerName: null,
        consumerPhone: null,
        consumerEmail: null,
        consumerAddress: {
          city: null,
          zip: null,
          street: null,
          house: null,
          apartment: null
        },
        externalReference: null
      },
      valid: false,
      loading: false
    };
  },
  methods: {
    async createCustomer() {
      if (!this.$refs.form.validate() || this.loading) return;
      this.model = {...this.model, ...this.$refs.customerFormFields.getData()}

      this.loading = true;
      let result = await this.$api.consumers.createConsumer(this.model);
      this.loading = false;
      if (!this.$apiSuccess(result)) return;

      this.processCustomer({
        ...this.model,
        consumerID: result.entityReference
      });
    },
    processCustomer(data){
      this.$emit("ok", data);
      this.visible = false;
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
  }
};
</script>