<template>
  <v-form class="pt-2" ref="form" v-model="valid" lazy-validation>
    <v-row>
      <v-col cols="12">
        <v-select
          :items="terminals"
          item-text="label"
          item-value="terminalID"
          v-model="model.terminalID"
          outlined
          class="px-1"
          :label="$t('Terminal')"
          required
          :disabled="model.consumerID != null"
        ></v-select>
      </v-col>
      <v-col cols="12" class="py-0">
        <v-text-field
          v-model="model.consumerName"
          :counter="50"
          :rules="[vr.primitives.required, vr.primitives.maxLength(50)]"
          :label="$t('Name')"
          class="px-1"
          outlined
        ></v-text-field>
      </v-col>
     <v-col cols="12" class="py-0">
        <v-text-field
          v-model="model.consumerPhone"
          :counter="50"
          :rules="[vr.primitives.required, vr.primitives.maxLength(50)]"
          :label="$t('Phone')"
          class="px-1"
          outlined
        ></v-text-field>
      </v-col>
      <v-col cols="12" class="py-0">
        <v-text-field
          v-model="model.consumerEmail"
          :counter="50"
          :rules="[vr.primitives.required, vr.primitives.email]"
          :label="$t('Email')"
          class="px-1"
          outlined
        ></v-text-field>
      </v-col>
      <v-col cols="12" class="py-0">
        <v-text-field
          v-model="model.consumerAddress"
          :counter="50"
          :rules="[vr.primitives.maxLength(50)]"
          :label="$t('Address')"
          class="px-1"
          outlined
        ></v-text-field>
      </v-col>
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
  async mounted(){
    this.terminals = (await this.$api.terminals.getTerminals()).data || [];
    if(!this.model.terminalID){
      this.model.terminalID = this.terminals[0].terminalID;
    }
  }
};
</script>

<style lang="scss" scoped>
</style>