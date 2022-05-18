<template>
  <v-form ref="scheduleFormRef" v-model="valid" lazy-validation>
    <v-row no-gutters>
      <v-col cols="12" md="12" class="px-1">
        <v-select
          :items="dictionaries.repeatPeriodTypeEnum"
          item-text="description"
          item-value="code"
          v-model="model.repeatPeriodType"
          outlined
          :label="$t('RepeatPeriodType')"
          :rules="[vr.primitives.required]"
        ></v-select>
      </v-col>

      <v-col cols="12" md="6" class="px-1">
        <v-select
          :items="dictionaries.startAtTypeEnum"
          item-text="description"
          item-value="code"
          v-model="model.startAtType"
          outlined
          :label="$t('StartAtType')"
          v-on:change="model.startAt = null"
        ></v-select>
      </v-col>
      <v-col cols="12" md="6" class="px-1">
        <ec-date-input v-model="model.startAt" :min="minDate" :label="$t('StartAt')" />
      </v-col>

      <v-col cols="12" md="6" class="px-1">
        <v-select
          :items="dictionaries.endAtTypeEnum"
          item-text="description"
          item-value="code"
          v-model="model.endAtType"
          outlined
          :label="$t('EndAtType')"
          v-on:change="model.endAt = null; model.endAtNumberOfPayments = null"
        ></v-select>
      </v-col>
      <v-col cols="12" md="6" class="px-1">
        <ec-date-input v-if="model.endAtType == 'specifiedDate'" v-model="model.endAt" :min="model.startAt || minDate" :label="$t('EndAt')" />
        <v-text-field
          v-else-if="model.endAtType == 'afterNumberOfPayments'"
          v-model.number="model.endAtNumberOfPayments"
          :label="$t('EndAtNumberOfPayments')"
          type="number"
          outlined
        ></v-text-field>
      </v-col>
    </v-row>
  </v-form>
</template>

<script>
import ValidationRules from "../../helpers/validation-rules";
export default {
  components: {
    EcDateInput: () => import("../../components/inputs/EcDateInput"),
  },
  props: {
    data: {
      type: Object,
      required: true
    }
  },
  data() {
    return {
      model: {
        startAt:null,
        endAt: null,
        ...this.data
      },
      valid: true,
      vr: ValidationRules,
      dictionaries: {},
      minDate: new Date().toISOString(),
    };
  },
  async mounted() {
    this.dictionaries = await this.$api.dictionaries.getTransactionDictionaries();
  },
  methods: {
    validate() {
      return this.$refs.scheduleFormRef.validate();
    },
    checkDates(){
      if (this.model.startAt > this.model.endAt){
        this.model.endAt = null;
      }
    }
  }
};
</script>