import Vue from 'vue'
import TransactionsApi from './modules/transactions/TransactionsApi';
import DictionariesApi from './modules/transactions/DictionariesApi';
import TerminalsApi from './modules/profile/TerminalsApi';
import ConsumersApi from './modules/profile/ConsumersApi';
import ItemsApi from './modules/profile/ItemsApi';
import moment from 'moment'
import store from '../store/index';
import i18n from '../i18n'
import CardTokensApi from './modules/transactions/CardTokensApi';

class ApiBase {
    constructor() {
        this.oidc = Vue.prototype.$oidc;
        this._ongoingRequests = {};

        /**Apis */
        this.transactions = new TransactionsApi(this);
        this.dictionaries = new DictionariesApi(this);
        this.terminals = new TerminalsApi(this);
        this.consumers = new ConsumersApi(this);
        this.items = new ItemsApi(this);
        this.cardTokens = new CardTokensApi(this);
    }

    /** Get requests are syncronized based on their url and query string to prevent the same requests be fired at the same time */
    async get(url, params) {
        if (params) {
            if (params.page && params.itemsPerPage) {
                params.take = params.itemsPerPage;
                params.skip = params.itemsPerPage * (params.page - 1);
            }
            /**Clear up empty params */
            for(var prop of Object.keys(params)){
                if(!params[prop]){
                    delete params[prop];
                }
            }
        }
        let _urlKey = url;

        let requestUrl = new URL(url)
        if(params){
            requestUrl.search = new URLSearchParams(params).toString();
            _urlKey += requestUrl.search;
        }

        if(this._ongoingRequests[_urlKey]){
            return this._ongoingRequests[_urlKey];
        }

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
        this._ongoingRequests[_urlKey] = this._handleRequest(request);

        return new Promise((s, e) => s(this._ongoingRequests[_urlKey])).finally(() => delete this._ongoingRequests[_urlKey])
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

    async put(url, payload) {
        let requestUrl = new URL(url);
        let request = fetch(requestUrl, {
            method: 'PUT',
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

    async delete(url) {
        let requestUrl = new URL(url);
        let request = fetch(requestUrl, {
            method: 'DELETE',
            withCredentials: true,
            mode: 'cors',
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${this.oidc.user.access_token}`,
                'Accept': 'application/json'
            }
        });

        return this._handleRequest(request, true);
    }

    async _handleRequest(request, showSuccessToastr = false) {
        try {
            store.commit("ui/requestsCountIncrement");
            request = await request;
            let result = await request.json();
            if (request.ok) {
                if (result.status === "warning") {
                    Vue.toasted.show(result.message, { type: 'info'});
                }else if(showSuccessToastr && result.status === "success"){
                    Vue.toasted.show(result.message, { type: 'success', duration: 5000});
                }

                return result;
            }
            else{
                //Server Validation errors are returned to component
                if(request.status === 400){
                    return result;
                }
                else if(request.status === 404){
                    Vue.toasted.show(result.message, { type: 'error'});
                    return null;
                }else{
                    Vue.toasted.show(result.message, { type: 'error'});
                }
            } 
            return result;

        } catch (err) {
            Vue.toasted.show(i18n.t('ServerErrorTryAgainLater'), { type: 'error'});
        } finally{
            store.commit("ui/requestsCountDecrement");
        }
        return null;
    }

    _formatHeaders(headers) {
        return Object.keys(headers.columns).map(key => { return { value: key, text: headers.columns[key].name } });
    }

    format(d, headers, dictionaries) {
        for (const property in d) {
            let v = d[property]
            let h = headers[property]
            if(!h) continue;
            if (h.dataType == 'guid' && v && v.length > 8) {
                d[`$${property}`] = v
                d[property] = v.substring(0, 8)
            }
            else if (h.dataType == 'dictionary') {
                d[`$${property}`] = v
                d[property] = dictionaries[h.dictionary][v]
            }
            // else if (h.dataType == 'money') {
            //     d[`$${property}`] = v
            //     d[property] = new Intl.NumberFormat().format(v) // TODO: locale, currency symbol
            // }
            else if (h.dataType == 'date') {
                d[`$${property}`] = v
                d[property] = moment(String(v)).format('MM/DD/YYYY') // TODO: locale
            }
        }
        return d
    }
}

export default {
    install: function (Vue, ) {
        Object.defineProperty(Vue.prototype, '$api', { value: new ApiBase() });
    }
}

