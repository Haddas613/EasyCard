<template>
  <div>
    <v-checkbox
      v-model="model.roles"
      :label="$t(appConstants.users.roles.merchant)"
      :value="appConstants.users.roles.merchant"
      disabled
    ></v-checkbox>
    <v-checkbox
      v-model="model.roles"
      :label="$t(appConstants.users.roles.manager)"
      :value="appConstants.users.roles.manager"
    ></v-checkbox>
  </div>
</template>

<script>
import appConstants from "../../helpers/app-constants";

export default {
  props: {
    user: {
      type: Object,
      default: null,
      required: true
    }
  },
  data() {
    return {
      model: { ...this.user },
      appConstants: appConstants
    };
  },
  methods: {
    getData() {
      return this.model;
    }
  }
};
</script>