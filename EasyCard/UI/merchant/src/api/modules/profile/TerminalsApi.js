export default class TerminalsApi {
    constructor(base) {
        this.base = base;
        this.baseUrl = process.env.VUE_APP_PROFILE_API_BASE_ADDRESS;
        this.terminalsUrl = this.baseUrl + '/api/terminals';
    }

    async getTerminals(params) {
        if(this.$terminals){
            return this.$terminals;
        }
        this.$terminals = await this.base.get(this.terminalsUrl, params);
        return this.$terminals;
    }
}