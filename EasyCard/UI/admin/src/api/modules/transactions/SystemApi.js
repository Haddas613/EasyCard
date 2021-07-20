export default class SystemApi {
    constructor(base) {
        this.base = base;
        this.baseUrl = this.base.cfg.VUE_APP_TRANSACTIONS_API_BASE_ADDRESS;
        this.systemUrl = this.baseUrl + '/api/system';
    }

    async getCorrelationLog(date, correlationId) {
        return await this.base.get(this.systemUrl + '/log', {date, correlationId});
    }
}