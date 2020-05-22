import Vue from 'vue'
import { createOidcAuth, SignInType, LogLevel } from 'vue-oidc-client'

const loco = window.location
const appRootUrl = `${loco.protocol}//${loco.host}${process.env.BASE_URL}`

var mainOidc = createOidcAuth(
  'main',
  SignInType.Window,
  appRootUrl,
  {
    authority: process.env.VUE_APP_AUTHORITY,
    client_id: 'merchant_frontend',
    response_type: 'token',
    scope: 'openid profile transactions_api',
  },
  console,
  LogLevel.Error
)
Vue.prototype.$oidc = mainOidc
export default mainOidc
