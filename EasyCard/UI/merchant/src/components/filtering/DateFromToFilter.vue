<template>
  <v-row>
    <v-col cols="12" :md="md">
      <v-menu
        ref="dateFromMenu"
        v-model="dateFromMenu"
        :close-on-content-click="false"
        :return-value.sync="dateFromRaw"
        offset-y
        min-width="290px"
      >
        <template v-slot:activator="{ on }">
          <v-text-field v-model="dateFromRaw" :label="$t('DateFrom')" readonly v-on="on" clearable outlined :rules="rules"></v-text-field>
        </template>
        <v-date-picker v-model="dateFromRaw" no-title scrollable color="primary" :min="fromToday ? today : false">
          <v-spacer></v-spacer>
          <v-btn text color="primary" @click="$refs.dateFromMenu.save(dateFromRaw)">{{$t("Ok")}}</v-btn>
        </v-date-picker>
      </v-menu>
    </v-col>
    <v-col cols="12" :md="md">
      <v-menu
        ref="dateToMenu"
        v-model="dateToMenu"
        :close-on-content-click="false"
        :return-value.sync="dateToRaw"
        offset-y
        min-width="290px"
      >
        <template v-slot:activator="{ on }">
          <v-text-field v-model="dateToRaw" :label="$t('DateTo')" readonly v-on="on" clearable outlined :rules="rules"></v-text-field>
        </template>
        <v-date-picker v-model="dateToRaw" no-title scrollable color="primary" :min="dateFromRaw ? dateFromRaw : null">
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
    },
    md: {
      type: String,
      default: "6"
    },
    required: {
      type: Boolean,
      default: false
    },
    fromToday: {
      type: Boolean,
      default: false
    }
  },
  data() {
    return {
      vr: ValidationRules,
      dateFromMenu: false,
      dateToMenu: false,
      dateFromRaw: null,
      dateToRaw: null,
      rules: [],
      today: new Date().toISOString()
    };
  },
  mounted () {
    if (this.required){
      this.rules = [this.vr.primitives.required]
    }
    this.dateFromRaw = this.data.dateFrom ? this.$formatDate(this.data.dateFrom) : null;
    this.dateToRaw = this.data.dateTo ? this.$formatDate(this.data.dateTo) : null;
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