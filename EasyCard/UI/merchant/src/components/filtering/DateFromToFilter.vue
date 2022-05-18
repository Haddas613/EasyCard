<template>
  <v-row class="d-flex">
    <v-col cols="12" :md="md">
      <ec-date-input v-model="dateFromRaw" :min="fromToday ? today : null" :label="$t(dateFromLabel)" />
    </v-col>
    <v-col cols="12" :md="md">
      <ec-date-input v-model="dateToRaw" :min="dateFromRaw ? dateFromRaw : null" :label="$t(dateToLabel)" />
    </v-col>
  </v-row>
</template>

<script>
import ValidationRules from "../../helpers/validation-rules";

export default {
  components: {
    EcDateInput: () => import("../inputs/EcDateInput"),
  },
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
    },
    dateFromLabel: {
      type: String,
      default: 'DateFrom'
    },
    dateToLabel: {
      type: String,
      default: 'DateTo'
    }
  },
  data() {
    return {
      vr: ValidationRules,
      dateFromMenu: false,
      dateToMenu: false,
      dateFromRaw: null,
      dateToRaw: null,
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