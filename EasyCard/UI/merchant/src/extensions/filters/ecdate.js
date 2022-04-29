import moment from 'moment';
import store from '../../store/index';

export default function(value, format = null){
    if(!value) return;

    if(!format){
        /**Default locale format LLLL can not be used due to time present in the output*/
        if(store.state.localization.currentLocale == "he-IL"){
            format = "dddd, D MMMM YYYY";
        }else{
            format = "dddd, MMMM D, YYYY"
        }
    } else {
        if(format.toLowerCase() == "dt"){
            format = "DD/MM/YYYY HH:mm";
        }
    
        if(format.toLowerCase() == "d"){
            format = "DD/MM/YYYY";
        }
    }
    return moment.utc(value).local().locale(store.state.localization.currentLocale).format(format);
}