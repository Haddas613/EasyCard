<template>
  <v-form class="pt-2" ref="form" v-model="valid" lazy-validation>
    <v-row>
      <v-col cols="12" md="6" class="py-0">
        <v-text-field
          v-model="model.businessName"
          :counter="50"
          :rules="[vr.primitives.required, vr.primitives.stringLength(3, 50)]"
          :label="$t('BusinessName')"
          class="px-1"
          outlined
        ></v-text-field>
      </v-col>
     <v-col cols="12" md="6" class="py-0">
        <v-text-field
          v-model="model.marketingName"
          :counter="50"
          :rules="[vr.primitives.required, vr.primitives.stringLength(3, 50)]"
          :label="$t('MarketingName')"
          class="px-1"
          outlined
        ></v-text-field>
      </v-col>
      <v-col cols="12" md="6" class="py-0">
        <v-text-field
          v-model="model.businessID"
          :counter="9"
          :rules="[vr.primitives.stringLength(9, 9)]"
          :label="$t('BusinessID')"
          class="px-1"
          outlined
        ></v-text-field>
      </v-col>
       <v-col cols="12" md="6" class="py-0">
        <v-text-field
          v-model="model.contactPerson"
          :counter="50"
          :rules="[vr.primitives.stringLength(3, 50)]"
          :label="$t('ContactPerson')"
          class="px-1"
          outlined
        ></v-text-field>
      </v-col>
      <v-col cols="12" class="d-flex justify-end" v-if="!$vuetify.breakpoint.smAndDown">
        <v-btn class="mx-1" color="white" :to="{ name: 'Merchants' }">{{$t('Cancel')}}</v-btn>
        <v-btn color="primary" @click="ok()">{{$t('Save')}}</v-btn>
      </v-col>
      <v-col cols="12" v-if="$vuetify.breakpoint.smAndDown">
        <v-btn block color="white" :to="{ name: 'Merchants' }">{{$t('Cancel')}}</v-btn>
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
      this.$emit('ok', this.model);
    }
  },
  computed: {
    ...mapState({
      terminalStore: state => state.settings.terminal
    })
  },
};
</script>

<style lang="scss" scoped>
</style>