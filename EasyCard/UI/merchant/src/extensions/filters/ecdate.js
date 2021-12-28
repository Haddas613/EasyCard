import moment from 'moment';
import store from '../../store/index';

export default function(value, format = null){
    if(!value) return;
    
    if(format == "DT"){
        format = "DD/MM/YYYY HH:mm";
    }

    if(!format){
        /**Default locale format LLLL can not be used due to time present in the output*/
        if(store.state.localization.currentLocale == "he-IL"){
            format = "dddd, D MMMM YYYY";
        }else{
            format = "dddd, MMMM D, YYYY"
        }
    }
    return moment.utc(value).local().locale(store.state.localization.currentLocale).format(format);
}