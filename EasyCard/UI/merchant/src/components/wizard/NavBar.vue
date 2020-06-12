<template>
  <div>
    <v-app-bar class="ec-toolbar" absolute hide-on-scroll flat color="white">
      <v-col class="d-flex justify-start">
        <v-btn icon color="primary" @click="onClickBack()">
          <v-icon size="2rem" left>mdi-chevron-left</v-icon>
        </v-btn>
      </v-col>
      <v-col class="d-flex justify-space-around">
        <v-toolbar-title class="subtitle-1">{{title}}</v-toolbar-title>
      </v-col>
      <v-col class="d-flex px-0 justify-end" v-if="skippable" @click="onClickSkip()">
        <!-- <v-btn color="primary" icon>
          <v-icon size="2rem">mdi-debug-step-over</v-icon>
        </v-btn> -->
        <v-btn color="primary" class="text-none">{{$t('Skip')}}</v-btn>
      </v-col>
      <v-col class="d-flex px-0 justify-end" v-if="closeable" @click="onClickClose()">
        <v-btn color="primary" icon>
          <v-icon icon size="2rem" color="primary">mdi-close</v-icon>
        </v-btn>
      </v-col>
      <v-col class="d-flex px-0 justify-end" v-if="!skippable && !closeable">
        <v-spacer></v-spacer>
      </v-col>
    </v-app-bar>
    <!-- spacer that will take app-bar space -->
    <div class="py-7"></div>
  </div>
</template>

<script>
export default {
  props: {
    title: {
      type: String,
      default: null
    },
    skippable: {
      type: Boolean,
      default: false
    },
    closeable: {
      type: Boolean,
      default: false
    }
  },
  methods: {
    onClickBack() {
      this.$emit("back");
    },
    onClickSkip() {
      this.$emit("skip");
    },
    onClickClose() {
      this.$emit("close");
    }
  }
};
</script>

<style lang="scss" scoped>
.v-toolbar__content {
  padding-left: 0 !important;
}
</style>