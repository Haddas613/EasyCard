<template>
  <div v-if="integrations">
    <ec-list class="ecbg" v-if="integrations && integrations.length > 0" :items="integrations" dashed stretch dense>
      <template v-slot:left="{ item }">
        <v-col cols="12" md="6" class="text-start text-oneline">
          <span class="body-1">{{$t(item.type)}}</span>
        </v-col>
        <v-col cols="12" md="6" class="text-align-initial subtitle-2">
          <small>{{item.name}}</small>
        </v-col>
      </template>
      <!-- <template v-slot:append="{ item }">
        <v-col cols="12" class="pt-2 text-end font-weight-bold subtitle-2">
          <v-switch v-model="item.enabled" disabled></v-switch>
        </v-col>
      </template> -->
    </ec-list>
    <p class="text-center" v-else>
      {{$t("NothingToShow")}}
    </p>
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
      integrations: null,
    };
  },
  async mounted() {
    // let integrations = await this.$api.terminals.getAvailableFeatures({
    //   showForTemplatesOnly: this.isTemplate
    // });
    // if (this.terminal.enabledFeatures && this.terminal.enabledFeatures.length > 0) {
    //   for (var f of integrations) {
    //     if (this.terminal.enabledFeatures.indexOf(f.featureID) > -1) {
    //       f.enabled = true;
    //     }
    //   }
    // }
    this.integrations = this.lodash.map(this.terminal.integrations, (v, k) => ({type: k, name: v}))
  }
};
</script>