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

    async getConsumer(id) {
        return await this.base.get(this.consumersUrl + `/${id}`);
    }

    async updateConsumer(id, data){
        return await this.base.put(this.consumersUrl + `/${id}`, data);
    }

    async createConsumer(data){
        return await this.base.post(this.consumersUrl, data);
    }
}