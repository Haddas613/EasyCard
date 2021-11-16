<template>
  <v-dialog v-model="dialog" fullscreen persistent>
    <v-card flat :color="color">
      <v-card-title class="headline py-0 white">
        <v-row :align="'center'">
          <v-col class="d-flex justify-start px-1" cols="2">
            <v-btn icon @click="onDismiss()">
              <re-icon class="px-2">mdi-arrow-left</re-icon>
            </v-btn>
          </v-col>
          <v-col cols="8" lg="8" class="d-flex justify-space-around px-0 text-subtitle-1 text-md-h6 text-lg-h6">
            <div class="text-truncate">
              <slot name="title"></slot>
            </div>
          </v-col>
          <v-col cols="2" lg="2" xl="2" class="d-flex justify-end">
            <slot name="right"></slot>
          </v-col>
        </v-row>
      </v-card-title>
      <v-divider></v-divider>
      <v-card-text class="px-0">
        <!-- <v-main class="py-2"> -->
          <v-container fluid ma-0 pa-0 center>
            <v-row no-gutters v-bind:class="{'pt-2': $vuetify.breakpoint.lgAndUp}">
              <v-col cols="3" class="hidden-sm-and-down">
                <v-spacer></v-spacer>
              </v-col>
              <v-col>
                <slot></slot>
              </v-col>
              <v-col cols="3" class="hidden-sm-and-down">
                <v-spacer></v-spacer>
              </v-col>
            </v-row>
          </v-container>
        <!-- </v-main> -->
      </v-card-text>
    </v-card>
  </v-dialog>
</template>

<script>
/** slots: title & default (body) */
export default {
  components: {
    ReIcon: () => import("../misc/ResponsiveIcon"),
  },
  props: {
    dialog: {
      type: Boolean,
      default: false,
      required: true
    },
    color: {
      type: String,
      default: "white",
      required: false
    }
  },
  methods: {
    onDismiss() {
      this.$emit('update:dialog', false);
      this.$emit('dismiss');
    }
  },
};
</script>