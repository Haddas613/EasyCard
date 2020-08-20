export default class MerchantsApi {
    constructor(base) {
        this.base = base;
        this.baseUrl = process.env.VUE_APP_MERCHANT_API_BASE_ADDRESS;
        this.merchantsUrl = this.baseUrl + '/api/merchant';
    }

    async get(params) {
        if (!this.headers) {
            let data = await this.base.get(this.merchantsUrl + '/$meta')
            this.headers = this.base.formatHeaders(data)
            this.$headers = data.columns
        }

        let data = await this.base.get(this.merchantsUrl, params);

        let dictionaries = await this.base.dictionaries.$getMerchantDictionaries()

        data.data = data.data.map(d => this.base.formatColumns(d, this.$headers, dictionaries))

       data.headers = this.headers

        return data;
    }
}