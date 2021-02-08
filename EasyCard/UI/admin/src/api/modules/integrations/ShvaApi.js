export default class ShvaApi {
    constructor(base) {
        this.base = base;
        this.baseUrl = process.env.VUE_APP_MERCHANT_API_BASE_ADDRESS;
        this.shvaIntegrationUrl = this.baseUrl + '/api/integrations/shva';
    }
    
    async setNewPassword(data){
        return this.base.post(this.shvaIntegrationUrl + `/new-password`, data);
    }

    async testConnection(data){
        return this.base.post(this.shvaIntegrationUrl + `/test-connection`, data);
    }
}