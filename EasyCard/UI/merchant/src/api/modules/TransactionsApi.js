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

        let data = await this.base.get(this.transactionsUrl, params)

        let dictionaries = await this.base.dictionaries.$getTransactionDictionaries()

        data.data = data.data.map(d => this.format(d, this.$headers, dictionaries))

        data.headers = this.headers

        return data;
    }

    // async getHeaders() {
    //     if (this.headers)
    //         return this.headers;

    //     let data = await this.base.get(this.transactionsUrl + '/$meta');
    //     this.headers = this.base._formatHeaders(data);
    //     return this.headers;
    // }

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