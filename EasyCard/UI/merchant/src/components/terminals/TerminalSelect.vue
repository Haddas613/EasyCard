<template>
  <div>
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
      terminalID: null,
      terminals: []
    };
  },
  async mounted() {
    this.terminals = (await this.$api.terminals.getTerminals()).data || [];
    if (this.terminals.length > 0) {
      if (this.terminals.length === 1) {
        this.$emit("ok", this.terminals[0].terminalID);
      }
      this.terminalID = this.terminals[0].terminalID;
    }
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