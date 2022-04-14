import appConstants from "../../helpers/app-constants";

const state = () => ({
    /**This is main navigation header. Wizard header is accessed directly by component. On each route change all values are set correspondingly */
    header: {
        text: {
            translate: false,
            value: "easycard"
        },
        threeDotMenu: [],

        /** When true: do not display title and switch color to desktop version. 
         * This is default behavior for Dashboard page */
        altDisplay: true,

        /**When set to function, refresh button will be shown in header */
        refresh: null
    },
    requestsCount: 0,
    versionMismatch: false,
    dashboardDateFilter: {
        quickDateType: appConstants.filtering.defaultDashboardQuickDateType,
        customDate: false,
        dateFrom: null,
        dateTo: null
    },
    //For timeline chart & other elements that typically require wider range
    dashboardDateFilterAlt: {
        quickDateType: appConstants.filtering.defaultDashboardAltQuickDateType,
        granularity: appConstants.filtering.defaultDashboardAltGranularity,
        altQuickDateFilter: appConstants.filtering.defaultDashboardAltQuickDateFilterAltEnum,
        customDate: false,
        dateFrom: null,
        dateTo: null
    },

    //can be used to force re-render cached keep-alive components
    keepAliveRenderState: 0,
});

const getters = {};
const actions = {};

const mutations = {
    changeHeader(state, {value}) {
        Object.assign(state.header, value);
    },
    setRefreshHandler(state, { value }) {
        state.header.refresh = value;
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
    setDashboardDateFilter(state, value){
        state.dashboardDateFilter = value;
    },
    setDashboardDateFilterAlt(state, value){
        state.dashboardDateFilterAlt = value;
    },
    refreshKeepAlive(state){
        state.keepAliveRenderState += 1;
    },
}

export default {
    namespaced: true,
    state,
    getters,
    actions,
    mutations
}