export default class EasyInvoiceApi {
    constructor(base) {
        this.base = base;
        this.baseUrl = this.base.cfg.VUE_APP_MERCHANT_API_BASE_ADDRESS;
        this.easyInvoiceIntegrationUrl = this.baseUrl + '/api/integrations/easy-invoice';
    }
    
    async createCustomer(data){
        return this.base.post(this.easyInvoiceIntegrationUrl + `/create-customer`, data);
    }
}