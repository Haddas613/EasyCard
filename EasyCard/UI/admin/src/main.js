import Vue from 'vue'
import App from './App.vue'
import router from './router'
import store from './store'
import vuetify from './plugins/vuetify';
import i18n from './i18n'
import 'vue-oidc-client/src/polyfill';
import './assets/css/main.scss';
import Api from './api/ApiBase';
import VueLodash from 'vue-lodash'
import lodash from 'lodash'
import VueCardFormat from './plugins/card-validator';
import Toasted from 'vue-toasted';
import ecdate from './extensions/filters/ecdate'
import currency from './extensions/filters/currency'
import guid from './extensions/filters/guid'
import length from './extensions/filters/length'
import vmoney from 'v-money';
import auth from './auth'
import VueClipboard from 'vue-clipboard2';
import mixin from './extensions/mixins';
import "./integrations";

Vue.config.productionTip = false

Vue.use(auth);
Vue.use(Api);
Vue.use(VueLodash, { lodash: lodash });
Vue.use(VueCardFormat);
Vue.use(vmoney, { precision: 2, decimal: '.', thousands: '' });
Vue.use(Toasted, {
    iconPack: 'mdi',
    keepOnHover: true,
    containerClass: 'ecng-toast',
    action: {
        icon: 'close-circle',
        onClick: (e, toastObject) => {
            toastObject.goAway(0);
        }
    },
});
Vue.use(VueClipboard);
Vue.filter('ecdate', ecdate);
Vue.filter('currency', currency);
Vue.filter('guid', guid);
Vue.filter('length', length);
Vue.mixin(mixin);

new Vue({
    router,
    store,
    vuetify,
    i18n,
    render: function(h) {
        this.$store.dispatch('localization/refreshLocale', { $vuetify: vuetify.framework, $i18n: i18n });
        return h(App)
    }
}).$mount('#app')