<template>
  <v-app id="inspire">
    <v-main>
      <v-container class="ecbg--lighten-4 fill-height text-center" fluid>
        <v-row align="center" justify="center">
          <v-col cols="12">
            <p class="display-1">{{$t('LoginWelcomeText')}}</p>
          </v-col>
          <v-col cols="12" sm="8" md="4">
            <v-card class="elevation-12">
              <v-card-title class="overline justify-center">{{$t('LoginTitle')}}</v-card-title>
              <v-card-text class="pb-0">
                <v-btn :color="'primary'" :to="buttonLink">{{$t('Login')}}</v-btn>
              </v-card-text>
              <v-card-actions>
                <v-row no-gutters>
                  <v-spacer></v-spacer>
                  <v-col cols="3" sm="3">
                    <lang-switcher></lang-switcher>
                  </v-col>
                </v-row>
              </v-card-actions>
            </v-card>
          </v-col>
        </v-row>
      </v-container>
    </v-main>
  </v-app>
</template>

<script>

import LangSwitcher from "../components/LanguageSwitcher"

export default {
  components: {LangSwitcher},
  mounted() {
    if (mainAuth.isAuthenticated) {
      this.$router.push(this.buttonLink);
    }
  },
  data() {
    return {
      buttonLink: "/admin"
    };
  }
};
</script>