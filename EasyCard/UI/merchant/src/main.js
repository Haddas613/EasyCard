import Vue from 'vue'
import App from './App.vue'
import router from './router'
import store from './store'
import vuetify from './plugins/vuetify';
import i18n from './i18n'
import 'vue-oidc-client/src/polyfill';
import mainAuth from './auth';
import Api from './api/ApiBase';
import './assets/css/main.scss';
import VueLodash from 'vue-lodash'
import lodash from 'lodash'

Vue.config.productionTip = false

mainAuth.useRouter(router);

mainAuth.startup().then(ok => {
  Vue.use(Api);
  Vue.use(VueLodash, { lodash: lodash })
  
  if (ok) {
    new Vue({
      router,
      store,
      vuetify,
      i18n,
      render: function (h) { 
        this.$store.dispatch('localization/refreshLocale',{ $vuetify: vuetify.framework, $i18n: i18n });
        return h(App) 
      }
    }).$mount('#app')
  }
});