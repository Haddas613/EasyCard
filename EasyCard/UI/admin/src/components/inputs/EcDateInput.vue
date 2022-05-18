<template>
  <v-menu
    ref="menu"
    v-model="menu"
    :close-on-content-click="true"
    transition="scale-transition"
    offset-y
    min-width="auto"
  >
    <template v-slot:activator="{ on, attrs }">
      <v-text-field
        :value="$options.filters.ecdate(model, 'd')"
        :label="inputText"
        readonly
        v-bind="attrs"
        v-on="on"
        hide-details="auto"
      ></v-text-field>
    </template>
    <v-date-picker v-model="model" no-title scrollable :min="min" :max="max" @change="onChange($event)">
      <v-spacer></v-spacer>
      <v-btn text color="error" @click="cancel()">{{ $t("Clear") }}</v-btn>
    </v-date-picker>
  </v-menu>
</template>

<script>
export default {
  model: {
    prop: "data",
    event: "change",
  },
  props: {
    data: {
      required: true,
    },
    label: {
      type: String,
    },
    min: {
      default: null,
    },
    max: {
      default: null,
    },
  },
  data() {
    return {
      model: this.data,
      inputText: this.label || this.$t("SelectDate"),
      menu: false,
    };
  },
  methods: {
    save() {
      this.$refs.menu.save(this.model);
      this.$emit("ok", this.model);
      this.$emit("change", this.model);
    },
    cancel() {
      this.menu = false;
      this.model = null;
      this.save();
    },
    onChange(data){
      this.model = data;
      this.save();
    },
  },
};
</script>