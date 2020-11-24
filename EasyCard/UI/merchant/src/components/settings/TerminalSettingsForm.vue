<template>
  <div>
    <v-row no-gutters>
      <v-col md="4" cols="12">
        <v-text-field
          v-model="model.label"
          :counter="50"
          :rules="[vr.primitives.maxLength(50)]"
          :label="$t('Label')"
          outlined
          required
        ></v-text-field>
      </v-col>
      <v-col md="4" cols="12">
        <v-text-field
          :value="$options.filters.ecdate(model.created ,'LLLL')"
          v-bind:class="{'px-1' : $vuetify.breakpoint.mdAndUp}"
          disabled
          :label="$t('Created')"
          outlined
          required
        ></v-text-field>
      </v-col>
      <v-col md="4" cols="12">{{model.status}}</v-col>
    </v-row>
    <v-row no-gutters>
      <v-col cols="12" class="subtitle-2 black--text pb-3">
        {{$t("General")}}
        <v-divider class="pt-1"></v-divider>
      </v-col>
      <v-col cols="12">
          <v-text-field
          v-model="model.settings.defaultItemName"
          :counter="128"
          :rules="[vr.primitives.maxLength(128)]"
          :label="$t('DefaultItemName')"
          outlined
          required
        ></v-text-field>
      </v-col>
      <v-col cols="12">
          <v-text-field
          v-model="model.settings.defaultChargeDescription"
          :counter="128"
          :rules="[vr.primitives.maxLength(128)]"
          :label="$t('DefaultChargeDescription')"
          outlined
          required
        ></v-text-field>
      </v-col>
      <v-col cols="12">
          <v-text-field
          v-model="model.settings.defaultRefundDescription"
          :counter="128"
          :rules="[vr.primitives.maxLength(128)]"
          :label="$t('DefaultRefundDescription')"
          outlined
          required
        ></v-text-field>
      </v-col>

      <v-col md="6" cols="12">
        <v-switch class="pt-0" v-model="model.settings.j5Allowed" :label="$t('J5Allowed')" hide-details></v-switch>
        <v-switch v-model="model.settings.j2Allowed" :label="$t('J2Allowed')" hide-details></v-switch>
        <v-switch
          v-model="model.settings.enableCancellationOfUntransmittedTransactions"
          :label="$t('EnableCancellationOfUntransmittedTransactions')"
          hide-details
        ></v-switch>
      </v-col>
      <v-col md="6" cols="12">
        <v-switch class="pt-0" v-model="model.settings.cvvRequired" :label="$t('CvvRequired')" hide-details></v-switch>
        <v-switch
          v-model="model.settings.nationalIDRequired"
          :label="$t('NationalIDRequired')"
          hide-details
        ></v-switch>
      </v-col>
      <v-col cols="12">
        <v-spacer class="py-4"></v-spacer>
      </v-col>
      <v-col md="6" cols="12">
        <v-text-field
          v-model="model.settings.maxInstallments"
          :label="$t('MaxInstallments')"
          type="number"
          :rules="[vr.primitives.required]"
          outlined
        ></v-text-field>
      </v-col>
      <v-col md="6" cols="12">
        <v-text-field
          v-bind:class="{'px-1' : $vuetify.breakpoint.mdAndUp}"
          v-model="model.settings.minInstallments"
          :label="$t('MinInstallments')"
          type="number"
          :rules="[vr.primitives.required]"
          outlined
        ></v-text-field>
      </v-col>
      <v-col md="6" cols="12">
        <v-text-field
          v-model="model.settings.maxCreditInstallments"
          :label="$t('MaxCreditInstallments')"
          type="number"
          :rules="[vr.primitives.required]"
          outlined
        ></v-text-field>
      </v-col>
      <v-col md="6" cols="12">
        <v-text-field
          v-bind:class="{'px-1' : $vuetify.breakpoint.mdAndUp}"
          v-model="model.settings.minCreditInstallments"
          :label="$t('MinCreditInstallments')"
          type="number"
          :rules="[vr.primitives.required]"
          outlined
        ></v-text-field>
      </v-col>
    </v-row>
  </div>
</template>

<script>
import ValidationRules from "../../helpers/validation-rules";

export default {
  props: {
    data: {
      type: Object,
      default: () => null,
      required: true
    }
  },
  data() {
    return {
      vr: ValidationRules,
      model: { ...this.data }
    };
  }
};
</script>