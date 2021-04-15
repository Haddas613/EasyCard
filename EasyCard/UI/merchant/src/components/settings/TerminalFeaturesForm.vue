<template>
  <div v-if="features">
    <ec-list class="ecbg" v-if="features && features.length > 0" :items="features" dashed stretch>
      <template v-slot:left="{ item }">
        <v-col cols="12" md="6" class="text-start text-oneline">
          <span class="body-1">{{item.nameHE}}</span>
        </v-col>
        <v-col cols="12" md="6" class="text-align-initial font-weight-bold subtitle-2">
          <span class="px-2">{{item.price | currency('ILS')}}</span>
        </v-col>
      </template>
      <template v-slot:append="{ item }">
        <v-col cols="12" class="pt-2 text-end font-weight-bold subtitle-2">
          <v-switch v-model="item.enabled" disabled></v-switch>
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
    }
  },
  data() {
    return {
      features: null,
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
    this.features = features;
  }
};
</script>