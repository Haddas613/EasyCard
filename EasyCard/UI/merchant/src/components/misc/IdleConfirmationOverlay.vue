<template>
  <v-overlay z-index="10" opacity="0.25" v-if="idlingStore.showWarningPrompt">
    <v-card class="text-center" rounded light v-bind:class="{'mx-4' : $vuetify.breakpoint.smAndDown}">
      <v-card-title class="subtitle-1">{{$t("SecurityNotice")}}</v-card-title>
      <v-card-text>{{$t("SessionEndingMsg")}}</v-card-text>
      <v-card-actions class="d-flex justify-center">
        <v-row no-gutters>
            <v-col cols="4">
                <v-btn color="error" @click="$oidc.signOut()">
                    {{$t("SignOut")}}
                </v-btn>
            </v-col>
            <v-col cols="4">
                <p class="pt-1">{{idlingTimeLeft}}</p>
            </v-col>
            <v-col cols="4">
                <v-btn color="primary" @click="renew()">
                    {{$t("Renew")}}
                </v-btn>
            </v-col>
        </v-row>
      </v-card-actions>
    </v-card>
  </v-overlay>
</template>

<script>
import { mapState, mapGetters } from "vuex";

export default {
  computed: {
    ...mapState({
      versionMismatchStore: state => state.ui.versionMismatch,
      idlingStore: state => state.idling,
    }),
    ...mapGetters({
      idlingTimeLeft: 'idling/idlingTimeLeft',
    }),
  },
  methods: {
    renew() {
        this.$store.commit('idling/refreshTime');
    }
  }
};
</script>