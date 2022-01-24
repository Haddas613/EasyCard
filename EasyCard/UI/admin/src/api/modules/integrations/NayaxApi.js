export default class NayaxApi {
    constructor(base) {
        this.base = base;
        this.baseUrl = this.base.cfg.VUE_APP_MERCHANT_API_BASE_ADDRESS;
        this.nayaxIntegrationUrl = this.baseUrl + '/api/integrations/nayax';
    }
    
    async pairDevice(data){
        return this.base.post(this.nayaxIntegrationUrl + '/pair-device', data, { showSuccessToastr: false });
    }

    async authenticateDevice(data){
        return this.base.post(this.nayaxIntegrationUrl + '/authenticate-device', data);
    }

    async testConnection(data){
        return this.base.post(this.nayaxIntegrationUrl + `/test-connection`, data);
    }
}