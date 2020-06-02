export default class DictionariesApi {
    constructor(base) {
        this.base = base;
        this.baseUrl = process.env.VUE_APP_TRANSACTIONS_API_BASE_ADDRESS;
        this.dictionariesUrl = this.baseUrl + '/api/dictionaries';
    }

    async getTransactionDictionaries() {
        if (!this.transactionDictionaries) {

            let data = await this.base.get(this.dictionariesUrl + '/transaction');
            this.$transactionDictionaries = data;
            this.transactionDictionaries = {};

            for (const dict in data) {
                let members = [];
                for (const [code, description] of Object.entries(data[dict])) {
                    members.push({ code, description });
                }
                this.transactionDictionaries[dict] = members;
            }
        }
        return this.transactionDictionaries;
    }

    async $getTransactionDictionaries() {
        if (!this.$transactionDictionaries) {
            await this.getTransactionDictionaries()
        }
        return this.$transactionDictionaries;
    }
}