export default class AuditApi {
    constructor(base) {
        this.base = base;
        this.baseUrl = this.base.cfg.VUE_APP_MERCHANT_API_BASE_ADDRESS;
        this.auditUrl = this.baseUrl + '/api/audit';
    }

    async get(params) {
        if (!this.headers) {
            let data = await this.base.get(this.auditUrl + '/$meta')
            this.headers = this.base._formatHeaders(data);
            this.$headers = data.columns
        }

        let data = await await this.base.get(this.auditUrl, params);

        let dictionaries = await this.base.dictionaries.$getMerchantDictionaries()

        data.data = data.data.map(d => this.base.format(d, this.$headers, dictionaries))

        data.headers = this.headers

        return data;
    }
}