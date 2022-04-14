<template>
  <div>
    <ec-list
      v-if="model.webHooksConfiguration.webHooks.length"
      :items="model.webHooksConfiguration.webHooks"
      dashed
      dense
    >
      <template v-slot:left="{ item }">
        <v-col cols="12" md="12" class="px-2 text-start text-oneline">
          <span class="body-1">{{ item.eventName }}</span>
        </v-col>
      </template>
      <template v-slot:right="{ item }">
        <v-col cols="12" md="12 " class="text-start text-oneline" :title="item.url">
          <span class="primary--text text-caption" @click="$copyToClipboard(item.url)">
            <v-icon class="px-1" color="primary" small>mdi-content-copy</v-icon>
            {{item.url}}
          </span>
        </v-col>
      </template>
    </ec-list>
    <p v-else>{{ $t('NothingToShow') }}</p>
  </div>
</template>

<script>
import ValidationRules from '../../helpers/validation-rules';
export default {
  components: {
    EcList: () => import('@/components/ec/EcList.vue'),
  },
  props: {
    terminal: {
      type: Object,
      required: true,
    },
  },
  data() {
    return {
      model: { ...this.terminal },
      valid: false,
      vr: ValidationRules,
    };
  },
};
</script>
