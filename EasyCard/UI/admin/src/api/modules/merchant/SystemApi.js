export default class UsersApi {
    constructor(base) {
        this.base = base;
        this.baseUrl = process.env.VUE_APP_MERCHANT_API_BASE_ADDRESS;
        this.systemUrl = this.baseUrl + '/api/system';
    }

    async getSystemLogs(params) {
        if (!this.headers) {
            let data = await this.base.get(this.systemUrl + '/$meta')
            this.headers = this.base._formatHeaders(data);
            this.$headers = data.columns
        }

        let data = await await this.base.get(this.systemUrl + '/log', params);

        let dictionaries = await this.base.dictionaries.$getMerchantDictionaries()

        data.data = data.data.map(d => this.base.format(d, this.$headers, dictionaries))

        data.headers = this.headers

        return data;
    }
}