<template>
  <v-container fluid>
    <v-form v-model="formIsValid" ref="form">
      <v-row>
        <v-col cols="12" md="4">
          <v-text-field
            outlined
            :rules="[vr.primitives.guid]"
            v-model="model.searchGuid"
            :label="$t('UserOrMerchantID')"
            clearable
          ></v-text-field>
        </v-col>
        <v-col cols="12" md="4">
          <v-text-field outlined v-model="model.search" :label="$t('NameOrEmail')"></v-text-field>
        </v-col>
        <v-col cols="12" md="4">
          <v-select
            outlined
            :items="dictionaries.userStatusEnum"
            item-text="description"
            item-value="code"
            v-model="model.status"
            :label="$t('Status')"
            clearable
          ></v-select>
        </v-col>
      </v-row>
      <v-row>
        <v-col cols="12" class="d-flex justify-end pt-0 mt-0">
          <v-btn color="success" class="mr-4" @click="apply()" :disabled="!formIsValid">{{$t('Apply')}}</v-btn>
        </v-col>
      </v-row>
    </v-form>
  </v-container>
</template>

<script>
import ValidationRules from "../../helpers/validation-rules";
export default {
  name: "UsersFilter",
  components: {
    MerchantTerminalFilter: () => import("../filtering/MerchantTerminalFilter")
  },
  data() {
    return {
      model: { ...this.filterData },
      dictionaries: {},
      vr: ValidationRules,
      formIsValid: true,
    };
  },
  async mounted() {
    this.dictionaries = await this.$api.dictionaries.getMerchantDictionaries();
  },
  props: {
    filterData: {
      type: Object
    }
  },
  methods: {
    apply() {
      if(this.$refs.form.validate()){
        this.$emit("apply", this.model);
      }
    }
  }
};
</script>