export default class TerminalsApi {
    constructor(base) {
        this.base = base;
        this.baseUrl = this.base.cfg.VUE_APP_MERCHANT_API_BASE_ADDRESS;
        this.terminalsUrl = this.baseUrl + '/api/terminals';
    }

    async get(params) {
        if (!this.headers) {
            let data = await this.base.get(this.terminalsUrl + '/$meta')
            this.headers = this.base._formatHeaders(data);
            this.$headers = data.columns
        }

        let data = await await this.base.get(this.terminalsUrl, params);

        let dictionaries = await this.base.dictionaries.$getMerchantDictionaries()

        data.data = data.data.map(d => this.base.format(d, this.$headers, dictionaries))

        data.headers = this.headers

        return data;
    }

    async getTerminals(params, refreshCache = false) {
        if(!refreshCache && this.$terminals){
            return this.$terminals;
        }
        this.$terminals = await this.base.get(this.terminalsUrl, params);
        return this.$terminals;
    }

    async getTerminal(terminalID) {
        return this.base.get(this.terminalsUrl + '/' + terminalID);
    }

    async createTerminal(data){
        return this.base.post(this.terminalsUrl, data);
    }

    async updateTerminal(data){
        return this.base.put(this.terminalsUrl + '/' + data.terminalID, data);
    }

    async resetPrivateApiKey(terminalID){
        return this.base.post(this.terminalsUrl + `/${terminalID}/resetApiKey`);
    }

    async resetSharedApiKey(terminalID){
        return this.base.post(this.terminalsUrl + `/${terminalID}/resetSharedApiKey`);
    }

    async getAvailableIntegrations(data){
        return await this.base.get(this.terminalsUrl + '/available-integrations', data);
    }

    async getAvailableFeatures(){
        return await this.base.get(this.terminalsUrl + '/available-features');
    }

    async saveExternalSystem(terminalID, data){
        return this.base.put(this.terminalsUrl + `/${terminalID}/externalsystem`, data);
    }

    async deleteExternalSystem(terminalID, externalSystemID){
        return this.base.delete(this.terminalsUrl + `/${terminalID}/externalsystem/${externalSystemID}`);
    }

    async enableTerminal(terminalTemplateID){
        return this.base.put(this.terminalsUrl + `/${terminalTemplateID}/enable`);
    }

    async disableTerminal(terminalTemplateID){
        return this.base.put(this.terminalsUrl + `/${terminalTemplateID}/disable`);
    }
}