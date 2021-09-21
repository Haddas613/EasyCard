export default class CardTokensReportingApi {
    constructor(base) {
        this.base = base;
        this.baseUrl = this.base.cfg.VUE_APP_REPORT_API_BASE_ADDRESS;
        this.tokensUrl = this.baseUrl + '/api/cardtokens';
    }
    async getTerminalsTokens(params) {

        if (!this.terminalsTokensHeaders) {
            let data = await this.base.get(this.tokensUrl + '/$meta-terminals-tokens')
            this.terminalsTokensHeaders = this.base._formatHeaders(data)
            this.$terminalsTokensHeaders = data.columns
        }

        let data = await this.base.get(this.tokensUrl + '/terminals-tokens', params);
        
        if(!data || data.status === "error")
            return null;

        let dictionaries = await this.base.dictionaries.$getMerchantDictionaries();
        
        data.data = data.data.map(d => this.base.format(d, this.$terminalsTokensHeaders, dictionaries))

        data.headers = this.terminalsTokensHeaders;

        return data;
    }

    async getTokensTransactions(params) {

        if (!this.tokensTransactionsHeaders) {
            let data = await this.base.get(this.tokensUrl + '/$meta-tokens-transactions')
            this.tokensTransactionsHeaders = this.base._formatHeaders(data)
            this.$tokensTransactionsHeaders = data.columns
        }

        let data = await this.base.get(this.tokensUrl + '/tokens-transactions', params);
        
        if(!data || data.status === "error")
            return null;

        let dictionaries = await this.base.dictionaries.$getMerchantDictionaries();
        
        data.data = data.data.map(d => this.base.format(d, this.$tokensTransactionsHeaders, dictionaries))

        data.headers = this.tokensTransactionsHeaders;

        return data;
    }
}