<template>
  <div>
    <v-card flat tile width="100%" height="200px" class="text-center mobile-top-area-dashboard"
      color="primary" dark v-if="$vuetify.breakpoint.xs">
      <v-card-text class="py-0">
        <v-list class="py-0" color="primary">
          <v-list-item class="justify-center">
            <!-- <p class="display-1">easycard</p> -->
            <img src="https://ecng-identity.azurewebsites.net/img/logo.png">
          </v-list-item>
          <v-list-item-avatar height="50" width="auto">
            <img src="https://randomuser.me/api/portraits/men/81.jpg" />
          </v-list-item-avatar>
          <v-list-item class="justify-center">
            <p class="subtitle-1 pt-4">{{$t('@GoodMorning').replace("@name", userName)}}</p>
          </v-list-item>
        </v-list>
      </v-card-text>
    </v-card>
    <actions-bar v-if="$vuetify.breakpoint.xs"></actions-bar>
  </div>
</template>

<script>
import ActionsBar from "../components/misc/ActionsBar";
export default {
  components: { ActionsBar },
  data() {
    return {
      userName: null,
      publicPath: process.env.BASE_URL
    }
  },
  async mounted(){
    this.userName = !!this.$oidc ? (await this.$oidc.getUserProfile()).name : null;
  }
};
</script>

<style lang="scss" scoped>
.mobile-top-area-dashboard {
  position: absolute;
  right: 0;
  left: 0;
  top: 0;
}

</style>