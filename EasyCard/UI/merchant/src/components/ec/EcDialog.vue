<template>
  <v-dialog v-model="dialog" fullscreen>
    <v-card>
      <v-card-title class="headline px-2 py-0">
        <v-row :align="'center'">
          <v-col class="d-flex justify-start px-1" cols="2" md="3" lg="4">
            <v-icon @click="$emit('update:dialog', false)" class="px-2">mdi-arrow-left</v-icon>
          </v-col>
          <v-col class="d-flex justify-space-around">
            <slot name="title"></slot>
          </v-col>
          <v-col cols="2" md="3" lg="4" class="d-flex justify-end">
          </v-col>
        </v-row>
      </v-card-title>
      <v-divider></v-divider>
      <v-card-text class="px-0">
        <v-main class="py-2">
          <v-container fluid ma-0 pa-0 center>
            <v-row no-gutters v-bind:class="{'pt-2': $vuetify.breakpoint.lgAndUp}">
              <v-col cols="3" class="hidden-sm-and-down">
                <v-spacer></v-spacer>
              </v-col>
              <v-col class="d-flex justify-space-around">
                <slot></slot>
              </v-col>
              <v-col cols="3" class="hidden-sm-and-down">
                <v-spacer></v-spacer>
              </v-col>
            </v-row>
          </v-container>
        </v-main>
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