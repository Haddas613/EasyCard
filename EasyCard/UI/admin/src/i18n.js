import Vue from 'vue'
import VueI18n from 'vue-i18n'
import cfg from "./app.config";

Vue.use(VueI18n)

function loadLocaleMessages () {
  const locales = require.context('./locales', true, /[A-Za-z0-9-_,\s]+\.json$/i)
  const messages = {}
  locales.keys().forEach(key => {
    const matched = key.match(/([A-Za-z0-9-_]+)\./i)
    if (matched && matched.length > 1) {
      const locale = matched[1]
      messages[locale] = locales(key)
    }
  })
  return messages
}

export default new VueI18n({
  locale: cfg.VUE_APP_I18N_LOCALE || 'en-IL',
  fallbackLocale: cfg.VUE_APP_I18N_FALLBACK_LOCALE || 'en-IL',
  messages: loadLocaleMessages(),
  //TODO: remove later on
  silentTranslationWarn: true
})
