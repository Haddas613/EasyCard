import moment from 'moment';
import store from '../../store/index';

export default function(value, format = null){
    if(!value) return;
    
    if(!format){
        format = "DD/MM/YYYY HH:mm";
        /**Default locale format LLLL can not be used due to time present in the output*/
        // if(store.state.localization.currentLocale == "he-IL"){
        //     format = "DD/MM/YYYY HH:MM";
        // }else{
        //     format = "DD/MM/YYYY HH:MM"
        // }
    }
    return moment.utc(value).local().locale(store.state.localization.currentLocale).format(format);
}