export default function(value){
    if(value == null || !value.repeatPeriodType){
        return null;
    }
    let str = `${value.repeatPeriodType}: ${value.repeatPeriod || '-'}`;
    if(value.startAtType){
        str += ` / ${value.startAtType}: ${value.startAt}`;
    }
    if(value.endAtType){
        str += ` / ${value.endAtType}: ${value.endAt}`;
    }
    if(value.endAtNumberOfPayments){
        str += ` | ${value.endAtNumberOfPayments}`;
    }

    return str;
}