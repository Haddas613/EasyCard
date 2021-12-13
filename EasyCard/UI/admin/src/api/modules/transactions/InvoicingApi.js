export default class InvoicingApi {
    constructor(base) {
        this.base = base;
        this.baseUrl = this.base.cfg.VUE_APP_TRANSACTIONS_API_BASE_ADDRESS;
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

    async resend(invoicesIDs){
        return await this.base.post(this.invoicingUrl + '/resend-admin', {
            invoicesIDs: invoicesIDs
        });
    }
    async downloadPDF(invoiceID){
        return await this.base.get(this.invoicingUrl + `/${invoiceID}/download`);
    }

    async getHistory(invoiceID){
        return await this.base.get(this.invoicingUrl + `/${invoiceID}/history`);
      }
}