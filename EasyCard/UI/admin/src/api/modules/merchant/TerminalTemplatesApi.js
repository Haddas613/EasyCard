export default class TerminalsApi {
    constructor(base) {
        this.base = base;
        this.baseUrl = process.env.VUE_APP_MERCHANT_API_BASE_ADDRESS;
        this.templatesUrl = this.baseUrl + '/api/terminal-templates';
    }

    async get(params) {
        if (!this.headers) {
            let data = await this.base.get(this.templatesUrl + '/$meta')
            this.headers = this.base._formatHeaders(data);
            this.$headers = data.columns
        }

        let data = await await this.base.get(this.templatesUrl, params);

        let dictionaries = await this.base.dictionaries.$getMerchantDictionaries()

        data.data = data.data.map(d => this.base.format(d, this.$headers, dictionaries))

        data.headers = this.headers

        return data;
    }

    async getTerminalTemplate(terminalID) {
        return this.base.get(this.templatesUrl + '/' + terminalID);
    }

    async createTerminalTemplate(data){
        return this.base.post(this.templatesUrl, data);
    }
}