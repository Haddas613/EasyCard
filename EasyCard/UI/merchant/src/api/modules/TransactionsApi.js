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

        let dictionaries = await this.base.dictionaries.$getTransactionDictionaries()
        
        data.data = data.data.map(d => this.format(d, this.$headers, dictionaries))

        data.headers = this.headers

        return data;
    }

    /**Based on the given data's JDealType will create corresponding operation */
    async processTransaction(data){
      let result = { status: "error" };

      switch (data.jDealType) {
        case "J4":
          result = await this.$api.transactions.createTransaction(data);
          break;
        case "J2":
          result = await this.$api.transactions.checkCreditCard(data);
          break;
        case "J5":
          result = await this.$api.transactions.blockCreditCard(data);
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

    format(d, headers, dictionaries) {
        for (const property in d) {
            let v = d[property]
            let h = headers[property]
            if (h.dataType == 'string' && v && v.length > 8) {
                d[`$${property}`] = v
                d[property] = v.substring(0, 8)
            }
            else if (h.dataType == 'dictionary') {
                d[`$${property}`] = v
                d[property] = dictionaries[h.dictionary][v]
            }
            else if (h.dataType == 'money') {
                d[`$${property}`] = v
                d[property] = new Intl.NumberFormat().format(v) // TODO: locale, currency symbol
            }
            else if (h.dataType == 'date') {
                d[`$${property}`] = v
                d[property] = moment(String(v)).format('MM/DD/YYYY') // TODO: locale
            }
        }
        return d
    }
}