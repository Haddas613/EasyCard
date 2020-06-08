export default class DictionariesApi {
    constructor(base) {
        this.base = base;
        this.dictionariesUrl = '/api/dictionaries';
    }

    async getTransactionDictionaries() {
        if (!this.transactionDictionaries) {

            let data = await this.base.get(process.env.VUE_APP_TRANSACTIONS_API_BASE_ADDRESS + this.dictionariesUrl + '/transaction');
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

    async getMerchantDictionaries() {
        if (!this.merchantDictionaries) {

            let data = await this.base.get(process.env.VUE_APP_MERCHANT_API_BASE_ADDRESS + this.dictionariesUrl);
            this.$merchantDictionaries = data;
            this.merchantDictionaries = {};

            for (const dict in data) {
                let members = [];
                for (const [code, description] of Object.entries(data[dict])) {
                    members.push({ code, description });
                }
                this.merchantDictionaries[dict] = members;
            }
        }
        return this.merchantDictionaries;
    }

    async $getMerchantDictionaries() {
        if (!this.$merchantDictionaries) {
            await this.getMerchantDictionaries()
        }
        return this.$merchantDictionaries;
    }
}