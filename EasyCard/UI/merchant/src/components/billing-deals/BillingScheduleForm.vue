<template>
  <v-form ref="scheduleFormRef" v-model="valid" lazy-validation>
    <v-row no-gutters>
      <v-col cols="12" md="6" class="px-1">
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
        <v-text-field
          v-model.number="model.repeatPeriod"
          :label="$t('RepeatPeriod')"
          :rules="[vr.primitives.requiredDepends(model.repeatPeriodType)]"
          type="number"
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
        <v-menu
          ref="startAtMenu"
          v-model="startAtMenu"
          :close-on-content-click="false"
          :return-value.sync="model.startAt"
          v-if="model.startAtType == 'SpecifiedDate'"
          offset-y
          min-width="290px"
        >
          <template v-slot:activator="{ on }">
            <v-text-field
              v-model="model.startAt"
              :label="$t('StartAt')"
              readonly
              outlined
              v-on="on"
            ></v-text-field>
          </template>
          <v-date-picker v-model="model.startAt" :min="minDate" no-title scrollable>
            <v-spacer></v-spacer>
            <v-btn text color="primary" @click="$refs.startAtMenu.save(model.startAt)">{{$t("Ok")}}</v-btn>
          </v-date-picker>
        </v-menu>
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
        <v-menu
          ref="endAtMenu"
          v-model="endAtMenu"
          :close-on-content-click="false"
          :return-value.sync="model.endAt"
          v-if="model.endAtType == 'SpecifiedDate'"
          offset-y
          min-width="290px"
        >
          <template v-slot:activator="{ on }">
            <v-text-field
              v-model="model.endAt"
              :label="$t('EndAt')"
              readonly
              outlined
              v-on="on"
            ></v-text-field>
          </template>
          <v-date-picker v-model="model.endAt" :min="model.startAt || minDate" no-title scrollable>
            <v-spacer></v-spacer>
            <v-btn text color="primary" @click="$refs.endAtMenu.save(model.endAt)">{{$t("Ok")}}</v-btn>
          </v-date-picker>
        </v-menu>
        <v-text-field
          v-else
          v-model.number="model.endAt"
          :label="$t('EndAt')"
          :rules="[vr.primitives.requiredDepends((model.endAtType == 'Never' ? false : model.endAtType))]"
          :disabled="!model.endAtType"
          type="number"
          outlined
        ></v-text-field>
      </v-col>
      <v-col cols="12" md="6" class="px-1">
        <v-text-field
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
      valid: true,
      vr: ValidationRules,
      dictionaries: {},
      minDate: new Date().toISOString(),
      startAtMenu: false,
      endAtMenu: false
    };
  },
  async mounted() {
    this.dictionaries = await this.$api.dictionaries.getTransactionDictionaries();
  },
  methods: {
    validate() {
      return this.$refs.scheduleFormRef.validate();
    }
  }
};
</script>