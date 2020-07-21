
const state = () => ({
    headerText: {
        translate: false,
        text: "easycard"
    },
});

const getters = {};
const actions = {
};

const mutations = {
    changeHeaderText(state, { vm, newHeaderText }) {
        state.headerText = newHeaderText;
    }
}

export default {
    namespaced: true,
    state,
    getters,
    actions,
    mutations
}