<template>
  <div>
    <v-card flat color="ecbg">
      <v-card-text>
        <v-row no-gutters>
          <v-col cols="12" md="2">
            <lang-switcher class></lang-switcher>
          </v-col>
          <v-col cols="12" md="10" class="text-end">
            <v-btn @click="$oidc.signOut()">
              <v-icon left>mdi-logout</v-icon>
              {{$t("SignOut")}}
            </v-btn>
          </v-col>
        </v-row>
      </v-card-text>
    </v-card>
  </div>
</template>

<script>
import { mapState } from "vuex";
export default {
  components: {
    LangSwitcher: () => import("../../components/LanguageSwitcher"),
  },
  data() {
    return {
    };
  },
  async mounted() {
    await this.$store.dispatch('settings/getDefaultSettings', { api: this.$api, lodash: this.lodash });
  },
  computed: {
    isRtl: {
      cache: false,
      get: function() {
        return this.$vuetify.rtl === true;
      }
    }
  }
};
</script>