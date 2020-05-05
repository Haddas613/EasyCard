<template>
  <v-app-bar :clipped-left="$vuetify.breakpoint.lgAndUp" app color="primary" dark>
    <v-app-bar-nav-icon @click.stop="drawerObj = !drawerObj" />
    <v-toolbar-title style="width: 300px" class="ml-0 pl-4">
      <span class="hidden-sm-and-down">Easy Card</span>
    </v-toolbar-title>
    <v-spacer></v-spacer>
    <v-toolbar-items>
      <v-switch class="pt-5" :flat="true" :color="'light-blue lighten-5'" label="RTL" v-model="$vuetify.rtl"></v-switch>
    </v-toolbar-items>
  </v-app-bar>
</template>
<script>
export default {
  name: "MainAppBar",
  props: ["drawer"],
  computed: {
    drawerObj: {
      get: function() {
        return this.drawer;
      },
      set: function(nv) {
        this.$emit("update:drawer", nv);
      }
    }
  }
};
</script>