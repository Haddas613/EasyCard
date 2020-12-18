<template>
  <v-form v-model="formValid" lazy-validation>
    <v-row v-if="model.settings">
      <v-col cols="12" md="4" class="py-0">
        <v-text-field v-model="model.settings.keyStorePassword" :label="$t('KeyStorePassword')" outlined></v-text-field>
      </v-col>
      <v-col cols="12" md="4" class="py-0">
        <v-text-field v-model="model.settings.userName" :label="$t('UserName')" outlined></v-text-field>
      </v-col>
      <v-col cols="12" md="4" class="py-0">
        <v-text-field v-model="model.settings.password" :label="$t('Password')" outlined></v-text-field>
      </v-col>
    </v-row>
    <div class="d-flex justify-end">
      <v-btn color="primary" @click="save()" :loading="loading">{{$t("Save")}}</v-btn>
    </div>
  </v-form>
</template>

<script>
export default {
  props: {
    data: {
      type: Object,
      required: true
    },
    terminalId: {
      type: String,
      required: true
    }
  },
  data() {
    return {
      model: {
        ...this.data
      },
      formValid: false,
      loading: false
    }
  },
  mounted () {
    if(!this.model.settings){
      this.model.settings = {};
    }
  },
  methods: {
    save() {
      if(!this.formValid){
        return;
      }
      this.loading = true;
      this.$api.terminals.saveTerminalExternalSystem(this.terminalId, this.model);
      this.loading = false;
    }
  },
};
</script>