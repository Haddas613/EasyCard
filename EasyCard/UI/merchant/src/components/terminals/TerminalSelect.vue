<template>
  <div>
    <v-overlay :value="loading">
      <v-progress-circular indeterminate size="64"></v-progress-circular>
    </v-overlay>
    <v-form ref="form" lazy-validation>
      <v-container fluid>
        <v-row>
          <v-col>
            <v-select
              :items="terminals"
              item-text="label"
              item-value="terminalID"
              v-model="terminalID"
              solo
              :label="$t('Terminal')"
              required
            ></v-select>
          </v-col>
        </v-row>
      </v-container>
    </v-form>
    <v-btn
      color="primary"
      bottom
      :x-large="true"
      :fixed="$vuetify.breakpoint.mdAndDown"
      block
      @click="ok()"
    >{{$t('Ok')}}</v-btn>
  </div>
</template>

<script>
export default {
  data() {
    return {
      loading: false,
      terminalID: null,
      terminals: []
    };
  },
  async mounted() {
    let timeout = setTimeout((() => {this.loading = true}).bind(this), 1000);
    
    this.terminals = (await this.$api.terminals.getTerminals()) || [];
    if (this.terminals.length > 0) {
      if (this.terminals.length === 1) {
        this.$emit("ok", this.terminals[0].terminalID);
      }
      this.terminalID = this.terminals[0].terminalID;
    }
    clearTimeout(timeout);
    this.loading = false;
  },
  methods: {
    ok() {
      this.$emit("ok", this.terminalID);
    }
  }
};
</script>

<style lang="scss" scoped>
</style>