import Vue from 'vue'
import TransactionsApi from './modules/TransactionsApi';

class ApiBase {
    constructor() {
        this.oidc = Vue.prototype.$oidc;
        this.transactions = new TransactionsApi(this);
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

    _formatHeaders(headers){
        return Object.keys(headers.columns).map(key => {return { value: key, text: headers.columns[key].name } });
    }
}

export default {
    install: function (Vue, ) {
        Object.defineProperty(Vue.prototype, '$api', { value: new ApiBase() });
    }
}

