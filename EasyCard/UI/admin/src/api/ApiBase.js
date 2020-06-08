import Vue from 'vue'
import TransactionsApi from './modules/TransactionsApi';
import DictionariesApi from './modules/DictionariesApi';
import TerminalsApi from './modules/TerminalsApi';
import moment from 'moment'

class ApiBase {
    constructor() {
        this.oidc = Vue.prototype.$oidc;
        this.transactions = new TransactionsApi(this);
        this.dictionaries = new DictionariesApi(this);
        this.terminals = new TerminalsApi(this);
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
        let request = await fetch(requestUrl, {
            method: 'GET',
            withCredentials: true,
            mode: 'cors',
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${this.oidc.user.access_token}`,
                'Accept': 'application/json'
            }
        });
        return await request.json();
    }

    async post(url, payload) {
        let requestUrl = new URL(url);
        let request = await fetch(requestUrl, {
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
        let result = await request.json();

        //TODO: handle operation response
        if (result.error === true || result.status === "error") {
            result.isError = true;
        }
        return result;
    }

    formatHeaders(headers){
        return Object.keys(headers.columns).map(key => {return { value: key, text: headers.columns[key].name } });
    }

    formatColumns(d, headers, dictionaries) {
        for (const property in d) {
            let v = d[property]
            let h = headers[property]
            if (h.dataType == 'string' && v && v.length > 8) {
                d[`$${property}`] = v
                d[property] = v.substring(0, 8)
            }
            else if (h.dataType == 'dictionary') {
                if(dictionaries){
                    d[`$${property}`] = v
                    d[property] = dictionaries[h.dictionary][v]
                }
            }
            else if (h.dataType == 'money') {
                d[`$${property}`] = v
                d[property] = new Intl.NumberFormat().format(v) // TODO: locale, currency symbol
            }
            else if (h.dataType == 'date') {
                if(moment.isDate(v)){
                    d[`$${property}`] = v
                    d[property] = moment(String(v)).format('MM/DD/YYYY') // TODO: locale
                }
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

