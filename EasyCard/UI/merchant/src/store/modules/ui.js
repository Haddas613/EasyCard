
const state = () => ({
    header: {
        text: {
            translate: false,
            value: "easycard"
        },
        threeDotMenu: []
    }

});

const getters = {};
const actions = {
};

const mutations = {
    changeHeader(state, {value}){
        Object.assign(state.header, value);
    }
}

export default {
    namespaced: true,
    state,
    getters,
    actions,
    mutations
}