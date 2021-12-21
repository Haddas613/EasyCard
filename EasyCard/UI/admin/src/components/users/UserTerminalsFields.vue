<template>
  <v-col cols="12" class="mt-2">
    <v-autocomplete
      :items="terminals"
      item-text="label"
      item-value="terminalID"
      v-model="userTerminals"
      outlined
      :label="$t('Terminal')"
      multiple
    ></v-autocomplete>
  </v-col>
</template>

<script>
import appConstants from "../../helpers/app-constants";

export default {
  props: {
    user: {
      type: Object,
      default: null,
      required: true
    }
  },
  data() {
    return {
      model: { ...this.user },
      userTerminals: [],
      appConstants: appConstants,
      terminals: []
    };
  },
  async mounted() {
    if(this.model.merchantID){
      let terminals = await this.$api.terminals.getTerminals({
        merchantID: this.model.merchantID
      });
      this.terminals = terminals ? terminals.data : [];
    }

    if(this.model.terminals && this.model.terminals.length > 0){
      this.userTerminals = this.model.terminals.map(t => t.code);
    }
  },
  methods: {
    getData() {
      return this.userTerminals;
    }
  }
};
</script>