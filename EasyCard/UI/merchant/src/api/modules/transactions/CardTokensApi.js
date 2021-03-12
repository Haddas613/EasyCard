export default class CardTokensApi {
    constructor(base) {
        this.base = base;
        this.baseUrl = this.base.cfg.VUE_APP_TRANSACTIONS_API_BASE_ADDRESS;
        this.cardtokensUrl = this.baseUrl + '/api/cardtokens';
    }

    getCustomerCardTokens(customerId){
        return this.base.get(this.cardtokensUrl, {consumerID: customerId});
    }

    createCardToken(data){
        return this.base.post(this.cardtokensUrl, data);
    }

    deleteCardToken(tokenId){
        return this.base.delete(this.cardtokensUrl + `/${tokenId}`);
    }
}