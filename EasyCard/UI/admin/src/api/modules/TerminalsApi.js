export default class TerminalsApi {
    constructor(base) {
        this.base = base;
        this.baseUrl = process.env.VUE_APP_MERCHANT_API_BASE_ADDRESS;
        this.terminalsUrl = this.baseUrl + '/api/terminals';
    }

    async get(params) {
        if (!this.headers) {
            let data = await this.base.get(this.terminalsUrl + '/$meta')
            this.headers = this.base.formatHeaders(data)
            this.$headers = data.columns
        }

        let data = await await this.base.get(this.terminalsUrl, params);

        let dictionaries = await this.base.dictionaries.$getMerchantDictionaries()

        data.data = data.data.map(d => this.base.formatColumns(d, this.$headers, dictionaries))

        data.headers = this.headers

        return data;
    }
}