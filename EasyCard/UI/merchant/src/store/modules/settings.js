
const state = () => ({
  terminal: null,
  currency: null
});

const getters = {};
const actions = {
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