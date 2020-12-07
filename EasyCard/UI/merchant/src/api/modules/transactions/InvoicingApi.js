export default class InvoicingApi {
    constructor(base) {
        this.base = base;
        this.baseUrl = process.env.VUE_APP_TRANSACTIONS_API_BASE_ADDRESS;
        this.invoicingUrl = this.baseUrl + '/api/invoicing';
    }

    async get(params) {

        if (!this.headers) {
            let data = await this.base.get(this.invoicingUrl + '/$meta')
            this.headers = this.base._formatHeaders(data)
            this.$headers = data.columns
        }

        let data = await this.base.get(this.invoicingUrl, params);
        
        if(!data || data.status === "error")
            return null;

        let dictionaries = await this.base.dictionaries.$getTransactionDictionaries();
        
        data.data = data.data.map(d => this.base.format(d, this.$headers, dictionaries))

        data.headers = this.headers;

        return data;
    }

    async getInvoice(id){
      if (!this.headers) {
        let data = await this.base.get(this.invoicingUrl + '/$meta')
        this.headers = this.base._formatHeaders(data)
        this.$headers = data.columns
      }
      let invoice = await this.base.get(this.invoicingUrl + `/${id}`);
      let dictionaries = await this.base.dictionaries.$getTransactionDictionaries();

      invoice = this.base.format(invoice, this.$headers, dictionaries)
      return invoice;
    }

    async createInvoice(data){
        return await this.base.post(this.invoicingUrl, data);
    }

    async updateInvoice(id, data){
        return await this.base.put(this.invoicingUrl + `/${id}`, data);
    }

    async deleteInvoice(id, data){
        return await this.base.delete(this.invoicingUrl + `/${id}`, data);
    }

    async resend(terminalId, invoicesIDs){
        return await this.base.post(this.invoicingUrl + '/resend', {
            terminalID: terminalId,
            invoicesIDs: invoicesIDs
        });
    }
    async downloadPDF(invoiceID){
        return await this.base.get(this.invoicingUrl + `/${invoiceID}/download`);
    }
}