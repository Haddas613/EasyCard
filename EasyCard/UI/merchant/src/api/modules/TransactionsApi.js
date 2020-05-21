export default class TransactionsApi{
    constructor(base){
        this.base = base;
        this.baseUrl = process.env.VUE_APP_TRANSACTIONS_API_BASE_ADDRESS;
        this.transactionsUrl = this.baseUrl + '/api/transactions';
    }

    async get(params){
        let data = await this.base.get(this.transactionsUrl, params);

        data.headers = await this.getHeaders()

        // TODO: data should be formatted based on headers data type
        return data;
    }

    async getHeaders() {
        if (this.headers) 
            return this.headers;

        let data = await this.base.get(this.transactionsUrl + '/$meta');
        this.headers = this.base._formatHeaders(data);
        return this.headers;
    }
}