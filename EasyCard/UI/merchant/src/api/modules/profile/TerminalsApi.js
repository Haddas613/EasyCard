export default class TerminalsApi {
    constructor(base) {
        this.base = base;
        this.baseUrl = this.base.cfg.VUE_APP_PROFILE_API_BASE_ADDRESS;
        this.terminalsUrl = this.baseUrl + '/api/terminals';
    }

    async getTerminals(params, opts) {
        opts = {
            refreshCache: false,
            showDeleted: false,
            ...opts
        };
        
        if(opts.refreshCache || !this.$terminals){
            this.$terminals = (await this.base.get(this.terminalsUrl, params)) || [];
        }
        var t = opts.showDeleted ? this.$terminals.data : this.$terminals.data.filter(t => t.status != "disabled");
        return { numberOfRecords: t.length, data: t};
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

    async getAvailableFeatures(){
        return await this.base.get(this.terminalsUrl + '/available-features');
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

    async getTerminalDevices(terminalID){
        return await this.base.get(this.terminalsUrl + `/terminal-devices/${terminalID}`);
    }
}