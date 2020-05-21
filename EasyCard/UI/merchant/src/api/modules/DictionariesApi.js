export default class DictionariesApi{
    constructor(base){
        this.base = base;
        this.baseUrl = process.env.VUE_APP_TRANSACTIONS_API_BASE_ADDRESS;
        this.dictionariesUrl = this.baseUrl + '/api/dictionaries';
    }

    async getTransactionDictionaries() {
        if (this.transactionDictionaries) 
            return this.transactionDictionaries;

        let data = await this.base.get(this.dictionariesUrl + '/transaction');
        this.transactionDictionaries = data;
        return this.transactionDictionaries;
    }
}