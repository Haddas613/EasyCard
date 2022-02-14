<template>
  <v-form class="pt-2" ref="form" v-model="valid" lazy-validation>
    <customer-form-fields ref="customerFormFields" :data="model"></customer-form-fields>
    <v-row>
      <v-col cols="12">
        <v-card class="ecbg">
          <v-row>
            <v-col cols="12" class="text-center">
              <v-btn color="success" v-if="!model.bankDetails" @click="addBankDetails()">{{
                $t("AddBankDetails")
              }}</v-btn>
              <v-btn color="error" v-else @click="deleteBankDetails()">{{
                $t("DeleteBankDetails")
              }}</v-btn>
            </v-col>
            <v-col cols="12" v-if="model.bankDetails">
              <bank-fields ref="bankDetails" :data="model.bankDetails"></bank-fields>
            </v-col>
          </v-row>
        </v-card>
      </v-col>
      <v-col cols="12" v-if="$vuetify.breakpoint.smAndDown">
        <v-btn block color="white" :to="{ name: 'Customers' }">{{ $t("Cancel") }}</v-btn>
        <v-spacer class="py-2"></v-spacer>
        <v-btn block color="primary" @click="ok()">{{ $t("Save") }}</v-btn>
      </v-col>
      <v-col cols="12" class="d-flex justify-end" v-if="!$vuetify.breakpoint.smAndDown">
        <v-btn class="mx-1" color="white" :to="{ name: 'Customers' }">{{ $t("Cancel") }}</v-btn>
        <v-btn color="primary" @click="ok()">{{ $t("Save") }}</v-btn>
      </v-col>
    </v-row>
  </v-form>
</template>

<script>
import ValidationRules from "../../helpers/validation-rules";

export default {
  components: {
    CustomerFormFields: () => import("./CustomerFormFields"),
    BankFields: () => import("../transactions/BankDetailsFields"),
  },
  props: {
    data: {
      type: Object,
      default: null,
      required: true,
    },
  },
  data() {
    return {
      model: { ...this.data },
      vr: ValidationRules,
      terminals: [],
      valid: true,
    };
  },
  methods: {
    ok() {
      if (!this.$refs.form.validate()) return;
      let result = { ...this.model, ...this.$refs.customerFormFields.getData()};

      if(this.$refs.bankDetails){
        result.bankDetails =  {...this.$refs.bankDetails.getData() };
      }

      this.$emit("ok", result);
    },
    addBankDetails() {
      this.$set(this.model, 'bankDetails', {
        bank: null,
        bankBranch: null,
        bankAccount: null,
      });
    },
    deleteBankDetails() {
      this.model.bankDetails = null;
    },
  },
};
</script>
