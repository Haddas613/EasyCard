<template>
  <div hidden></div>
</template>

<script>
export default {
  data() {
    return {
      APP_ID: "pb65ta0u"
    };
  },
  mounted() {
    //this.initIntercom();
  },
  methods: {
    initIntercom() {
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

      window.Intercom("boot", {
        app_id: APP_ID,
        user_id: "632eb049-8562-411d-ab9f-ab92007f0293", // user_id
        name: "Bob", // Full name
        email: "kanivetsig@mydigicode.com", // Email address
        created_at: "1617881513" // Signup date as a Unix timestamp
      });
      window.Intercom("show")
    }
  }
};
</script>