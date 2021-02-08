<template>
  <v-menu
    ref="menu"
    v-model="menu"
    :close-on-content-click="false"
    :return-value.sync="model"
    transition="scale-transition"
    offset-y
    min-width="auto"
  >
    <template v-slot:activator="{ on, attrs }">
      <v-text-field
        v-model="model"
        :label="inputText"
        readonly
        v-bind="attrs"
        v-on="on"
        outlined
        hide-details="auto"
      ></v-text-field>
    </template>
    <v-date-picker v-model="model" no-title scrollable>
      <v-spacer></v-spacer>
      <v-btn text color="primary" @click="cancel()">{{$t("Clear")}}</v-btn>
      <v-btn text color="primary" @click="save(model)">{{$t("OK")}}</v-btn>
    </v-date-picker>
  </v-menu>
</template>

<script>
export default {
  model: {
    prop: 'data',
    event: 'change'
  },
  props: {
    data: {
      required: true
    },
    label: {
      type: String
    }
  },
  data() {
    return {
      model: this.data,
      inputText: this.label || this.$t("SelectDate"),
      menu: false
    };
  },
  methods: {
    save() {
      this.$refs.menu.save(this.model);
      this.$emit("ok", this.model);
      this.$emit("change", this.model);
    },
    cancel(){
        this.menu = false;
        this.model = null;
        this.save();
    }
  }
};
</script>