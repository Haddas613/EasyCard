export default class PaymentRequestsApi {
    constructor(base) {
        this.base = base;
        this.baseUrl = process.env.VUE_APP_TRANSACTIONS_API_BASE_ADDRESS;
        this.paymentRequestsUrl = this.baseUrl + '/api/paymentRequests';
    }

    async get(params) {

        if (!this.headers) {
            let data = await this.base.get(this.paymentRequestsUrl + '/$meta')
            this.headers = this.base._formatHeaders(data)
            this.$headers = data.columns
        }

        let data = await this.base.get(this.paymentRequestsUrl, params);
        
        if(!data || data.status === "error")
            return null;

        let dictionaries = await this.base.dictionaries.$getTransactionDictionaries();
        
        data.data = data.data.map(d => this.base.format(d, this.$headers, dictionaries))

        data.headers = this.headers;

        return data;
    }

    async getPaymentRequest(id){
      if (!this.headers) {
        let data = await this.base.get(this.paymentRequestsUrl + '/$meta')
        this.headers = this.base._formatHeaders(data)
        this.$headers = data.columns
      }
      let invoice = await this.base.get(this.paymentRequestsUrl + `/${id}`);
      let dictionaries = await this.base.dictionaries.$getTransactionDictionaries();

      invoice = this.base.format(invoice, this.$headers, dictionaries)
      return invoice;
    }

    async createPaymentRequest(data){
        return await this.base.post(this.paymentRequestsUrl, data);
    }

    async updatePaymentRequest(id, data){
        return await this.base.put(this.paymentRequestsUrl + `/${id}`, data);
    }

    async deletePaymentRequest(id, data){
        return await this.base.delete(this.paymentRequestsUrl + `/${id}`, data);
    }
}