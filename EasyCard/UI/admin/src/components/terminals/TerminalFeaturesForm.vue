<template>
  <div v-if="features">
    <ec-list v-if="features && features.length > 0" :items="features" dashed>
      <template v-slot:left="{ item }">
        <v-col cols="12" md="6" class="px-2 text-start text-oneline">
          <span class="body-1">{{$vuetify.lang.current == 'he' ? item.nameHE : item.nameEN}}</span>
        </v-col>
        <v-col cols="12" md="6" class="text-align-initial font-weight-bold subtitle-2">
          <small>{{$vuetify.lang.current == 'he' ? item.descriptionHE : item.descriptionEN}}</small>
        </v-col>
      </template>
      <template v-slot:append="{ item }">
        <v-col cols="12" class="pt-2 px-4 text-end font-weight-bold subtitle-2">
          <v-switch v-model="item.enabled" @change="switchFeature(item)" :disabled="item.disabled">
            <template v-slot:label>
              <span class="px-2">{{$t('Active')}}</span>
            </template>
          </v-switch>
        </v-col>
      </template>
    </ec-list>
  </div>
</template>

<script>
import ValidationRules from "../../helpers/validation-rules";
export default {
  components: {
    EcList: () => import("../ec/EcList")
  },
  props: {
    terminal: {
      type: Object,
      required: true
    },
    isTemplate: {
      type: Boolean,
      default: false,
      required: false
    }
  },
  data() {
    return {
      model: { ...this.terminal },
      features: null,
      valid: false,
      vr: ValidationRules,
      idKey: "terminalID",
      //api key for features to use ($api['terminals'] for example)
      apiName: this.isTemplate ? "terminalTemplates" : "terminals"
    };
  },
  async mounted() {
    let features = await this.$api.terminals.getAvailableFeatures({
      showForTemplatesOnly: this.isTemplate
    });
    if (this.terminal.enabledFeatures && this.terminal.enabledFeatures.length > 0) {
      for (var f of features) {
        if (this.terminal.enabledFeatures.indexOf(f.featureID) > -1) {
          f.enabled = true;
        }
      }
    }
    if (this.apiName == "terminalTemplates") {
      this.idKey = "terminalTemplateID";
    }
    this.features = features;
  },
  methods: {
    async switchFeature(feature) {
      if(feature.enabled && feature.featureID == 'Checkout'){
        let f = this.lodash.find(this.features, feature => feature.featureID == 'Api');
        this.$set(f, 'enabled', true);
      }
      this.$set(feature, 'disabled', true);
      await this.$api[this.apiName].switchTerminalFeature(
        this.terminal[this.idKey],
        feature.featureID
      );
      this.$set(feature, 'disabled', false);
      this.$emit('update');
    }
  }
};
</script>