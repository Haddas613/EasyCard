export default class TransmissionsApi {
    constructor(base) {
        this.base = base;
        this.baseUrl = this.base.cfg.VUE_APP_TRANSACTIONS_API_BASE_ADDRESS;
        this.transmissionsUrl = this.baseUrl + '/api/transmission';
    }

    transmit(data){
        return this.base.post(this.transmissionsUrl + '/transmit', data);
    }

    transmitByTerminal(terminalID) {
        return this.base.post(this.transmissionsUrl + `/transmitByTerminal/${terminalID}`);
    }
    
    cancelTransmission(data){
        return this.base.post(this.transmissionsUrl + '/cancel', data);
    }
}