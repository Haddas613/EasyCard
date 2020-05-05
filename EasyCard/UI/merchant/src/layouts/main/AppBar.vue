<template>
  <v-app-bar
    :clipped-left="$vuetify.breakpoint.lgAndUp"
    :clipped-right="$vuetify.breakpoint.lgAndUp"
    app
    color="primary"
    dark
    flat="true"
  >
    <v-row :align="'center'">
      <v-col cols="2">
        <v-app-bar-nav-icon @click.stop="drawerObj = !drawerObj" />
      </v-col>
      <v-col class="d-flex justify-space-around">
        <v-toolbar-title class="ml-0 pl-4">
          easy card
        </v-toolbar-title>
      </v-col>
      <v-col cols="2" class="d-flex justify-end">
        <v-toolbar-items>
          <v-switch
            class="pt-5"
            :flat="true"
            :color="'accent'"
            label="RTL"
            v-model="$vuetify.rtl"
          ></v-switch>
        </v-toolbar-items>
      </v-col>
    </v-row>
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