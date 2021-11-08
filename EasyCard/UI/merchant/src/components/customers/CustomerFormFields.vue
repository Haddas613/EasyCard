<template>
  <v-row class="pt-2" no-gutters>
    <v-col cols="12" md="6" class="pb-0">
      <terminal-select
        class="px-1"
        v-model="model.terminalID"
        :disabled="disableTerminal || model.consumerID != null"
        required
      ></terminal-select>
    </v-col>
    <v-col cols="12" md="6" class="pb-0">
      <v-text-field
        v-model="model.consumerName"
        :counter="50"
        :rules="[vr.primitives.required, vr.primitives.maxLength(50)]"
        :label="$t('Name')"
        class="px-1"
        outlined
      ></v-text-field>
    </v-col>
    <v-col cols="12" md="6" class="py-0">
      <v-text-field
        v-model="model.consumerPhone"
        :counter="50"
        :rules="[vr.primitives.required, vr.primitives.maxLength(50)]"
        :label="$t('Phone')"
        class="px-1"
        outlined
      ></v-text-field>
    </v-col>
    <v-col cols="12" md="6" class="py-0">
      <v-text-field
        v-model="model.consumerEmail"
        :counter="50"
        :rules="[vr.primitives.required, vr.primitives.email]"
        :label="$t('Email')"
        class="px-1"
        outlined
      ></v-text-field>
    </v-col>
    <v-col cols="12" md="6" class="py-0">
      <v-text-field
        v-model="model.consumerNationalID"
        :counter="50"
        :rules="[vr.special.israeliNationalId]"
        :label="$t('NationalID')"
        class="px-1"
        outlined
      ></v-text-field>
    </v-col>
    <v-col cols="12" md="6" class="py-0">
      <v-text-field
        v-model="model.externalReference"
        :counter="50"
        :rules="[vr.primitives.maxLength(50)]"
        :label="$t('ExternalReference')"
        class="px-1"
        outlined
      ></v-text-field>
    </v-col>
    <v-col cols="12" md="6" class="py-0">
      <v-text-field
        v-model="model.origin"
        :counter="50"
        :rules="[vr.primitives.maxLength(50)]"
        :label="$t('Origin')"
        class="px-1"
        outlined
      ></v-text-field>
    </v-col>
    <v-col cols="12" md="4" class="py-0">
      <v-text-field
        v-model="model.consumerAddress.city"
        :counter="50"
        :rules="[vr.primitives.maxLength(50)]"
        :label="$t('City')"
        class="px-1"
        outlined
      ></v-text-field>
    </v-col>
    <v-col cols="12" md="2" class="py-0">
      <v-text-field
        v-model="model.consumerAddress.zip"
        :counter="50"
        :rules="[vr.primitives.maxLength(50)]"
        :label="$t('Zip')"
        class="px-1"
        outlined
      ></v-text-field>
    </v-col>
    <v-col cols="12" md="6" class="py-0">
      <v-text-field
        v-model="model.consumerAddress.street"
        :counter="50"
        :rules="[vr.primitives.maxLength(50)]"
        :label="$t('Street')"
        class="px-1"
        outlined
      ></v-text-field>
    </v-col>
    <v-col cols="12" md="6" class="py-0">
      <v-text-field
        v-model="model.consumerAddress.house"
        :counter="50"
        :rules="[vr.primitives.maxLength(50)]"
        :label="$t('House')"
        class="px-1"
        outlined
      ></v-text-field>
    </v-col>
    <v-col cols="12" md="6" class="py-0">
      <v-text-field
        v-model="model.consumerAddress.apartment"
        :counter="50"
        :rules="[vr.primitives.maxLength(50)]"
        :label="$t('Apartment')"
        class="px-1"
        outlined
      ></v-text-field>
    </v-col>
  </v-row>
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
    },
    disableTerminal: {
      type: Boolean,
      default: false
    }
  },
  data() {
    return {
      model: { ...this.data },
      vr: ValidationRules,
      terminals: []
    };
  },
  computed: {
    ...mapState({
      terminalStore: state => state.settings.terminal
    })
  },
  async mounted() {
    this.terminals = (await this.$api.terminals.getTerminals()).data || [];
    if (!this.model.terminalID) {
      this.model.terminalID = this.terminalStore
        ? this.terminalStore.terminalID
        : this.terminals[0].terminalID;
    }
    
    if(!this.model.consumerAddress || typeof(this.model.consumerAddress) != "object"){
      this.$set(this.model, 'consumerAddress', {});
    }
  },
  methods: {
    getData() {
      return this.model;
    }
  }
};
</script>