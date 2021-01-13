import { UserManager, WebStorageStateStore, User } from 'oidc-client';

class AuthService {
    constructor() {

        const loco = window.location
        const appRootUrl = `${loco.protocol}//${loco.host}${process.env.BASE_URL}`

        const settings = {
            userStore: new WebStorageStateStore({ store: window.localStorage }),
            automaticSilentRenew: true,
            filterProtocolClaims: true,
            authority: process.env.VUE_APP_AUTHORITY,

            redirect_uri: `${appRootUrl}callback.html`,

            silent_redirect_uri: `${appRootUrl}silent-renew.html`,

            post_logout_redirect_uri: appRootUrl,

            client_id: 'merchant_frontend',
            response_type: 'id_token token',
            scope: 'openid profile transactions_api roles',
        };

        this.userManager = new UserManager(settings);
    }

    getUser() {
        return this.userManager.getUser();
    }

    getUserProfile() {
        return this.userManager.getUser().then((data) => {
            return !!data ? data.profile : null;
        });
    }

    isAuthenticated() {
        return this.getAccessToken().then((access_token) => {
            return access_token != null;
        });
    }

    signinRedirect(route) {
        return this.userManager.signinRedirect({
            state: route
        });
    }

    signOut() {
        return this.userManager.signoutRedirect();
    }

    getAccessToken() {
        return this.userManager.getUser().then((data) => {
            return !!data ? data.access_token : null;
        });
    }
}

export default {
    install: function(Vue, ) {
        Object.defineProperty(Vue.prototype, '$oidc', { value: new AuthService() });
    }
}