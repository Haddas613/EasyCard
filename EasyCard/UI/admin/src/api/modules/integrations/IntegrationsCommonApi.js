export default class IntegrationsCommonApi {
    constructor(base) {
        this.base = base;
        this.baseUrl = this.base.cfg.VUE_APP_MERCHANT_API_BASE_ADDRESS;
        this.integrationUrl = this.baseUrl + '/api/integrations/';
    }
    
    async getIntegrationLogs(integration, entityID){
        if (!this.headers) {
            let data = await this.base.get(this.baseUrl + '/api/system/$meta-integration-logs')
            this.headers = this.base._formatHeaders(data);
            this.$headers = data.columns
        }

        var data = await this.base.get(this.integrationUrl + `${integration}/request-logs/${entityID}`);

        if(!data || data.status === "error")
            return null;

        let dictionaries = await this.base.dictionaries.$getMerchantDictionaries();
        data.data = data.data.map(d => this.base.format(d, this.$headers, dictionaries))

        data.headers = this.headers;

        return data;
    }
}