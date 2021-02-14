<template>
  <v-flex>
    <v-card class="mx-2 my-2">
      <v-card-title class="py-2">
        <v-row no-gutters class="py-0">
          <v-col cols="12" md="8" class="d-flex">
            <span class="pt-2 ecdgray--text subtitle-2 text-uppercase">{{$t('PersonalInformation')}}</span>
          </v-col>
          <v-col cols="12" md="4" class="d-flex justify-end">
            <v-btn color="secondary" @click="loginAsMerchant()" v-if="!loginAsMerchantURL">
              <v-icon left class="body-1">mdi-account-convert</v-icon>
              {{$t("LoginAsMerchant")}}
            </v-btn>
            <v-btn color="secondary" v-show="loginAsMerchantURL" ref="loginAsMerchantBtn">
              <a class="white--text text-decoration-none" :href="loginAsMerchantURL" target="_blank">
                <v-icon left class="body-1">mdi-account-convert</v-icon>
                {{$t("LoginAsMerchant")}}
              </a>
            </v-btn>
            <v-btn
              text
              class="primary--text mx-1"
              link
              :to="{name: 'EditMerchant', params: { id: this.$route.params.id}}"
            >
              <v-icon left class="body-1">mdi-pencil-outline</v-icon>
              {{$t('Edit')}}
            </v-btn>
          </v-col>
        </v-row>
      </v-card-title>
      <v-divider></v-divider>
      <v-card-text>
        <v-row class="body-2 black--text">
          <v-col cols="12" md="3">
            <p class="caption ecgray--text text--darken-2">{{$t('BusinessName')}}</p>
            <p>{{model.businessName}}</p>
          </v-col>
          <v-col cols="12" md="3">
            <p class="caption ecgray--text text--darken-2">{{$t('MarketingName')}}</p>
            <p>{{model.marketingName}}</p>
          </v-col>
          <v-col cols="12" md="3">
            <p class="caption ecgray--text text--darken-2">{{$t('BusinessID')}}</p>
            <p>{{model.businessID}}</p>
          </v-col>
          <v-col cols="12" md="3">
            <p class="caption ecgray--text text--darken-2">{{$t('ContactPerson')}}</p>
            <p>{{model.contactPerson}}</p>
          </v-col>
        </v-row>
      </v-card-text>
    </v-card>

    <v-card class="mx-2 my-2" :loading="terminals == null">
      <v-card-title class="py-2">
        <v-row no-gutters class="py-0">
          <v-col cols="9" class="d-flex">
            <span class="pt-2 ecdgray--text subtitle-2 text-uppercase">{{$t('Terminals')}}</span>
          </v-col>
          <v-col cols="3" class="d-flex justify-end">
            <v-btn text class="primary--text px-0" @click="showCreateTerminalDialog = true;">
              <v-icon left class="body-1">mdi-plus-circle</v-icon>
              {{$t('CreateNew')}}
            </v-btn>
          </v-col>
        </v-row>
      </v-card-title>
      <v-divider></v-divider>
      <v-card-text class="body-1 black--text">
        <create-terminal-dialog
          :show.sync="showCreateTerminalDialog"
          v-on:ok="getTerminals()"
          :merchant-id="$route.params.id"
        ></create-terminal-dialog>
        <ec-list :items="terminals" v-if="terminals && terminals.length > 0">
          <template v-slot:left="{ item }">
            <v-col cols="12" md="6" lg="6" class="caption ecgray--text">{{item.terminalID}}</v-col>
            <v-col cols="12" md="6" lg="6">{{item.label}}</v-col>
          </template>

          <template v-slot:append="{ item }">
            <v-btn icon link :to="{name: 'EditTerminal', params: {id: item.$terminalID}}">
              <v-icon color="secondary">mdi-pencil</v-icon>
            </v-btn>
          </template>
        </ec-list>
        <p
          class="subtitle-2 ecgray--text"
          v-if="terminals && terminals.length === 0"
        >{{$t("NothingToShow")}}</p>
      </v-card-text>
    </v-card>

    <v-card class="mx-2 my-2">
      <v-card-title class="py-2">
        <v-row no-gutters class="py-0">
          <v-col cols="9" class="d-flex">
            <span class="pt-2 ecdgray--text subtitle-2 text-uppercase">{{$t('Users')}}</span>
          </v-col>
          <v-col cols="3" class="d-flex justify-end">
            <v-btn text class="primary--text px-0" @click="showCreateUserDialog = true;">
              <v-icon left class="body-1">mdi-plus-circle</v-icon>
              {{$t('CreateNew')}}
            </v-btn>
          </v-col>
        </v-row>
      </v-card-title>
      <v-divider></v-divider>
      <v-card-text class="body-1 black--text">
        <create-user-dialog
          :show.sync="showCreateUserDialog"
          v-on:ok="getMerchant()"
          :merchant-id="$route.params.id"
        ></create-user-dialog>
        <ec-list :items="model.users" v-if="model.users && model.users.length > 0" :dense="true">
          <template v-slot:left="{ item }">
            <v-col cols="12" md="6" lg="6" class="caption ecgray--text">{{item.userID | guid}}</v-col>
            <v-col cols="12" md="6" lg="6">{{item.email}}</v-col>
          </template>

          <template v-slot:append="{ item }">
            <v-btn
              class="mx-1"
              color="primary"
              icon
              :title="$t('Invite')"
              :loading="actionInProgress"
              @click="inviteUser(item)"
              v-if="item.status == 'invited'"
            >
              <v-icon>mdi-email</v-icon>
            </v-btn>
            <v-btn
              class="mx-1"
              color="error"
              icon
              :title="$t('Lock')"
              :loading="actionInProgress"
              @click="lockUser(item)"
              v-if="item.status == 'active'"
            >
              <v-icon>mdi-lock</v-icon>
            </v-btn>
            <v-btn
              class="mx-1"
              color="success"
              icon
              :title="$t('Unlock')"
              :loading="actionInProgress"
              @click="unlockUser(item)"
              v-if="item.status == 'locked'"
            >
              <v-icon>mdi-lock-open</v-icon>
            </v-btn>
            <v-btn
              class="mx-1"
              color="orange darken-3"
              icon
              :title="$t('ResetPassword')"
              :loading="actionInProgress"
              @click="resetUserPassword(item.userID)"
            >
              <v-icon>mdi-lock-reset</v-icon>
            </v-btn>
            <v-btn class="mx-1" color="deep-purple" icon link :title="$t('SeeHistory')" :to="{name:'Audits',params:{filters:{userID: item.userID}}}">
              <v-icon>mdi-book-account</v-icon>
            </v-btn>
            <v-btn icon @click="unlinkFromMerchant(item.userID)" :loading="actionInProgress">
              <v-icon color="error">mdi-delete</v-icon>
            </v-btn>
          </template>
        </ec-list>
        <p
          class="subtitle-2 ecgray--text"
          v-if="model.users && model.users.length === 0"
        >{{$t("NothingToShow")}}</p>
      </v-card-text>
    </v-card>
  </v-flex>
