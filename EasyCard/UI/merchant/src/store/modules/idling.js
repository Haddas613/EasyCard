import cfg from "../../app.config";
import Vue from "vue";

const state = () => ({
    idleTime: (cfg.VUE_APP_IDLE_TIMER_MINUTES || 50) * 60, // converting to seconds
    secondsLeft: null,
    showWarningPrompt: false,
    showWarningPromptWhenSeconds: 60,
    interval: null
});

function pad(num) {
    return num <= 9 ? ("0"+num) : num;
}
function mmss(secs) {
  var minutes = Math.floor(secs / 60);
  return `${pad(minutes)}:${pad(secs % 60)}`;
}

const getters = {
    idlingTimeLeft: (state) => {
        return mmss(state.secondsLeft);
    }
};

const actions = {
    start({dispatch, state, commit}) {
        if(state.interval){
            return;
        }
        commit('refreshTime');
        state.interval = setInterval(() => {
            if(--state.secondsLeft <= 0){
                dispatch('stop');
                Vue.prototype.$oidc.signOut();
            }
            if(state.secondsLeft <= state.showWarningPromptWhenSeconds){
                state.showWarningPrompt = true;
            }
        }, 1000);
    },
    stop({state, commit}){
        if(!state.interval){
            return;
        }
        clearInterval(state.interval);
        state.interval = null;
    }
};

const mutations = {
    refreshTime(state) {
        state.secondsLeft = state.idleTime;
        state.showWarningPrompt = false;
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