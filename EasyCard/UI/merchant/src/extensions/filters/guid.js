export default function(value){
    if(!value || value.length <= 8) return value;

    return value.substr(0, 8);
}