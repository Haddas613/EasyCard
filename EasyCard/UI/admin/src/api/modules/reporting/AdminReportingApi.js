export default class AdminReportingApi {
    constructor(base) {
        this.base = base;
        this.baseUrl = this.base.cfg.VUE_APP_REPORT_API_BASE_ADDRESS;
        this.adminUrl = this.baseUrl + '/api/admin';
    }

    async getSmsTimelines(params) {
        return await this.base.get(this.adminUrl + '/getSmsTimelines', params);
    }
}