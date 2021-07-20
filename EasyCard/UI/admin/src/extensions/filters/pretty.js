export default function(value) {
    return JSON.stringify(JSON.parse(value), null, 2);
}