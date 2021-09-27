module.exports = {
  pluginOptions: {
    i18n: {
      locale: 'en',
      fallbackLocale: 'en',
      localeDir: 'locales',
      enableInSFC: true
    }
  },
  productionSourceMap: true,
  configureWebpack: (config) => {
    config.output.filename = '[name].[hash:8].js';
    config.output.chunkFilename = '[name].[hash:8].js';
    config.devtool = 'source-map';
  }
}
