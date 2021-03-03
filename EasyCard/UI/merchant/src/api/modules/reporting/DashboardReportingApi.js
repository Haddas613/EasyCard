export default class DashboardReportingApi {
    constructor(base) {
        this.base = base;
        this.baseUrl = this.base.cfg.VUE_APP_REPORT_API_BASE_ADDRESS;
        this.dashboardUrl = this.baseUrl + '/api/dashboard';
    }

    async getTransactionsTotals(params) {
        return await this.base.get(this.dashboardUrl + '/transactionsTotals', params);
    }

    async getPaymentTypeTotals(params) {
        return await this.base.get(this.dashboardUrl + '/paymentTypeTotals', params);
    }

    async getTransactionTimeline(params) {
        return await this.base.get(this.dashboardUrl + '/transactionTimeline', params);
    }

    async getItemsTotals(params) {
        return await this.base.get(this.dashboardUrl + '/itemsTotals', params);
    }

    async getConsumersTotals(params) {
        return await this.base.get(this.dashboardUrl + '/consumersTotals', params);
    }
}