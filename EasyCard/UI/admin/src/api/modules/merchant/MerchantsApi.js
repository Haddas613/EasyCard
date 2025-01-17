export default class MerchantsApi {
    constructor(base) {
        this.base = base;
        this.baseUrl = this.base.cfg.VUE_APP_MERCHANT_API_BASE_ADDRESS;
        this.merchantsUrl = this.baseUrl + '/api/merchant';
    }

    async get(params) {
        if (!this.headers) {
            let data = await this.base.get(this.merchantsUrl + '/$meta')
            this.headers = this.base._formatHeaders(data);
            this.$headers = data.columns
        }

        let data = await await this.base.get(this.merchantsUrl, params);

        let dictionaries = await this.base.dictionaries.$getMerchantDictionaries()

        data.data = data.data.map(d => this.base.format(d, this.$headers, dictionaries))

        data.headers = this.headers

        return data;
    }

    async getMerchantsRaw(params) {
        return await this.base.get(this.merchantsUrl, params);
    }

    async getMerchants(params, refreshCache = false) {
        if(!refreshCache && this.$merchants){
            return this.$merchants;
        }
        this.$merchants = await this.base.get(this.merchantsUrl, params);
        return this.$merchants;
    }

    async getMerchant(merchantID) {
        return this.base.get(this.merchantsUrl + '/' + merchantID);
    }

    async createMerchant(data){
        return this.base.post(this.merchantsUrl, data);
    }

    async updateMerchant(data){
        return this.base.put(this.merchantsUrl + '/' + data.merchantID, data);
    }

    async resetPrivateApiKey(merchantID){
        return this.base.post(this.merchantsUrl + `/${merchantID}/resetApiKey`);
    }

    async resetSharedApiKey(merchantID){
        return this.base.post(this.merchantsUrl + `/${merchantID}/resetSharedApiKey`);
    }

    async loginAsMerchant(merchantID){
        return this.base.post(this.merchantsUrl + `/${merchantID}/loginAsMerchant`, null, { showSuccessToastr: false });
    }
}