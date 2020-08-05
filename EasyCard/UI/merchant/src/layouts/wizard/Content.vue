<template>
  <v-main class="ecbg">
    <v-container fluid ma-0 pa-0 center>
      <v-row no-gutters v-bind:class="{'pt-2': $vuetify.breakpoint.lgAndUp}">
        <v-col cols="3" class="hidden-sm-and-down">
          <v-spacer></v-spacer>
        </v-col>
        <v-col class="d-flex justify-space-around">
          <router-view />
        </v-col>
        <v-col cols="3" class="hidden-sm-and-down">
          <v-spacer></v-spacer>
        </v-col>
      </v-row>
    </v-container>
  </v-main>
</template>
<script>
export default {
  name: "WizardContent"
};
</script>