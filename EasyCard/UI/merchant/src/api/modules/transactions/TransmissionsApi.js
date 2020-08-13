export default class TransmissionsApi {
    constructor(base) {
        this.base = base;
        this.baseUrl = process.env.VUE_APP_TRANSACTIONS_API_BASE_ADDRESS;
        this.transmissionsUrl = this.baseUrl + '/api/transmission';
    }

    transmit(data){
        return this.base.delete(this.transmissionsUrl + '/transmit', data);
    }
    cancelTransmission(data){
        return this.base.post(this.transmissionsUrl + '/cancel', data);
    }
}