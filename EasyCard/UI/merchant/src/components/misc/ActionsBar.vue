<template>
  <v-flex>
    <v-footer :fixed="true" :padless="true" v-if="footer" height="90">
      <v-card flat tile width="100%" class="text-center" dark color="grey darken-3">
        <v-card-text class="d-flex justify-space-between pb-0 mt-3 py-0" dir="ltr">
          <div v-for="item in items" class="pt-1" :key="item.text">
            <v-btn class="white mx-3" icon link :to="item.to">
              <v-icon :class="item.css">{{ item.icon }}</v-icon>
            </v-btn>
            <p class="pt-1">{{$t(item.text)}}</p>
          </div>
          <div class="py-0">
            <div class="mx-8">
            </div>
          </div>
        </v-card-text>
      </v-card>
    </v-footer>
    <v-row no-gutters justify="end" v-if="!footer">
      <v-col cols="4" v-for="item in items" :key="item.text">
        <v-btn
          class="px-2 text-none body-2 font-weight-medium"
          depressed
          :small="$vuetify.breakpoint.sm"
          :color="btncoloring"
          :width="getBtnWidth"
          link
          :to="item.to"
        >
          <v-icon :class="item.css" left>{{item.icon}}</v-icon>
          {{$t(item.text)}}
        </v-btn>
      </v-col>
    </v-row>
  </v-flex>
</template>

<script>
import appConstants from "../../helpers/app-constants";
import { mapState } from "vuex";

export default {
  props: {
    footer: {
      type: Boolean,
      default: true
    },
    btncoloring: {
      type: String,
      default: "white ecnavLink--text",
      required: false
    }
  },
  computed: {
    getBtnWidth() {
      switch (this.$vuetify.breakpoint.name) {
        case "md":
        case "lg":
        case "xl":
          return "110px";
        case "sm":
        default:
          return "auto";
      }
    },
    ...mapState({
      terminal: state => state.settings.terminal
    })
  },
  data() {
    return {
      items: [
        {
          icon: "mdi-chevron-down-box-outline",
          css: "deep-purple--text",
          text: "Charge",
          link: "Charge",
          quickChargeLink: "QuickCharge"
        },
        {
          icon: "mdi-chevron-left-box-outline",
          css: "orange--text",
          text: "Request",
          link: "CreatePaymentRequest",
        },
        {
          icon: "mdi-chevron-up-box-outline",
          css: "primary--text",
          text: "Refund",
          link: "Refund",
          allowedFor: [appConstants.users.roles.manager, appConstants.users.roles.billingAdmin]
        }
      ]
    };
  },
  async mounted() {
    const self = this;
    await this.refreshLinks();
  },
  watch: {
    async terminal(newValue, oldValue) {
      await this.refreshLinks();
    }
  },
  methods: {
    async refreshLinks() {
      let items = [];
      for(var i of this.items){
        i.to = {
          name: (this.terminal.settings.useQuickChargeByDefault && i.quickChargeLink) ? i.quickChargeLink : i.link
        }
        if (!i.allowedFor){
          items.push(i);
        }else if (await this.$oidc.isInRole(i.allowedFor)){
          items.push(i);
        }
      }
      this.items = items;
    }
  },
};
</script>