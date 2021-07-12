import Vue from 'vue'
import TransactionsApi from './modules/transactions/TransactionsApi';
import DictionariesApi from './modules/common/DictionariesApi';
import TerminalsApi from './modules/merchant/TerminalsApi';
import moment from 'moment'
import store from '../store/index';
import i18n from '../i18n'
import BillingDealsApi from './modules/transactions/BillingDealsApi';
import InvoicingApi from './modules/transactions/InvoicingApi';
import PaymentRequestsApi from './modules/transactions/PaymentRequestsApi';
import MerchantsApi from './modules/merchant/MerchantsApi';
import UsersApi from './modules/merchant/UsersApi';
import SystemApi from './modules/merchant/SystemApi';
import AuditApi from './modules/merchant/AuditApi';
import TerminalTemplatesApi from './modules/merchant/TerminalTemplatesApi';
import ShvaApi from './modules/integrations/ShvaApi';
import EasyInvoiceApi from './modules/integrations/EasyInvoiceApi';
import ClearingHouseApi from './modules/integrations/ClearingHouseApi';
import TransmissionApi from './modules/transactions/TransmissionsApi'
import cfg from "../app.config";
import appConstants from "../helpers/app-constants";
import CardTokensApi from './modules/transactions/CardTokensApi';
import NayaxApi from "./modules/integrations/NayaxApi";

class ApiBase {
    constructor() {
        this.oidc = Vue.prototype.$oidc;
        this._ongoingRequests = {};
        this.cfg = cfg;

        /**Apis */
        this.transactions = new TransactionsApi(this);
        this.dictionaries = new DictionariesApi(this);
        this.terminals = new TerminalsApi(this);
        this.merchants = new MerchantsApi(this);
        this.billingDeals = new BillingDealsApi(this);
        this.invoicing = new InvoicingApi(this);
        this.paymentRequests = new PaymentRequestsApi(this);
        this.users = new UsersApi(this);
        this.terminalTemplates = new TerminalTemplatesApi(this);
        this.system = new SystemApi(this);
        this.audit = new AuditApi(this);
        this.transmissions = new TransmissionApi(this);
        this.cardTokens = new CardTokensApi(this);
        this.integrations = {
            shva: new ShvaApi(this),
            easyInvoice: new EasyInvoiceApi(this),
            clearingHouse: new ClearingHouseApi(this),
            nayax: new NayaxApi(this)
        };
    }

    /** Get requests are syncronized based on their url and query string to prevent the same requests be fired at the same time */
    async get(url, params) {
        const access_token = await this.oidc.getAccessToken()
        
        if (!access_token) {
            Vue.toasted.show(i18n.t('SessionExpired'), { type: 'error' });
            // this.oidc.signOut();
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
            headers: this._buildRequestHeaders(access_token),
        });
        this._ongoingRequests[_urlKey] = this._handleRequest(request);

        return new Promise((s, e) => s(this._ongoingRequests[_urlKey])).finally(() => delete this._ongoingRequests[_urlKey])
    }

    async post(url, payload, showSuccessToastr = true) {
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

        return this._handleRequest(request, showSuccessToastr);
    }

    async postFile(url, file) {
        const access_token = await this.oidc.getAccessToken()

        if (!access_token) {
            Vue.toasted.show(i18n.t('SessionExpired'), { type: 'error' });
            this.oidc.signOut();
            return null;
        }

        let requestUrl = new URL(url);
        
        var headers = this._buildRequestHeaders(access_token);
        delete headers['Content-Type'];
        
        const formData = new FormData();
        formData.append('file', file);

        let request = fetch(requestUrl, {
            method: 'POST',
            withCredentials: true,
            mode: 'cors',
            headers: headers,
            body: formData
        });

        return this._handleRequest(request, true);
    }

    async put(url, payload, showSuccessToastr = true) {
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

        return this._handleRequest(request, showSuccessToastr);
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
            headers: this._buildRequestHeaders(access_token),
        });

        return this._handleRequest(request, true);
    }

    async _handleRequest(request, showSuccessToastr = false) {
        let requestIncrementTimeout = null;
        let storeDispatched = false;
        try {
            requestIncrementTimeout = setTimeout(() => {
                store.commit("ui/requestsCountIncrement");
                storeDispatched = true;
            }, 1500);
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
                    Vue.toasted.show(result.message || i18n.t('SomethingWentWrong'), { type: 'error' });
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
                    return { status: "error", message: i18n.t('ServerErrorTryAgainLater') };
                }
            }

        } catch (err) {
            Vue.toasted.show(i18n.t('ServerErrorTryAgainLater'), { type: 'error' });
        } finally {
            if(requestIncrementTimeout){
                clearTimeout(requestIncrementTimeout);
            }
            if(storeDispatched){
                store.commit("ui/requestsCountDecrement");
            }
        }
        return null;
    }

    _buildRequestHeaders(access_token){
        const locale = (store.state.localization && store.state.localization.currentLocale) 
            ? store.state.localization.currentLocale : this.cfg.VUE_APP_I18N_LOCALE;

        let headers =  {
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${access_token}`,
            'Accept': 'application/json',
            'Accept-Language': `${locale}`,
        }

        if(this.cfg.VUE_APP_VERSION){
            headers['X-Version'] = this.cfg.VUE_APP_VERSION;
        }

        return headers;
    }

    _formatHeaders(headers) {
        return Object.keys(headers.columns).map(key => { return { value: key, text: headers.columns[key].name } });
    }

    async _checkAppVersion(responseHeaders) {
        if ((cfg.VUE_APP_VERSION == appConstants.misc.uiDefaultVersion) || this.lastCheckTimestamp && this.lastCheckTimestamp.getTime() > (new Date()).setMinutes(-5)) {
            return;
        }

        let responseHeaderVal = responseHeaders.get('x-ui-version');

        if(!responseHeaderVal || responseHeaderVal == appConstants.misc.uiDefaultVersion){
            return;
        }

        if (responseHeaderVal.toLowerCase().trim() != this.cfg.VUE_APP_VERSION.toLowerCase().trim()) {
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
                d[property] = moment(String(v)).format('DD/MM/YYYY') // TODO: locale
            }
        }
        return d
    }
}

export default {
    install: function(Vue, ) {
        Object.defineProperty(Vue.prototype, '$api', { value: new ApiBase() });
    }
}