<template>
  <div>
    <p>{{$t("Steps")}}</p>
    <ec-list class="ecbg" v-if="model" :items="model" dashed stretch dense>
      <template v-slot:left="{ item, index }">
        <v-col cols="12" md="6" class="text-start text-oneline">
          <v-badge left color="primary" inline :content="index">{{$t(item.title)}}</v-badge>
        </v-col>
        <v-col cols="12" md="6" class="text-align-initial subtitle-2">
          <template v-if="item.required">
            <small class="red--text">*</small>
            <small>{{$t("Required")}}</small>
          </template>
        </v-col>
      </template>
      <template v-slot:append="{ item }">
        <v-col cols="12" class="py-0 my-0 text-end font-weight-bold subtitle-2">
          <v-switch :input-value="!item.skip" @change="skipStep(item, $event)" :disabled="item.required">
            <template v-slot:label>
              <small class="px-2">{{$t("Active")}}</small>
            </template>
          </v-switch>
        </v-col>
      </template>
    </ec-list>
    <v-flex class="d-flex justify-end pt-2">
      <v-btn
        color="primary"
        :block="$vuetify.breakpoint.smAndDown"
        @click="saveWizardSettings()"
      >{{$t('Save')}}</v-btn>
    </v-flex>
  </div>
</template>

<script>
import { mapState, mapMutations } from "vuex";

export default {
  name: "ChargeWizardSettings",
  components: {
    EcList: () => import("../ec/EcList")
  },
  data() {
    return {
      model: null
    };
  },
  computed: {
    ...mapState({
      chargeWizardStore: state => state.ui.chargeWizard
    })
  },
  mounted() {
    this.model = { ...this.chargeWizardStore.steps };
  },
  methods: {
    async saveWizardSettings() {
        await this.setChargeWizard({ steps: this.model });
        this.$toasted.show(this.$t("Saved"), { type: "success" });
    },
    skipStep(item, $event) {
      if(item.required){
          return;
      }
      item.skip = !$event;
    },
    ...mapMutations({
        setChargeWizard: 'ui/setChargeWizard'
    })
  }
};
</script>

<style lang="scss" scoped>
</style>