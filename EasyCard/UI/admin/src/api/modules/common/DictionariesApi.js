export default class DictionariesApi {
    constructor(base) {
        this.base = base;
        this.transactionsBaseUrl = this.base.cfg.VUE_APP_TRANSACTIONS_API_BASE_ADDRESS;
        this.transactionDictionariesUrl = this.transactionsBaseUrl + '/api/dictionaries';

        this.merchantBaseUrl = this.base.cfg.VUE_APP_MERCHANT_API_BASE_ADDRESS;
        this.merchantDictionariesUrl = this.merchantBaseUrl + '/api/dictionaries';
    }

    async getTransactionDictionaries() {
        if (!this.transactionDictionaries) {
            let data = await this.base.get(this.transactionDictionariesUrl + '/transaction');
            if(!data)
                return null;

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

            let data = await this.base.get(this.merchantDictionariesUrl + '/merchant');
            if(!data)
                return null;

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