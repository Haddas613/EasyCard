<template>
  <ec-dialog :dialog.sync="visible" color="ecbg">
    <template v-slot:title>{{ $t('DeleteCustomer') }}</template>
    <template>
      <v-row class="bg">
        <v-col cols="12">
            <h3>{{$t("DeleteCustomerMessageTitle")}}</h3>
            <p class="pt-2">{{$t("DeleteCustomerMessageText")}}</p>
        </v-col>
        <v-col cols="12" class="d-flex justify-end" v-if="!$vuetify.breakpoint.smAndDown">
          <v-btn class="mx-1" color="white" @click="visible = false">{{ $t('Cancel') }}</v-btn>
          <v-btn color="error" @click="confirm()">{{
            $t('Confirm')
          }}</v-btn>
        </v-col>
        <v-col class="px-2" cols="12" v-if="$vuetify.breakpoint.smAndDown">
          <v-btn block color="white" @click="visible = false">{{ $t('Cancel') }}</v-btn>
          <v-spacer class="py-2"></v-spacer>
          <v-btn block color="error" @click="confirm()">{{
            $t('Confirm')
          }}</v-btn>
        </v-col>
      </v-row>
    </template>
  </ec-dialog>
</template>

<script>
export default {
  props: {
    show: {
      type: Boolean,
      default: false,
      required: true,
    },
  },
  components: {
    EcDialog: () => import('../../components/ec/EcDialog'),
  },
  methods: {
    confirm() {
      this.$emit('ok');
      this.visible = false;
    },
  },
  computed: {
    visible: {
      get: function() {
        return this.show;
      },
      set: function(val) {
        this.$emit('update:show', val);
      },
    },
  },
};
</script>
