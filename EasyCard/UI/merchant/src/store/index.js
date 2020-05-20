import Vue from 'vue'
import Vuex from 'vuex'
import createPersistedState from "vuex-persistedstate";
import localization from './modules/localization';

Vue.use(Vuex)

export default new Vuex.Store({
  state: {
  },
  mutations: {
  },
  actions: {
  },
  modules: {
    localization
  },
  plugins: [
    createPersistedState({
      paths: ['localization'],
    })
  ]
})
