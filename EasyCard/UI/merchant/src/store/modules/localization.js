import i18n from '../../i18n'

const state = () => ({
  currentLocale:  i18n.locale 
});

const getters = {};
const actions = {};

const mutations = {
    changeLanguage(state, {vm, newLocale}){
        state.currentLocale = newLocale;
        vm.$i18n.locale = state.currentLocale;
        if (state.currentLocale == 'he-IL') {
            vm.$vuetify.rtl = true;
            vm.$vuetify.lang.current = 'he';
        } else {
            vm.$vuetify.rtl = false;
            vm.$vuetify.lang.current = 'en';
        }
    }
}

export default {
    namespaced: true,
    state,
    getters,
    actions,
    mutations
  }