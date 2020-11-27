export default class TerminalsApi {
    constructor(base) {
        this.base = base;
        this.baseUrl = process.env.VUE_APP_PROFILE_API_BASE_ADDRESS;
        this.terminalsUrl = this.baseUrl + '/api/terminals';
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

    async updateTerminal(data){
        return this.base.put(this.terminalsUrl + '/' + data.terminalID, data);
    }

    async resetPrivateApiKey(terminalID){
        return this.base.post(this.terminalsUrl + `/${terminalID}/resetApiKey`);
    }

    async resetSharedApiKey(terminalID){
        return this.base.post(this.terminalsUrl + `/${terminalID}/resetSharedApiKey`);
    }
}