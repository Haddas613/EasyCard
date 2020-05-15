import Vue from 'vue'
import { createOidcAuth, SignInType, LogLevel } from 'vue-oidc-client'

const loco = window.location
const appRootUrl = `${loco.protocol}//${loco.host}${process.env.BASE_URL}`

var mainOidc = createOidcAuth(
  'main',
  SignInType.Window,
  appRootUrl,
  {
    authority: 'https://localhost:44331/',
    client_id: 'merchant_frontend', // 'implicit.shortlived',
    response_type: 'id_token token',
    scope: 'openid profile transactions_api',
    // test use
    prompt: 'login',
    login_hint: 'bob'
  },
  console,
  LogLevel.Error
)
Vue.prototype.$oidc = mainOidc
export default mainOidc
