export default class UPayApi {
    constructor(base) {
        this.base = base;
        this.baseUrl = this.base.cfg.VUE_APP_MERCHANT_API_BASE_ADDRESS;
        this.uPayIntegrationUrl = this.baseUrl + '/api/integrations/upay';
    }
    
    async testConnection(data){
        return this.base.post(this.uPayIntegrationUrl + `/test-connection`, data);
    }
}