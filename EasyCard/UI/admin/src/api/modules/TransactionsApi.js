export default class TransactionsApi {
    constructor(base) {
        this.base = base;
        this.baseUrl = process.env.VUE_APP_TRANSACTIONS_API_BASE_ADDRESS;
        this.transactionsUrl = this.baseUrl + '/api/transactions';
    }

    async get(params) {

        if (!this.headers) {
            let data = await this.base.get(this.transactionsUrl + '/$meta')
            this.headers = this.base.formatHeaders(data)
            this.$headers = data.columns
        }

        let data = await this.base.get(this.transactionsUrl, params)

        let dictionaries = await this.base.dictionaries.$getTransactionDictionaries()

        data.data = data.data.map(d => this.base.formatColumns(d, this.$headers, dictionaries))

        data.headers = this.headers

        return data;
    }
}