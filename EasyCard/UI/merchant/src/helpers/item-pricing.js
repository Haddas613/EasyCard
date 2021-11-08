import lodash from "lodash";

const itemFunctions = {
    calculate: function(item, opts){
        let amount = this.calculateAmount(item);
        //let vatRate = item.vatRate >= 0 ? item.vatRate : opts.vatRate;

        let netAmount = amount ? this.calculateNetAmount(amount, opts.vatRate) : 0;
        let vat = this.calculateVat(amount, netAmount);
        
        item.amount = amount;
        item.netAmount = netAmount;
        item.vat = vat;
        item.vatRate = opts.vatRate;
    },
    calculateAmount: function(item){
        return parseFloat(((item.price * item.quantity) - item.discount).toFixed(2));
    },
    calculateNetAmount: function(amount, vatRate){
        if(!amount){ return 0; }
        return parseFloat((amount / (1 + vatRate)).toFixed(2));
    },
    calculateVat: function(amount, netAmount){
        return parseFloat((amount - netAmount).toFixed(2));
    }
};

const totalFunctions = {
    calculate: function(model, opts){
        let totalAmount = parseFloat(lodash.sumBy(model.dealDetails.items, "amount").toFixed(2));
        let vatRate = model.vatRate >= 0 ? model.vatRate : opts.vatRate;
        let netTotal = totalAmount ? this.calculateNetTotal(totalAmount, vatRate) : 0;
        let vatTotal = this.calculateVatTotal(model.dealDetails.items, totalAmount, netTotal);
        
        model.totalAmount = totalAmount;
        model.netTotal = netTotal;
        model.vatTotal = vatTotal;
        model.vatRate = vatRate;
    },
    calculateWithoutItems: function(model, key, opts){
        let totalAmount = model[key];
        let vatRate = model.vatRate >= 0 ? model.vatRate : opts.vatRate;
        let netTotal = totalAmount ? this.calculateNetTotal(totalAmount, vatRate) : 0;
        let vatTotal = parseFloat((totalAmount - netTotal).toFixed(2));
        model.netTotal = netTotal;
        model.vatTotal = vatTotal;
        model.vatRate = vatRate;
    },
    calculateNetTotal: function(amount, vatRate){
        return parseFloat((amount / (1 + vatRate)).toFixed(2));
    },
    calculateVatTotal: function(items, totalAmount, netTotal){
        let vat = parseFloat((totalAmount - netTotal).toFixed(2));
        let intermediateVat = lodash.sumBy(items, "vat");
        if(vat != intermediateVat){
            items[items.length - 1].vat += (vat - intermediateVat);
        }
        return vat;
    },

}

export default {item: itemFunctions, total: totalFunctions};