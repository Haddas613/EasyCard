export default class ClearingHouseApi {
    constructor(base) {
        this.base = base;
        this.baseUrl = this.base.cfg.VUE_APP_MERCHANT_API_BASE_ADDRESS;
        this.clearingHouseIntegrationUrl = this.baseUrl + '/api/integrations/clearing-house';
    }
    
    async getCustomerData(params){
        return this.base.get(this.clearingHouseIntegrationUrl + `/merchants`, params);
    }
}