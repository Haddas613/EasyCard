import store from '../../store/index';

export default function (value, currency) {
    value = parseFloat(value);
    if (typeof value !== "number") {
        return value;
    }
    let loc = store.state.localization.currentLocale;
    if (loc == "en-IL") {
        //en-IL shows USD as US$ which is not acceptable
        loc = "en-US";
    }

    var formatter = new Intl.NumberFormat(loc, {
        style: 'currency',
        currency: currency,
        minimumFractionDigits: 2
    });

    return formatter.format(value);
};