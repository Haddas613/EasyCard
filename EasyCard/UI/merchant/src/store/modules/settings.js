
const state = () => ({
  terminal: {},
  currency: {}
});

const getters = {};
const actions = {
  async getDefaultSettings(state, { api }){
      let terminals = (await api.terminals.getTerminals());
      terminals = terminals ? terminals.data : [];

      if(!state.terminal || !state.terminal.terminalID){
        state.terminal = terminals[0];
      }else{
        let exists = this.lodash.some(terminals, t => t.terminalID === state.terminal.terminalID);
        if(!exists) state.terminal = terminals[0];
      }

      let dictionaries = await api.dictionaries.getTransactionDictionaries();
      let currencies = dictionaries ? dictionaries.currencyEnum : [];

      if(!state.currency || !state.currency.code){
        if(currencies.length > 0 && !state.currency){
          state.currency = currencies[0];
        }
      }
      //cache validation for currencies
      else if (currencies.length > 0 && state.currency) {
        let exists = vm.lodash.some(state.currencies, c => c.code === state.currency.code);
        if(!exists) state.currency = currencies[0];
      }
  }
};

const mutations = {
  changeTerminal(state, { vm, newTerminal }) {
    state.terminal = newTerminal;
  },
  changeCurrency(state, { vm, newCurrency }) {
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