</template> 

<script>
import EcList from "../../components/ec/EcList";

export default {
  components: {
    EcList: () => import("../../components/ec/EcList"),
    CreateTerminalDialog: () =>
      import("../../components/terminals/CreateTerminalDialog"),
    CreateUserDialog: () => import("../../components/users/CreateUserDialog")
  },
  props: {
    data: {
      type: Object,
      default: null,
      required: false
    }
  },
  data() {
    return {
      model: {},
      terminals: null,
      showCreateTerminalDialog: false,
      showCreateUserDialog: false,
      actionInProgress: false,
      loginAsMerchantURL: null
    };
  },
  methods: {
    async getTerminals() {
      this.terminals =
        (await this.$api.terminals.get({ merchantID: this.$route.params.id }))
          .data || [];
    },
    async getMerchant() {
      let merchant = await this.$api.merchants.getMerchant(
        this.$route.params.id
      );

      if (!merchant) {
        return this.$router.push("/admin/merchants/list");
      }
      this.model = merchant;
    },
    async unlinkFromMerchant(userID) {
      let operationResult = await this.$api.users.unlinkUserFromMerchant(
        userID,
        this.$route.params.id
      );
      if (operationResult.status === "success") {
        await this.getMerchant();
      }
    },
    async inviteUser(user) {
      this.actionInProgress = true;
      let operation = await this.$api.users.inviteUser({
        merchantID: this.$route.params.id,
        email: user.email
      });

      if (operation.status == "success") {
        await this.getMerchant();
      } else {
        this.$toasted.show(operation.message, { type: "error" });
      }
      this.actionInProgress = false;
    },
    async lockUser(user) {
      this.actionInProgress = true;
      let operation = await this.$api.users.lockUser(user.$userID);

      if (operation.status == "success") {
        await this.getMerchant();
      }
      this.actionInProgress = false;
    },
    async unlockUser(user) {
      this.actionInProgress = true;
      let operation = await this.$api.users.unlockUser(user.$userID);

      if (operation.status == "success") {
        await this.getMerchant();
      }
      this.actionInProgress = false;
    },
    async resetUserPassword(userID) {
      this.actionInProgress = true;
      let operation = await this.$api.users.resetUserPassword(userID);
      this.actionInProgress = false;
    },
    async loginAsMerchant(){
      let operation = await this.$api.merchants.loginAsMerchant(this.$route.params.id);
      if(operation.status === "success" && operation.message){
        this.loginAsMerchantURL = operation.message;
        window.open(operation.message, "_blank");
      }
    }
  },
  async mounted() {
    if (this.data) {
      this.model = this.data;
      return;
    }

    await this.getMerchant();
    await this.getTerminals();
    this.$store.commit("ui/changeHeader", {
      value: {
        text: { translate: false, value: this.model.businessName },
        threeDotMenu: [
          {
            text: this.$t("SeeHistory"),
            fn: () => {
              this.$router.push({
                name: "Audits",
                params: {
                  filters: {
                    merchantID: this.$route.params.id
                  }
                }
              });
            }
          }
        ]
      }
    });
  }
};
</script>