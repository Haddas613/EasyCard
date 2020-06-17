<template>
  <div>
    <v-app-bar class="ec-toolbar" absolute hide-on-scroll flat color="white" height="56px">
      <v-col cols="2" class="d-flex px-0 justify-start" v-if="completed">
        <v-spacer></v-spacer>
      </v-col>
      <v-col cols="2" class="d-flex justify-start" v-if="!completed">
        <v-btn icon color="primary" @click="onClickBack()">
          <v-icon size="2rem" left>mdi-chevron-left</v-icon>
        </v-btn>
      </v-col>
      <v-col class="d-flex justify-space-around">
        <v-toolbar-title class="subtitle-2 font-weight-bold">{{title}}</v-toolbar-title>
      </v-col>
      <v-col cols="2" class="d-flex px-0 justify-end" v-if="skippable && !completed" @click="onClickSkip()">
        <!-- <v-btn color="primary" icon>
          <v-icon size="2rem">mdi-debug-step-over</v-icon>
        </v-btn> -->
        <v-btn color="primary" class="text-none">{{$t('Skip')}}</v-btn>
      </v-col>
      <v-col cols="2" class="d-flex px-0 justify-end" v-if="closeable || completed" @click="onClickClose()">
        <v-btn color="primary" icon>
          <v-icon icon size="2rem" color="primary">mdi-close</v-icon>
        </v-btn>
      </v-col>
      <v-col cols="2" class="d-flex px-0 justify-end" v-if="!skippable && (!closeable && !completed)">
        <v-spacer></v-spacer>
      </v-col>
    </v-app-bar>
    <!-- spacer that will take app-bar space -->
    <div class="navbar-space py-7"></div>
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
    },
    completed: {
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
.navbar-space{
  border-bottom: 1px solid var(--v-ecbg-darken1);
}
</style>