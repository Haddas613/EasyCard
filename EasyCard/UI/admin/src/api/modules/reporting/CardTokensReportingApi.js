export default class CardTokensReportingApi {
    constructor(base) {
        this.base = base;
        this.baseUrl = this.base.cfg.VUE_APP_REPORT_API_BASE_ADDRESS;
        this.tokensUrl = this.baseUrl + '/api/cardtokens';
    }
    async getTerminalsTokens(params) {

        if (!this.headers) {
            let data = await this.base.get(this.tokensUrl + '/$meta-terminals-tokens')
            this.headers = this.base._formatHeaders(data)
            this.$headers = data.columns
        }

        let data = await this.base.get(this.tokensUrl + '/terminals-tokens', params);
        
        if(!data || data.status === "error")
            return null;

        let dictionaries = await this.base.dictionaries.$getMerchantDictionaries();
        
        data.data = data.data.map(d => this.base.format(d, this.$headers, dictionaries))

        data.headers = this.headers;

        return data;
    }
}