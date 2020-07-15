/** 
 * To remove ambigiousness on server side between clients and customers api was named consumers. 
 * Represents customer entity on UI.
*/
export default class ConsumersApi {
    constructor(base) {
        this.base = base;
        this.baseUrl = process.env.VUE_APP_PROFILE_API_BASE_ADDRESS;
        this.consumersUrl = this.baseUrl + '/api/consumers';
    }

    async getConsumers(params) {
        return await this.base.get(this.consumersUrl, params);
    }
}