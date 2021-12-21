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

    async getTerminalsRaw(params) {
        return await this.base.get(this.terminalsUrl, params);
    }

    async getTerminals(params, refreshCache = false) {
        if(!refreshCache && this.$terminals && !params){
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

    async getPrivateApiKey(terminalID){
        return this.base.get(this.terminalsUrl + `/${terminalID}/getApiKey`);
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

    async saveExternalSystem(terminalID, data, showSuccessToastr = true){
        return this.base.put(this.terminalsUrl + `/${terminalID}/externalsystem`, data, showSuccessToastr);
    }

    async deleteExternalSystem(terminalID, externalSystemID){
        return this.base.delete(this.terminalsUrl + `/${terminalID}/externalsystem/${externalSystemID}`);
    }

    async enableTerminal(terminalID){
        return this.base.put(this.terminalsUrl + `/${terminalID}/enable`);
    }

    async disableTerminal(terminalID){
        return this.base.put(this.terminalsUrl + `/${terminalID}/disable`);
    }

    async switchTerminalFeature(terminalID, featureID){
        return this.base.put(this.terminalsUrl + `/${terminalID}/switchfeature/${featureID}`);
    }

    async uploadMerchantLogo(terminalID, file){
        return this.base.postFile(this.terminalsUrl + `/${terminalID}/merchantlogo`, file);
    }

    async uploadCustomCSS(terminalID, file){
        return this.base.postFile(this.terminalsUrl + `/${terminalID}/customcss`, file);
    }

    async deleteCustomCSS(terminalID){
        return this.base.delete(this.terminalsUrl + `/${terminalID}/customcss`);
    }

    async deleteMerchantLogo(terminalID){
        return this.base.delete(this.terminalsUrl + `/${terminalID}/merchantlogo`);
    }
}