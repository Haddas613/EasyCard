//TODO: use store
const switchLanguage = (i18n, vuetify) => {
    if (i18n.locale == 'he-IL') {
        vuetify.rtl = true;
        vuetify.lang.current = 'he';
    } else {
        vuetify.rtl = false;
        vuetify.lang.current = 'en';
    }
}

export default {
    switchLanguage
};