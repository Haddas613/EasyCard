import Vue from 'vue'
import TransactionsApi from './modules/transactions/TransactionsApi';
import DictionariesApi from './modules/common/DictionariesApi';
import TerminalsApi from './modules/profile/TerminalsApi';
import ConsumersApi from './modules/profile/ConsumersApi';
import ItemsApi from './modules/profile/ItemsApi';
import moment from 'moment'
import store from '../store/index';
import i18n from '../i18n'
import CardTokensApi from './modules/transactions/CardTokensApi';
import TransmissionsApi from './modules/transactions/TransmissionsApi';
import BillingDealsApi from './modules/transactions/BillingDealsApi';
import InvoicingApi from './modules/transactions/InvoicingApi';
import PaymentRequestsApi from './modules/transactions/PaymentRequestsApi';

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
        this.transmissions = new TransmissionsApi(this);
        this.billingDeals = new BillingDealsApi(this);
        this.invoicing = new InvoicingApi(this);
        this.paymentRequests = new PaymentRequestsApi(this);
    }

    /** Get requests are syncronized based on their url and query string to prevent the same requests be fired at the same time */
    async get(url, params) {
        const access_token = await this.oidc.getAccessToken()

        if (!access_token) {
            Vue.toasted.show(i18n.t('SessionExpired'), { type: 'error' });
            this.oidc.signOut();
            return null;
        }
        if (params) {
            if (params.page && params.itemsPerPage) {
                params.take = params.itemsPerPage;
                params.skip = params.itemsPerPage * (params.page - 1);
            }
            /**Clear up empty params */
            for (var prop of Object.keys(params)) {
                if (!params[prop]) {
                    delete params[prop];
                }
            }
        }
        let _urlKey = url;

        let requestUrl = new URL(url)
        if (params) {
            requestUrl.search = new URLSearchParams(params).toString();
            _urlKey += requestUrl.search;
        }

        if (this._ongoingRequests[_urlKey]) {
            return this._ongoingRequests[_urlKey];
        }

        let request = fetch(requestUrl, {
            method: 'GET',
            withCredentials: true,
            mode: 'cors',
            headers: this._buildRequestHeaders(access_token)
        });
        this._ongoingRequests[_urlKey] = this._handleRequest(request);

        return new Promise((s, e) => s(this._ongoingRequests[_urlKey])).finally(() => delete this._ongoingRequests[_urlKey])
    }

    async post(url, payload) {
        const access_token = await this.oidc.getAccessToken()

        if (!access_token) {
            Vue.toasted.show(i18n.t('SessionExpired'), { type: 'error' });
            this.oidc.signOut();
            return null;
        }

        let requestUrl = new URL(url);
        let request = fetch(requestUrl, {
            method: 'POST',
            withCredentials: true,
            mode: 'cors',
            headers: this._buildRequestHeaders(access_token),
            body: JSON.stringify(payload)
        });

        return this._handleRequest(request, true);
    }

    async put(url, payload) {
        const access_token = await this.oidc.getAccessToken()

        if (!access_token) {
            Vue.toasted.show(i18n.t('SessionExpired'), { type: 'error' });
            this.oidc.signOut();
            return null;
        }

        let requestUrl = new URL(url);
        let request = fetch(requestUrl, {
            method: 'PUT',
            withCredentials: true,
            mode: 'cors',
            headers: this._buildRequestHeaders(access_token),
            body: JSON.stringify(payload)
        });

        return this._handleRequest(request, true);
    }

    async delete(url) {
        const access_token = await this.oidc.getAccessToken()

        if (!access_token) {
            Vue.toasted.show(i18n.t('SessionExpired'), { type: 'error' });
            this.oidc.signOut();
            return null;
        }

        let requestUrl = new URL(url);
        let request = fetch(requestUrl, {
            method: 'DELETE',
            withCredentials: true,
            mode: 'cors',
            headers: this._buildRequestHeaders(access_token)
        });

        return this._handleRequest(request, true);
    }

    async _handleRequest(request, showSuccessToastr = false) {
        try {
            store.commit("ui/requestsCountIncrement");
            request = await request;

            if (request.ok) {
                let result = await request.json();
                if (result.status === "warning") {
                    Vue.toasted.show(result.message, { type: 'info' });
                } else if (showSuccessToastr && result.status === "success") {
                    Vue.toasted.show(result.message, { type: 'success', duration: 5000 });
                }
                await this._checkAppVersion(request.headers);
                return result;
            } else {
                //Server Validation errors are returned to component
                if (request.status === 400 || request.status === 409) {
                    let result = await request.json();
                    return result;
                } else if (request.status === 401) {
                    Vue.toasted.show(i18n.t('SessionExpired'), { type: 'error' });
                    //await this.oidc.signOut();
                    this.oidc.signinRedirect(location.href)
                    return null;
                } else if (request.status === 404) {
                    Vue.toasted.show(i18n.t('NotFound'), { type: 'error' });
                    return null;
                } else {
                    Vue.toasted.show(i18n.t('ServerErrorTryAgainLater'), { type: 'error' });
                    return null;
                }
            }

        } catch (err) {
            Vue.toasted.show(i18n.t('ServerErrorTryAgainLater'), { type: 'error' });
        } finally {
            store.commit("ui/requestsCountDecrement");
        }
        return null;
    }

    _buildRequestHeaders(access_token) {
        const locale = (store.state.localization && store.state.localization.currentLocale)
            ? store.state.localization.currentLocale : process.env.VUE_APP_I18N_LOCALE;

        return {
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${access_token}`,
            'Accept': 'application/json',
            'Accept-Language': `${locale}`
        }
    }

    _formatHeaders(headers) {
        return Object.keys(headers.columns).map(key => { return { value: key, text: headers.columns[key].name } });
    }

    async _checkAppVersion(responseHeaders) {
        if (this.lastCheckTimestamp && this.lastCheckTimestamp.getTime() > (new Date()).setMinutes(-5)) {
            return;
        }

        let responseHeaderVal = responseHeaders.get('x-version');
        if(!responseHeaderVal){
            return;
        }

        if (responseHeaderVal.toLowerCase().trim() != process.env.VUE_APP_VERSION.toLowerCase().trim()) {
            store.commit("ui/setVersionMismatch", true);
        }else{
            store.commit("ui/setVersionMismatch", false);
        }
        this.lastCheckTimestamp = new Date();
    }

    format(d, headers, dictionaries) {
        for (const property in d) {
            let v = d[property]
            let h = headers[property]
            if (!h) continue;
            if (h.dataType == 'guid' && v && v.length > 8) {
                d[`$${property}`] = v
                d[property] = v.substring(0, 8)
            } else if (h.dataType == 'dictionary') {
                d[`$${property}`] = v
                d[property] = dictionaries[h.dictionary][v]
            }
            // else if (h.dataType == 'money') {
            //     d[`$${property}`] = v
            //     d[property] = new Intl.NumberFormat().format(v) // TODO: locale, currency symbol
            // }
            else if (h.dataType == 'date' && v) {
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