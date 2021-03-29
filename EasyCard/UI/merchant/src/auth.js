import { UserManager, WebStorageStateStore, User } from 'oidc-client';
import cfg from "./app.config";

class AuthService {
    constructor() {

        const loco = window.location
        const appRootUrl = `${loco.protocol}//${loco.host}/`

        const settings = {
            userStore: new WebStorageStateStore({ store: window.localStorage }),
            automaticSilentRenew: true,
            filterProtocolClaims: true,
            authority: cfg.VUE_APP_AUTHORITY,

            redirect_uri: `${appRootUrl}callback.html`,

            silent_redirect_uri: `${appRootUrl}silent-renew.html`,

            post_logout_redirect_uri: appRootUrl,

            client_id: 'merchant_frontend',
            response_type: 'id_token token',
            scope: 'openid profile transactions_api roles',
        };

        this.userManager = new UserManager(settings);

        this.billingAdminRole = "BillingAdministrator";
        this.businessAdminRole = "BusinessAdministrator";
        this.merchantRole = "Merchant";
        this._accessTokenLockPromise = null;
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

    async getAccessToken() {
        if(this._accessTokenLockPromise){
            return await this._accessTokenLockPromise;
        }else{
            return await (this._accessTokenLockPromise = this.__getAccessTokenInternal());
        }
    }

    async __getAccessTokenInternal(){
        let userData = await this.userManager.getUser();

        if(userData && userData.expired){
            try{
                userData = await this.userManager.signinSilent();
            }catch{}
        }
        this._accessTokenLockPromise = null;
        return !!userData ? userData.access_token : null;
    }

    async getUserDisplayName() {
        if(!!this.userDisplayName){
            return this.userDisplayName;
        }
       
        const user = await this.userManager.getUser();
        if(!user || !user.profile){
            return null;
        }

        let fullname = `${user.profile.extension_FirstName || ""} ${user.profile.extension_LastName || ""}`;

        if(fullname.trim()){
            return fullname;
        }

        return user.profile.name;
    }

    async isMerchant(){
        return this.isInRole(this.merchantRole);
    }

    async isInRole(role){
        if(!this.roles){
            this.roles = {};
        }

        if(typeof(this.roles[role]) === "undefined"){
            const user = await this.userManager.getUser();
            if(!user || !user.profile){
                return false;
            }
            this.roles[role] = (user.profile.role && user.profile.role.indexOf(role) > -1);
        }

        return this.roles[role];
    }

    async isBillingAdmin(){
        return this.isInRole(this.billingAdminRole);
    }

    async isBusinessAdmin(){
        return this.isInRole(this.businessAdminRole);
    }
}

export default {
    install: function(Vue, ) {
        Object.defineProperty(Vue.prototype, '$oidc', { value: new AuthService() });
    }
}