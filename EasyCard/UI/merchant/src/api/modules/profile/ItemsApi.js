export default class ItemsApi {
    constructor(base) {
        this.base = base;
        this.baseUrl = this.base.cfg.VUE_APP_PROFILE_API_BASE_ADDRESS;
        this.itemsUrl = this.baseUrl + '/api/items';
    }

    async getItems(params) {
        if (!this.headers) {
            let data = await this.base.get(this.itemsUrl + '/$meta')
            this.headers = this.base._formatHeaders(data)
            this.$headers = data.columns
        }
        
        let data = await this.base.get(this.itemsUrl, params);
        let dictionaries = await this.base.dictionaries.$getTransactionDictionaries();

        data.data = data.data.map(d => this.base.format(d, this.$headers, dictionaries))

        return data;
    }

    async getItem(id){
        return await this.base.get(this.itemsUrl + `/${id}`);
    }

    async updateItem(id, data){
        return await this.base.put(this.itemsUrl + `/${id}`, data);
    }

    async createItem(data){
        return await this.base.post(this.itemsUrl, data);
    }

    async deleteItem(id){
        return await this.base.delete(this.itemsUrl + `/${id}`);
    }
    async bulkDeleteItems(ids){
        return await this.base.post(this.itemsUrl + '/bulkdelete', ids);
    }
}