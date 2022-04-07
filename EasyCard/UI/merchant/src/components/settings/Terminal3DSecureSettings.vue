<template>
  <div>
    <ec-dialog :dialog.sync="showConsentDialog">
      <template v-slot:title>{{ $t('Enable3DSecure') }}</template>
      <template>
        <v-row>
          <v-col cols="12">
            <p>{{ consentMsg }}</p>
          </v-col>
          <v-col cols="12" class="d-flex justify-end" v-if="!$vuetify.breakpoint.smAndDown">
            <v-btn class="mx-1" color="error" @click="visible = false">{{ $t('Reject') }}</v-btn>
            <v-btn color="success" @click="confirm($t('AgreeAndProceed'))">{{
              $t('AgreeAndProceed')
            }}</v-btn>
          </v-col>
          <v-col class="px-2" cols="12" v-if="$vuetify.breakpoint.smAndDown">
            <v-btn block color="error" @click="visible = false">{{ $t('Reject') }}</v-btn>
            <v-spacer class="py-2"></v-spacer>
            <v-btn block color="success" @click="confirm($t('AgreeAndProceed'))">{{
              $t('AgreeAndProceed')
            }}</v-btn>
          </v-col>
        </v-row>
      </template>
    </ec-dialog>
    <v-row v-if="model" :key="model.support3DSecure">
      <!-- <v-col cols="12" class="subtitle-2 black--text pb-3">
        {{ $t('3DSecure') }}
        <v-divider class="pt-1"></v-divider>
      </v-col>
      {{ model }}
      <v-col cols="12" md="7">
        {{ model.support3DSecure ? $t('3DSecureEnabledMessage') : $t('3DSecureEnabledMessage') }}
      </v-col>
      <v-col cols="12" md="5" class="d-flex justify-end">
        <v-btn v-if="model.support3DSecure" @click="disable3DS()" color="error" outlined>{{
          $t('Disable')
        }}</v-btn>
        <v-btn v-else @click="showDialog()" color="success" outlined>{{ $t('Enable') }}</v-btn>
      </v-col> -->
      <template v-if="model.support3DSecure">
        <v-col cols="12" md="7" class="success--text">
          {{ $t('3DSecureEnabledMessage') }}
        </v-col>
        <v-col cols="12" md="5" class="d-flex justify-end">
          <v-btn @click="disable3DS()" color="error" outlined>{{ $t('Disable') }}</v-btn>
        </v-col>
      </template>
      <template v-else>
        <v-col cols="12" md="7">
          {{ $t('3DSecureNotEnabledMessage') }}
        </v-col>
        <v-col cols="12" md="5" class="d-flex justify-end">
          <v-btn @click="showDialog()" color="success">{{ $t('Enable') }}</v-btn>
        </v-col>
      </template>
    </v-row>
  </div>
</template>

<script>
export default {
  components: {
    EcDialog: () => import('../ec/EcDialog.vue'),
  },
  props: {
    terminal: {
      type: Object,
      default: null,
      required: true,
    },
  },
  data() {
    return {
      model: { ...this.terminal },
      showConsentDialog: false,
      consentMsg: null,
      loading: false,
    };
  },
  methods: {
    async showDialog() {
      if (this.loading) {
        return;
      }
      this.loading = true;
      let operationResult = await this.$api.terminals.get3DSConsentMessage();

      if (!this.$apiSuccess(operationResult)) {
        this.$toasted.show(operationResult.message || this.$t('Error'), { type: 'error' });
      } else {
        this.consentMsg = operationResult.message;
        this.showConsentDialog = true;
      }

      this.loading = false;
    },
    async confirm(buttonText) {
      if (this.loading) {
        return;
      }
      this.loading = true;

      var operationResult = await this.$api.terminals.enable3DS({
        terminalID: this.model.terminalID,
        consentAgreeText: buttonText,
      });

      if (!this.$apiSuccess(operationResult)) {
        this.$toasted.show(operationResult.message || this.$t('Error'), { type: 'error' });
      } else {
        this.model.support3DSecure = true;
      }

      this.showConsentDialog = false;
      this.loading = false;
    },
    async disable3DS() {
      if (this.loading) {
        return;
      }
      this.loading = true;
      var operationResult = await this.$api.terminals.disable3DS(this.model.terminalID);

      if (!this.$apiSuccess(operationResult)) {
        this.$toasted.show(operationResult.message || this.$t('Error'), { type: 'error' });
      } else {
        this.model.support3DSecure = null;
      }

      this.loading = false;
    },
  },
};
</script>

<style lang="scss" scoped></style>
