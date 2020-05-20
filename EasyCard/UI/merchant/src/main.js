import Vue from 'vue'
import App from './App.vue'
import router from './router'
import store from './store'
import vuetify from './plugins/vuetify';
import i18n from './i18n'
import 'vue-oidc-client/src/polyfill';
import mainAuth from './auth';

Vue.config.productionTip = false

mainAuth.useRouter(router);

mainAuth.startup().then(ok => {
  if (ok) {
    new Vue({
      router,
      store,
      vuetify,
      i18n,
      render: function (h) { return h(App) }
    }).$mount('#app')
  }
});