const config = {
    ...{
        VUE_APP_I18N_LOCALE: process.env.VUE_APP_I18N_LOCALE,
        VUE_APP_I18N_FALLBACK_LOCALE: process.env.VUE_APP_I18N_FALLBACK_LOCALE,
        VUE_APP_TRANSACTIONS_API_BASE_ADDRESS: process.env.VUE_APP_TRANSACTIONS_API_BASE_ADDRESS,
        VUE_APP_PROFILE_API_BASE_ADDRESS: process.env.VUE_APP_PROFILE_API_BASE_ADDRESS,
        VUE_APP_REPORT_API_BASE_ADDRESS: process.env.VUE_APP_REPORT_API_BASE_ADDRESS,
        VUE_APP_AUTHORITY: process.env.VUE_APP_AUTHORITY,
        VUE_APP_APPLICATION_INSIGHTS_KEY: process.env.VUE_APP_APPLICATION_INSIGHTS_KEY,
        VUE_APP_VERSION: process.env.VUE_APP_VERSION
    },
    ...window.config
};

export default config;