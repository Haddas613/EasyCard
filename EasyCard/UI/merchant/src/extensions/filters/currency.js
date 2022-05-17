import store from '../../store/index';

export default function (value, currency) {
    if((!value && value !== 0) || !currency){
        return '-';
    }
    
    value = parseFloat(value);
    if (typeof value !== "number") {
        return value;
    }
    let loc = store.state.localization.currentLocale;
    if (loc == "en-IL") {
        //en-IL shows USD as US$ which is not acceptable
        //he-EN shows $ and maintains Israeli order with currency at the end, like 123$
        loc = "he-EN";
    }
    if(!window.formatter || window.formatter.loc != loc){
        window.formatter = { loc: loc, currencies: {} };
    }
    if(!window.formatter.currencies[currency]){
        window.formatter.currencies[currency] = new Intl.NumberFormat(loc, {
            style: 'currency',
            currency: currency,
            minimumFractionDigits: 2
        })
    }
    //remove whitespace to prevent direction issues
    return window.formatter.currencies[currency].format(value).replace(/\s/g, '');
};