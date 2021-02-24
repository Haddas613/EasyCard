
const state = () => ({
    /**This is main navigation header. Wizard header is accessed directly by component. On each route change all values are set correspondingly */
    header: {
        text: {
            translate: false,
            value: "easycard"
        },
        threeDotMenu: [],

        /** do not display title and switch color to desktop version. 
         * This is default behavior for Dashboard page */
        altDisplay: true
    },
    requestsCount: 0,
    versionMismatch: false
});

const getters = {};
const actions = {
};

const mutations = {
    changeHeader(state, { value }) {
        Object.assign(state.header, value);
    },
    requestsCountIncrement(state) {
        state.requestsCount++;
    },
    requestsCountDecrement(state) {
        state.requestsCount--;
    },
    setVersionMismatch(state, newVersionMismatch) {
        state.versionMismatch = newVersionMismatch;
    },
}

export default {
    namespaced: true,
    state,
    getters,
    actions,
    mutations
}