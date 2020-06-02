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
    client_id: 'admin_frontend', // 'implicit.shortlived',
    response_type: 'id_token token',
    scope: 'openid profile transactions_api merchants_api',
    // test use
    prompt: 'login',
    login_hint: 'bob'
  },
  console,
  LogLevel.Error
)
Vue.prototype.$oidc = mainOidc
export default mainOidc
