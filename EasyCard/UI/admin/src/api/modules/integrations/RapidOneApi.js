export default class RapidOneApi {
    constructor(base) {
        this.base = base;
        this.baseUrl = this.base.cfg.VUE_APP_MERCHANT_API_BASE_ADDRESS;
        this.rapidOneIntegrationUrl = this.baseUrl + '/api/integrations/rapidone';
    }
    
    async getCompanies(baseurl, token){
        return this.base.get(this.rapidOneIntegrationUrl + `/companies`, { baseurl, token });
    }

    async getBranches(baseurl, token){
        return this.base.get(this.rapidOneIntegrationUrl + `/branches`, { baseurl, token });
    }

    async getDepartments(baseurl, token, branchid){
        return this.base.get(this.rapidOneIntegrationUrl + `/departments`, { baseurl, token, branchid });
    }

    async testConnection(data){
        return this.base.post(this.rapidOneIntegrationUrl + `/test-connection`, data);
    }
}