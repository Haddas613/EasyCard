<template>
  <v-row no-gutters>
    <v-col cols="12" md="6" class="px-1">
      <v-select
        :items="dictionaries.repeatPeriodTypeEnum"
        item-text="description"
        item-value="code"
        v-model="model.repeatPeriodType"
        outlined
        :label="$t('RepeatPeriodType')"
        required
      ></v-select>
    </v-col>
    <v-col cols="12" md="6" class="px-1">
      <v-text-field
        v-model.number="model.repeatPeriod"
        :label="$t('RepeatPeriod')"
        :rules="[vr.primitives.required]"
        type="number"
        required
        :disabled="!model.repeatPeriodType"
        outlined
      ></v-text-field>
    </v-col>

    <v-col cols="12" md="6" class="px-1">
      <v-select
        :items="dictionaries.startAtTypeEnum"
        item-text="description"
        item-value="code"
        v-model="model.startAtType"
        outlined
        :label="$t('StartAtType')"
      ></v-select>
    </v-col>
    <v-col cols="12" md="6" class="px-1">
      <v-text-field
        v-model.number="model.startAt"
        :label="$t('StartAt')"
        :rules="[vr.primitives.required]"
        :disabled="!model.startAtType"
        type="number"
        outlined
      ></v-text-field>
    </v-col>

    <v-col cols="12" md="6" class="px-1">
      <v-select
        :items="dictionaries.endAtTypeEnum"
        item-text="description"
        item-value="code"
        v-model="model.endAtType"
        outlined
        :label="$t('EndAtType')"
      ></v-select>
    </v-col>
    <v-col cols="12" md="6" class="px-1">
      <v-text-field
        v-model.number="model.endAt"
        :label="$t('EndAt')"
        :rules="[vr.primitives.required]"
        :disabled="!model.endAtType"
        type="number"
        outlined
      ></v-text-field>
    </v-col>
    <v-col cols="12" md="6" class="px-1">
      <v-text-field
        v-model.number="model.endAtNumberOfPayments"
        :label="$t('EndAtNumberOfPayments')"
        :rules="[vr.primitives.required]"
        type="number"
        outlined
      ></v-text-field>
    </v-col>
  </v-row>
</template>

<script>
import ValidationRules from '../../helpers/validation-rules';
export default {
  props: {
    data: {
      type: Object,
      required: true
    }
  },
  data() {
    return {
      model: {
          ...this.data
      },
      vr: ValidationRules,
      dictionaries: {}
    };
  },
  async mounted(){
    this.dictionaries = await this.$api.dictionaries.getTransactionDictionaries();
  }
};
</script>

<style lang="scss" scoped>
</style>