export default class TerminalsApi {
    constructor(base) {
        this.base = base;
        this.baseUrl = process.env.VUE_APP_MERCHANT_API_BASE_ADDRESS;
        this.terminalsUrl = this.baseUrl + '/api/terminals';
    }

    async get(params) {
        return await this.base.get(this.terminalsUrl, params);
    }
}