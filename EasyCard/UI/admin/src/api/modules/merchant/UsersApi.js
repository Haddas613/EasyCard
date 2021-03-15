export default class UsersApi {
    constructor(base) {
        this.base = base;
        this.baseUrl = this.base.cfg.VUE_APP_MERCHANT_API_BASE_ADDRESS;
        this.usersUrl = this.baseUrl + '/api/user';
    }

    async get(params) {
        if (!this.headers) {
            let data = await this.base.get(this.usersUrl + '/$meta')
            this.headers = this.base._formatHeaders(data);
            this.$headers = data.columns
        }

        let data = await await this.base.get(this.usersUrl, params);

        let dictionaries = await this.base.dictionaries.$getMerchantDictionaries()

        data.data = data.data.map(d => this.base.format(d, this.$headers, dictionaries))

        data.headers = this.headers

        return data;
    }

    async getUser(userID) {
        return this.base.get(this.usersUrl + '/' + userID);
    }
    async inviteUser(data){
        return this.base.post(this.usersUrl + `/invite`, data);
    }

    async lockUser(userID){
        return this.base.post(this.usersUrl + `/${userID}/lock`);
    }

    async unlockUser(userID){
        return this.base.post(this.usersUrl + `/${userID}/unlock`);
    }

    async resetUserPassword(userID){
        return this.base.post(this.usersUrl + `/${userID}/resetPassword`);
    }

    async unlinkUserFromMerchant(userID, merchantID){
        return this.base.delete(this.usersUrl + `/${userID}/unlinkFromMerchant/${merchantID}`);
    }
}