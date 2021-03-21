export default class CardTokensApi {
    constructor(base) {
        this.base = base;
        this.baseUrl = this.base.cfg.VUE_APP_TRANSACTIONS_API_BASE_ADDRESS;
        this.cardTokensUrl = this.baseUrl + '/api/cardtokens';
    }

    async get(params) {

        if (!this.headers) {
            let data = await this.base.get(this.cardTokensUrl + '/$meta')
            this.headers = this.base._formatHeaders(data)
            this.$headers = data.columns
        }

        let data = await this.base.get(this.cardTokensUrl, params);
        
        if(!data || data.status === "error")
            return null;

        let dictionaries = await this.base.dictionaries.$getTransactionDictionaries();
        
        data.data = data.data.map(d => this.base.format(d, this.$headers, dictionaries))

        data.headers = this.headers;

        return data;
    }

    deleteCardToken(tokenId){
        return this.base.delete(this.cardTokensUrl + `/${tokenId}`);
    }
}