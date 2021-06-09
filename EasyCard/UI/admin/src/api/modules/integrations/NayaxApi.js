export default class NayaxApi {
    constructor(base) {
        this.base = base;
        this.baseUrl = this.base.cfg.VUE_APP_MERCHANT_API_BASE_ADDRESS;
        this.easyInvoiceIntegrationUrl = this.baseUrl + '/api/integrations/nayax';
    }
    
    async pairDevice(data){
        return this.base.post(this.easyInvoiceIntegrationUrl + '/pair-device', data, false);
    }

    async authenticateDevice(data){
        return this.base.post(this.easyInvoiceIntegrationUrl + '/authenticate-device', data);
    }
}