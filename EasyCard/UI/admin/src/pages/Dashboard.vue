<template>
  <div>
    <v-card flat tile width="100%" height="200px" class="text-center mobile-top-area-dashboard dash-gradient" dark v-if="$vuetify.breakpoint.xs">
      <v-card-text class="py-0">
        <v-list class="py-0" color="transparent">
          <v-list-item class="justify-center">
            <!-- <p class="display-1">easycard</p> -->
            <img src="https://ecng-identity.azurewebsites.net/img/logo.png">
          </v-list-item>
          <v-list-item-avatar height="50" width="auto">
            <img src="https://randomuser.me/api/portraits/men/81.jpg" />
          </v-list-item-avatar>
          <v-list-item class="justify-center">
            <p class="subtitle-1 pt-4">{{$t('@WelcomeText').replace("@name", userName)}}</p>
          </v-list-item>
        </v-list>
      </v-card-text>
    </v-card>
    <v-row no-gutters class="stats-area d-flex" v-bind:class="{'mobile': $vuetify.breakpoint.xs}">
      <v-col cols="12" md="4" class="dashboard-tile-item">
        <sales-stats></sales-stats>
      </v-col>
      <v-col cols="12" md="4" class="dashboard-tile-item">
        <sms-timeline-stats></sms-timeline-stats>
      </v-col>
      <v-col cols="12" md="4" class="dashboard-tile-item">
        <top-merchants-stats></top-merchants-stats>
      </v-col>
      <!-- <v-col cols="12" md="4" class="dashboard-tile-item">
        <charge-type-stats></charge-type-stats>
      </v-col>
      <v-col cols="12" md="4" class="dashboard-tile-item">
        <information-stats></information-stats>
      </v-col> -->
    </v-row>
  </div>
</template>

<script>
export default {
  components: { 
    ActionsBar: () => import("../components/misc/ActionsBar"),
    SalesStats: () => import("../components/stats/SalesStats"),
    SmsTimelineStats: () => import("../components/stats/SmsTimelineStats"),
    TopMerchantsStats: () => import("../components/stats/TopMerchantsStats"),
    ChargeTypeStats: () => import("../components/stats/ChargeTypeStats"),
    InformationStats: () => import("../components/stats/InformationStats"),
  },
  data() {
    return {
      userName: null,
      publicPath: this.$cfg.BASE_URL
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
.dash-gradient{
    background: linear-gradient(120deg, #139cca 40%, #1096c6 40%);
    // background: linear-gradient(120deg, #0f99c7f0 40%, #1096c6 41%);
}
.footer-spacer{
  height: 100px;
}
.stats-area{
  &.mobile{
    margin-top: 170px;
  }
  div{
    padding: 0 4px;
  }
  .dashboard-tile-item{
    display: flex;
    div{
      width: 100%;
    }
  }
}
</style>