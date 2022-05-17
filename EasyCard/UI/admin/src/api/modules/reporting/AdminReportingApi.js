export default class AdminReportingApi {
    constructor(base) {
        this.base = base;
        this.baseUrl = this.base.cfg.VUE_APP_REPORT_API_BASE_ADDRESS;
        this.adminUrl = this.baseUrl + '/api/admin';
    }

    async getSmsTimelines(params) {
        return await this.base.get(this.adminUrl + '/sms-timelines', params);
    }

    async getTransactionsTotals(params) {
        return await this.base.get(this.adminUrl + '/transactions-totals', params);
    }
    
    async getMerchantsTotals(params) {
        return await this.base.get(this.adminUrl + '/merchants-totals', params);
    }

    async get3DSChallengeReport(params) {
        if (!this.tdsHeaders) {
            let data = await this.base.get(this.adminUrl + '/$meta-tds-challenge-report')
            this.tdsHeaders = this.base._formatHeaders(data)
            this.$tdsHeaders = data.columns
        }

        let data = await this.base.get(this.adminUrl + '/tds-challenge-report', params);

        if (!data || data.status === "error")
            return null;

        let dictionaries = await this.base.dictionaries.$getTransactionDictionaries();

        data.data = data.data.map(d => this.base.format(d, this.$tdsHeaders, dictionaries))

        data.headers = this.tdsHeaders;

        return data;
    }
}