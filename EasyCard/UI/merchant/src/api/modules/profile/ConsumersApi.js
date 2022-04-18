/** 
 * To remove ambigiousness on server side between clients and customers api was named consumers. 
 * Represents customer entity on UI.
*/
export default class ConsumersApi {
    constructor(base) {
        this.base = base;
        this.baseUrl = this.base.cfg.VUE_APP_PROFILE_API_BASE_ADDRESS;
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

    async deleteConsumer(id){
        return await this.base.delete(this.consumersUrl + `/${id}`);
    }

    async restoreConsumer(id){
        return await this.base.put(this.consumersUrl + `/restore/${id}`);
    }

    async createConsumer(data){
        return await this.base.post(this.consumersUrl, data);
    }

    async getLastChargedConsumers(store, terminalId){
        if (store.length === 0){
            return [];
        }

        let consumersOfTerminal = store.filter(c => c.terminalID == terminalId).map(s => s.id);
        if (consumersOfTerminal.length == 0){ return [];}

        let data = (await this.base.get(this.consumersUrl, {
            consumersID: consumersOfTerminal.join(","),
            take: 5
        })).data || [];

        if (data.length === 0){
            return data;
        }
        let result = [];
        for(var s of store){
            let idx = data.findIndex(c => c.consumerID === s.id);
            if(idx > -1){
                result.push(data[idx]);
            }
        }

        return result;
    }
}