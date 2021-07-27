import Vue from 'vue'
import Vuex from 'vuex'
import createPersistedState from "vuex-persistedstate";
import localization from './modules/localization';
import settings from './modules/settings';
import ui from './modules/ui';
import payment from './modules/payment';
import idling from './modules/idling';

Vue.use(Vuex)

export default new Vuex.Store({
  state: {
  },
  mutations: {
  },
  actions: {
  },
  modules: {
    localization,
    settings,
    ui,
    payment,
    idling
  },
  plugins: [
    createPersistedState({
      paths: ['localization', 'settings', 'payment'],
    })
  ]
})
