import store from '../../../store/index';
import cfg from "../../../app.config";

export default class DictionariesApi {
    constructor(base) {
        this.base = base;
        this.transactionsBaseUrl = this.base.cfg.VUE_APP_TRANSACTIONS_API_BASE_ADDRESS;
        this.transactionDictionariesUrl = this.transactionsBaseUrl + '/api/dictionaries';

        this.merchantBaseUrl = this.base.cfg.VUE_APP_MERCHANT_API_BASE_ADDRESS;
        this.merchantDictionariesUrl = this.merchantBaseUrl + '/api/dictionaries';

        this.locale = (store.state.localization && store.state.localization.currentLocale)
            ? store.state.localization.currentLocale : cfg.VUE_APP_I18N_LOCALE;
    }

    async getTransactionDictionaries() {
        if (!this.transactionDictionaries) {
            let data = await this.base.get(this.transactionDictionariesUrl + `/transaction?lang=${this.locale}`);
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

            let data = await this.base.get(this.merchantDictionariesUrl + `/merchant?lang=${this.locale}`);
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

    async getBanks(){
        if(!this.banks){
            this.banks = await this.base.get(this.transactionDictionariesUrl + `/banks?lang=${this.locale}`);
        }

        return this.banks;
    }

    async getWebhooks(){
        if(!this.webhooks){
            this.webhooks = await this.base.get(this.transactionDictionariesUrl + `/webhooks?lang=${this.locale}`);
        }

        return this.webhooks;
    }
}