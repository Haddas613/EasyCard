<template>
  <v-row class="px-3">
    <v-col cols="12" md="6">
      <v-menu
        ref="dateFromMenu"
        v-model="dateFromMenu"
        :close-on-content-click="false"
        :return-value.sync="dateFromRaw"
        offset-y
        min-width="290px"
      >
        <template v-slot:activator="{ on }">
          <v-text-field v-model="dateFromRaw" :label="$t('DateFrom')" readonly v-on="on" clearable></v-text-field>
        </template>
        <v-date-picker v-model="dateFromRaw" no-title scrollable>
          <v-spacer></v-spacer>
          <v-btn text color="primary" @click="$refs.dateFromMenu.save(dateFromRaw)">{{$t("Ok")}}</v-btn>
        </v-date-picker>
      </v-menu>
    </v-col>
    <v-col cols="12" md="6">
      <v-menu
        ref="dateToMenu"
        v-model="dateToMenu"
        :close-on-content-click="false"
        :return-value.sync="dateToRaw"
        offset-y
        min-width="290px"
      >
        <template v-slot:activator="{ on }">
          <v-text-field v-model="dateToRaw" :label="$t('DateTo')" readonly v-on="on" clearable></v-text-field>
        </template>
        <v-date-picker v-model="dateToRaw" no-title scrollable>
          <v-spacer></v-spacer>
          <v-btn text color="primary" @click="$refs.dateToMenu.save(dateToRaw)">{{$t("Ok")}}</v-btn>
        </v-date-picker>
      </v-menu>
    </v-col>
  </v-row>
</template>

<script>
import ValidationRules from "../../helpers/validation-rules";

export default {
  model: {
    prop: "data",
    event: "change"
  },
  props: {
    data: {
      type: Object,
      default: null,
      required: true
    }
  },
  data() {
    return {
      vr: ValidationRules,
      dateFromMenu: false,
      dateToMenu: false,
      dateFromRaw: this.data.dateFrom,
      dateToRaw: this.data.dateTo
    };
  },
  watch: {
    dateFromRaw: function(val) {
      this.data.dateFrom = this.$formatDate(this.dateFromRaw);
      this.$emit("change", this.data);
    },
    dateToRaw: function(val) {
      this.data.dateTo = this.$formatDate(this.dateToRaw);
      this.$emit("change", this.data);
    }
  }
};
</script>