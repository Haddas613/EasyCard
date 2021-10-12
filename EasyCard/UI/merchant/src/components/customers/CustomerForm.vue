<template>
  <v-form class="pt-2" ref="form" v-model="valid" lazy-validation>
    <customer-form-fields ref="customerFormFields" :data="model"></customer-form-fields>
    <v-row>
      <v-col cols="12" class="d-flex justify-end" v-if="!$vuetify.breakpoint.smAndDown">
        <v-btn class="mx-1" color="white" :to="{ name: 'Customers' }">{{$t('Cancel')}}</v-btn>
        <v-btn color="primary" @click="ok()">{{$t('Save')}}</v-btn>
      </v-col>
      <v-col cols="12" v-if="$vuetify.breakpoint.smAndDown">
        <v-btn block color="white" :to="{ name: 'Customers' }">{{$t('Cancel')}}</v-btn>
        <v-spacer class="py-2"></v-spacer>
        <v-btn block color="primary" @click="ok()">{{$t('Save')}}</v-btn>
      </v-col>
    </v-row>
  </v-form>
</template>

<script>
import ValidationRules from "../../helpers/validation-rules";
import { mapState } from "vuex";

export default {
  components: {
    CustomerFormFields: () => import("./CustomerFormFields"),
  },
  props: {
    data: {
      type: Object,
      default: null,
      required: true
    }
  },
  data() {
    return {
      model: { ...this.data },
      vr: ValidationRules,
      terminals: [],
      valid: true
    };
  },
  methods: {
    ok() {
      if (!this.$refs.form.validate()) return;
      let result = {...this.model, ...this.$refs.customerFormFields.getData()}
      this.$emit('ok', result);
    }
  }
};
</script>