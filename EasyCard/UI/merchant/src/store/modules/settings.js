
const state = () => ({
  terminal: {},
  currency: {}
});

const getters = {};
const actions = {
  async getDefaultSettings(store, { api, lodash }) {
    let terminals = (await api.terminals.getTerminals());
    terminals = terminals ? terminals.data : [];
    let state = store.state;
    if (!state.terminal || !state.terminal.terminalID) {
      await store.dispatch('changeTerminal', {api, newTerminal: terminals[0].terminalID});
    } else {
      let exists = lodash.some(terminals, t => t.terminalID === state.terminal.terminalID);
      if (!exists) await store.dispatch('changeTerminal', {api, newTerminal: terminals[0].terminalID});
    }

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
  async changeTerminal({ commit }, { api, newTerminal }) {
    let terminal = await api.terminals
      .getTerminal(typeof (newTerminal) === "string" ? newTerminal : newTerminal.terminalID);

    commit('setTerminal', terminal);
  },
  async changeCurrency({ commit }, { api, newCurrency }) {
    commit('setCurrency', newCurrency);
  }
};

const mutations = {
  setTerminal(state, newTerminal) {
    state.terminal = newTerminal;
  },
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