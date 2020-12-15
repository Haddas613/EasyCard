
const state = () => ({
  currency: {}
});

const getters = {};
const actions = {
  async getDefaultSettings(store, { api, lodash }) {
    let dictionaries = await api.dictionaries.getTransactionDictionaries();
    let currencies = dictionaries ? dictionaries.currencyEnum : [];
    if (!state.currency || !state.currency.code) {
      if (currencies.length > 0) {
        await store.dispatch('changeCurrency', {api, newCurrency: currencies[0]});
      }
    }
    //cache validation for currencies
    else if (currencies.length > 0 && state.currency) {
      let exists = lodash.some(state.currencies, c => c.code === state.currency.code);
      if (!exists) await store.dispatch('changeCurrency', {api, newCurrency: currencies[0]});
    }
  },
  async changeCurrency({ commit }, { api, newCurrency }) {
    commit('setCurrency', newCurrency);
  }
};

const mutations = {
  setCurrency(state, newCurrency) {
    state.currency = newCurrency;
  }
}

export default {
  namespaced: true,
  state,
  getters,
  actions,
  mutations
}