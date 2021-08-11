export default class MasavFilesApi {
    constructor(base) {
        this.base = base;
        this.baseUrl = this.base.cfg.VUE_APP_TRANSACTIONS_API_BASE_ADDRESS;
        this.masavUrl = this.baseUrl + '/api/masav';
    }

    async get(params) {
        if (!this.headers) {
            let data = await this.base.get(this.masavUrl + '/$meta')
            this.headers = this.base._formatHeaders(data)
            this.$headers = data.columns
        }

        let data = await this.base.get(this.masavUrl, params);

        if (!data || data.status === "error")
            return null;

        let dictionaries = await this.base.dictionaries.$getTransactionDictionaries();

        data.data = data.data.map(d => this.base.format(d, this.$headers, dictionaries))

        data.headers = this.headers;

        return data;
    }

    async getRows(params) {
        if (!this.rowheaders) {
            let data = await this.base.get(this.masavUrl + '/$meta-row')
            this.rowheaders = this.base._formatHeaders(data)
            this.$rowheaders = data.columns
        }
        let data = await this.base.get(this.masavUrl +'/rows', params);
        let dictionaries = await this.base.dictionaries.$getTransactionDictionaries();
        data.data = data.data.map(d => this.base.format(d, this.$rowheaders, dictionaries));
        data.headers = this.rowheaders;
        return data;
    }
}