export default class TransactionsApi {
    constructor(base) {
        this.base = base;
        this.baseUrl = this.base.cfg.VUE_APP_TRANSACTIONS_API_BASE_ADDRESS;
        this.transactionsUrl = this.baseUrl + '/api/transactions';
    }

    async get(params) {

        if (!this.headers) {
            let data = await this.base.get(this.transactionsUrl + '/$meta')
            this.headers = this.base._formatHeaders(data)
            this.$headers = data.columns
        }

        let data = await this.base.get(this.transactionsUrl, params);
        
        if(!data || data.status === "error")
            return null;

        let dictionaries = await this.base.dictionaries.$getTransactionDictionaries();
        
        data.data = data.data.map(d => this.base.format(d, this.$headers, dictionaries))

        data.headers = this.headers;

        return data;
    }

    async getTransaction(id){
      if (!this.headers) {
        let data = await this.base.get(this.transactionsUrl + '/$meta')
        this.headers = this.base._formatHeaders(data)
        this.$headers = data.columns
      }
      let transaction = await this.base.get(this.transactionsUrl + `/${id}`);
      let dictionaries = await this.base.dictionaries.$getTransactionDictionaries();

      transaction = this.base.format(transaction, this.$headers, dictionaries)
      return transaction;
    }

    async getGrouped(params) {

      if (!this.headers) {
          let data = await this.base.get(this.transactionsUrl + '/$meta')
          this.headers = this.base._formatHeaders(data)
          this.$headers = data.columns
      }

      let data = await this.base.get(this.transactionsUrl + '/$grouped', params);
      
      if(!data || data.status === "error")
          return null;

      let dictionaries = await this.base.dictionaries.$getTransactionDictionaries();
      
      for(var d of data){
        d.data = d.data.map(x => this.base.format(x, this.$headers, dictionaries))
      }

      data.headers = this.headers;

      return data;
  }

  async getHistory(transactionId){
    return await this.base.get(this.transactionsUrl + `/${transactionId}/history`);
  }

  async sendTransactionSlipEmail(data) {
    return await this.base.post(this.transactionsUrl + '/send-transaction-slip-email', data);
  }

  async selectJ5(transactionID){
    return await this.base.post(this.transactionsUrl + `/selectJ5/${transactionID}`);
  }

  /** bit refund */
  async chargeback(transactionID, amount){
    let payload = {
      existingPaymentTransactionID: transactionID,
      refundAmount: amount,
    };

    return await this.base.post(this.transactionsUrl + '/chargeback', payload,  { showBadRequestToastr: false });
  }
}