export default class BillingDealsApi {
    constructor(base) {
        this.base = base;
        this.baseUrl = this.base.cfg.VUE_APP_TRANSACTIONS_API_BASE_ADDRESS;
        this.billingUrl = this.baseUrl + '/api/billing';
    }

    async get(params) {

        if (!this.headers) {
            let data = await this.base.get(this.billingUrl + '/$meta')
            this.headers = this.base._formatHeaders(data)
            this.$headers = data.columns
        }

        let data = await this.base.get(this.billingUrl, params);

        if (!data || data.status === "error")
            return null;

        let dictionaries = await this.base.dictionaries.$getTransactionDictionaries();

        data.data = data.data.map(d => this.base.format(d, this.$headers, dictionaries))

        data.headers = this.headers;

        return data;
    }

    async getBillingDeal(id, doNotFormatData = false) {
        if (!this.headers) {
            let data = await this.base.get(this.billingUrl + '/$meta')
            this.headers = this.base._formatHeaders(data)
            this.$headers = data.columns
        }
        let billingDeal = await this.base.get(this.billingUrl + `/${id}`);
        let dictionaries = await this.base.dictionaries.$getTransactionDictionaries();

        if (doNotFormatData) {
            return billingDeal;
        }
        billingDeal = this.base.format(billingDeal, this.$headers, dictionaries)
        return billingDeal;
    } 

    async getHistory(billingDealId){
        return await this.base.get(this.billingUrl + `/${billingDealId}/history`);
    }

    async pauseBillingDeal(id, data) {
        return await this.base.post(this.billingUrl + `/${id}/pause`, data);
    }

    async unpauseBillingDeal(id) {
        return await this.base.post(this.billingUrl + `/${id}/unpause`);
    }

    async triggerBillingDealsByTerminal(terminalID) {
        return await this.base.post(this.billingUrl + `/trigger-by-terminal/${terminalID}`);
    }
    
    async triggerBillingDeals(billingDealsID) {
        return await this.base.post(this.billingUrl + '/trigger-billing-deals', { billingDealsID });
    } 

    async disableBillingDeals(billingDealsID) {
        return await this.base.post(this.billingUrl + '/disable-billing-deals', { billingDealsID });
    } 

    async activateBillingDeals(billingDealsID) {
        return await this.base.post(this.billingUrl + '/activate-billing-deals', { billingDealsID });
    } 
}