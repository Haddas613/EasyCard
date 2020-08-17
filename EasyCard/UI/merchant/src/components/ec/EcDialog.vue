<template>
  <v-dialog v-model="dialog" fullscreen >
    <v-card flat>
      <v-card-title class="headline py-0">
        <v-row :align="'center'">
          <v-col class="d-flex justify-start px-1" cols="2">
            <v-icon @click="$emit('update:dialog', false)" class="px-2">mdi-arrow-left</v-icon>
          </v-col>
          <v-col cols="7" lg="8" class="d-flex justify-space-around px-0 text-subtitle-1 text-md-h6 text-lg-h6">
            <div class="text-truncate">
              <slot name="title"></slot>
            </div>
          </v-col>
          <v-col cols="3" lg="2" xl="2" class="d-flex justify-end">
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
  props: {
    dialog: {
      type: Boolean,
      default: false,
      required: true
    }
  }
};
</script>

<style lang="scss" scoped>
</style>