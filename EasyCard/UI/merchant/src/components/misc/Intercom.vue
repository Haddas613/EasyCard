<template>
  <div hidden></div>
</template>

<script>
import { mapState } from "vuex";

export default {
  data() {
    return {
      APP_ID: "pb65ta0u"
    };
  },
  async mounted() {
    this.initIntercom();
  },
  computed: {
    ...mapState({
      terminalStore: state => state.settings.terminal
    })
  },
  watch: {
    async 'terminalStore' (newValue, oldValue) {
      await this.initIntercom();
    }
  },
  methods: {
    async initIntercom() {
      const APP_ID = this.APP_ID;
      var w = window;
      var ic = w.Intercom;
      if (typeof ic === "function") {
        ic("reattach_activator");
        ic("update", w.intercomSettings);
      } else {
        var d = document;
        var i = function() {
          i.c(arguments);
        };
        i.q = [];
        i.c = function(args) {
          i.q.push(args);
        };
        w.Intercom = i;
        var l = function() {
          var s = d.createElement("script");
          s.type = "text/javascript";
          s.async = true;
          s.src = "https://widget.intercom.io/widget/" + APP_ID;
          var x = d.getElementsByTagName("script")[0];
          x.parentNode.insertBefore(s, x);
        };
        if (document.readyState === "complete") {
          l();
        } else if (w.attachEvent) {
          w.attachEvent("onload", l);
        } else {
          w.addEventListener("load", l, false);
        }
      }

      let user = await this.$oidc.getUserProfile();
      
      window.Intercom("boot", {
        app_id: APP_ID,
        user_id: user.sub, // user_id
        name: `${user.extension_FirstName} ${user.extension_LastName} | ${this.terminalStore.merchantName} | ${this.terminalStore.label}`, // Full name
        email: user.name, // Email address
        //created_at: "1617881513" // Signup date as a Unix timestamp
      });
    }
  }
};
</script>