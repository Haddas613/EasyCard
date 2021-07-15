export default class PaymentIntentsApi {
    constructor(base) {
        this.base = base;
        this.baseUrl = this.base.cfg.VUE_APP_TRANSACTIONS_API_BASE_ADDRESS;
        this.paymentIntentUrl = this.baseUrl + '/api/paymentIntent';
    }

    async getPaymentIntent(id){
      let paymentIntent = await this.base.get(this.paymentIntentUrl + `/${id}`);
      let dictionaries = await this.base.dictionaries.$getTransactionDictionaries();

      paymentIntent = this.base.format(paymentIntent, this.$headers, dictionaries)
      return paymentIntent;
    }

    async createPaymentIntent(data){
        return await this.base.post(this.paymentIntentUrl, data);
    }
}