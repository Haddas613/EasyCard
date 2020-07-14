import Vue from 'vue'
import TransactionsApi from './modules/TransactionsApi';
import DictionariesApi from './modules/DictionariesApi';
import TerminalsApi from './modules/profile/TerminalsApi';
import ConsumersApi from './modules/profile/ConsumersApi';
import i18n from '../i18n'


class ApiBase {
    constructor() {
        this.oidc = Vue.prototype.$oidc;
        this.transactions = new TransactionsApi(this);
        this.dictionaries = new DictionariesApi(this);
        this.terminals = new TerminalsApi(this);
        this.consumers = new ConsumersApi(this);
        
        this.toastedOpts = {
            iconPack: 'mdi',
            //duration: 5000,
            keepOnHover: true,
            containerClass: 'ecng-toast',
            action : {
                icon : 'close-circle',
                onClick : (e, toastObject) => {
                    toastObject.goAway(0);
                }
            },
        }
    }

    async get(url, params) {
        if (params) {
            if (params.page && params.itemsPerPage) {
                params.take = params.itemsPerPage;
                params.skip = params.itemsPerPage * (params.page - 1);
            }
        }
        let requestUrl = new URL(url)
        requestUrl.search = new URLSearchParams(params).toString();
        let request = fetch(requestUrl, {
            method: 'GET',
            withCredentials: true,
            mode: 'cors',
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${this.oidc.user.access_token}`,
                'Accept': 'application/json'
            }
        });
        return this._handleRequest(request);
    }

    async post(url, payload) {
        let requestUrl = new URL(url);
        let request = fetch(requestUrl, {
            method: 'POST',
            withCredentials: true,
            mode: 'cors',
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${this.oidc.user.access_token}`,
                'Accept': 'application/json'
            },
            body: JSON.stringify(payload)
        });

        return this._handleRequest(request, true);
    }

    async _handleRequest(request, showSuccessToastr = false) {
        try {
            request = await request;
            let result = await request.json();
            if (request.ok) {
                if (result.status === "warning") {
                    Vue.toasted.show(result.message, { type: 'info', ...this.toastedOpts });
                }else if(showSuccessToastr && result.status === "success"){
                    Vue.toasted.show(result.message, { type: 'success', duration: 5000, ...this.toastedOpts });
                }

                return result;
            }
            else{
                //Server Validation errors are returned to component
                if(request.status === 400){
                    return result;
                }else{
                    Vue.toasted.show(result.message, { type: 'error', ...this.toastedOpts });
                }
            } 
            return result;

        } catch (err) {
            Vue.toasted.show(i18n.t('ServerErrorTryAgainLater'), { type: 'error', ...this.toastedOpts });
        }
        return null;
    }

    _formatHeaders(headers) {
        return Object.keys(headers.columns).map(key => { return { value: key, text: headers.columns[key].name } });
    }
}

export default {
    install: function (Vue, ) {
        Object.defineProperty(Vue.prototype, '$api', { value: new ApiBase() });
    }
}

