import moment from 'moment'

export default class TransactionsApi {
    constructor(base) {
        this.base = base;
        this.baseUrl = process.env.VUE_APP_TRANSACTIONS_API_BASE_ADDRESS;
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

    /**Based on the given data's JDealType will create corresponding operation */
    async processTransaction(data){
      let result = { status: "error" };

      switch (data.jDealType) {
        case "J4":
          result = await this.createTransaction(data);
          break;
        case "J2":
          result = await this.checkCreditCard(data);
          break;
        case "J5":
          result = await this.blockCreditCard(data);
          break;
        default:
          result.message = `Could not process JDeal type: ${data.jDealType}`;
      }
      return result;
    }

    async createTransaction(data){
        //j4
        return await this.base.post(this.transactionsUrl + '/create', data);
    }

    async checkCreditCard(data){
        //j2
        return await this.base.post(this.transactionsUrl + '/checking', data);
    }

    async blockCreditCard(data){
        //j5
        return await this.base.post(this.transactionsUrl + '/blocking', data);
    }

    async refund(data){
        //refund no jdeal type
        return await this.base.post(this.transactionsUrl + '/refund', data);
    }
}