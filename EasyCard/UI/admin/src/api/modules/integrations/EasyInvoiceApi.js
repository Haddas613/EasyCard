export default class EasyInvoiceApi {
    constructor(base) {
        this.base = base;
        this.baseUrl = this.base.cfg.VUE_APP_MERCHANT_API_BASE_ADDRESS;
        this.easyInvoiceIntegrationUrl = this.baseUrl + '/api/integrations/easy-invoice';
    }
    
    async createCustomer(data){
        return this.base.post(this.easyInvoiceIntegrationUrl + `/create-customer`, data, { showSuccessToastr: false });
    }

    async getDocumentTypes(){
        return this.base.get(this.easyInvoiceIntegrationUrl + '/get-document-types');
    }

    async getDocumentNumber(terminalID, type){
        return this.base.get(this.easyInvoiceIntegrationUrl + '/get-document-number', { terminalID, docType: type });
    }

    async setDocumentNumber(data){
        return this.base.post(this.easyInvoiceIntegrationUrl + `/set-document-number`, data);
    }

    async testConnection(data){
        return this.base.post(this.easyInvoiceIntegrationUrl + `/test-connection`, data);
    }

    async getLanguages(){
        return this.base.get(this.easyInvoiceIntegrationUrl + `/languages`);
    }
}