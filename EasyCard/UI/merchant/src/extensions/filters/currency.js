import store from '../../store/index';

export default function(value, currency) {
    value = parseFloat(value);
    if (typeof value !== "number") {
        return value;
    }
    var formatter = new Intl.NumberFormat(store.state.localization.currentLocale, {
        style: 'currency',
        currency: currency,
        minimumFractionDigits: 2
    });
    
    return formatter.format(value);
};