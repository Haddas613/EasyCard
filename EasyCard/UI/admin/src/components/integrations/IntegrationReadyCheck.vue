<template>
  <v-row class="info-container">
    <v-col cols="12" md="6" class="info-block">
      <p class="caption ecgray--text text--darken-2">{{ $t("ReadyToUse") }}</p>
      <h3
        class="mx-1"
        v-bind:class="{
          'success--text': integration.valid,
          'error--text': !integration.valid,
        }"
      >
        {{ integration.valid ? $t("Yes") : $t("No") }}
      </h3>
    </v-col>
    <v-col cols="12" md="6" class="d-flex justify-end">
      <v-btn small color="success" @click="$emit('test')">{{
        $t("TestConnection")
      }}</v-btn>
    </v-col>
  </v-row>
</template>

<script>
export default {
  props: {
    integration: {
      type: Object,
      required: true,
    },
  },
};
</script>

<style lang="scss" scoped>
</style>