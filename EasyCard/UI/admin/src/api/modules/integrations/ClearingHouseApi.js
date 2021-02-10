export default class ClearingHouseApi {
    constructor(base) {
        this.base = base;
        this.baseUrl = process.env.VUE_APP_MERCHANT_API_BASE_ADDRESS;
        this.clearingHouseIntegrationUrl = this.baseUrl + '/api/integrations/clearing-house';
    }
    
    async getCustomerData(customerID){
        return this.base.get(this.clearingHouseIntegrationUrl + `/get-customer/${customerID}`);
    }
}