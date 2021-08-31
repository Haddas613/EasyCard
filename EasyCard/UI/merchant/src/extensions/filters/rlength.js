export default function(value, length){
    if(!value || value.length <= length) return value;

    return value.substr(value.length - length, length);
}