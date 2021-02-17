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

            client_id: 'admin_frontend',
            response_type: 'id_token token',
            scope: 'openid profile merchants_api transactions_api roles',
        };

        this.userManager = new UserManager(settings);
        this.billingAdminRole = "BillingAdministrator";
        this.businessAdminRole = "BusinessAdministrator";
    }

    getUser() {
        return this.userManager.getUser();
    }

    getUserProfile() {
        return this.userManager.getUser().then((data) => {
            return !!data ? data.profile : null;
        });
    }

    async isAuthenticated() {
        const user = await this.userManager.getUser();
        if(!user){
            return false;
        }
        return !!(user && user.access_token); 
    }

    async isAdmin(){
        let isBusinessAdmin = await this.isBusinessAdmin();
        let isBillingAdmin = await this.isBillingAdmin();
        
        return (isBusinessAdmin || isBillingAdmin);
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
        if(!(await this.isAuthenticated())){
            return null;
        }
       
        const user = await this.userManager.getUser();
        return user ? user.access_token : null; 
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
}

export default {
    install: function(Vue, ) {
        Object.defineProperty(Vue.prototype, '$oidc', { value: new AuthService() });
    }
}