<template>
  <v-flex>
    <edit-user-dialog
      v-if="selectedUser"
      :show.sync="editUserDialog"
      :key="selectedUser.userID"
      :user="selectedUser"
      :merchant-id="$route.params.id"
      v-on:ok="closeEditRolesDialog()"
    ></edit-user-dialog>
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
              <a
                class="white--text text-decoration-none"
                :href="loginAsMerchantURL"
                target="_blank"
              >
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
          v-on:ok="$router.push({name: 'EditTerminal', params: {id: $event}})"
          :merchant-id="$route.params.id"
        ></create-terminal-dialog>
        <ec-list :items="terminals" v-if="terminals && terminals.length > 0">
          <template v-slot:left="{ item }">
            <v-col cols="12" md="6" lg="6" class="caption ecgray--text">{{item.terminalID}}</v-col>
            <v-col v-bind:class="{ 'text-decoration-line-through' : item.$status == 'disabled' }" cols="12" md="6" lg="6">{{item.label}}</v-col>
          </template>

          <template v-slot:right="{ item }">
            <v-col class="text-start" cols="6" md="6">
              <span class="success--text" v-if="item.processorTerminalReference">
                <b>{{item.processorTerminalReference}}</b>
              </span>
            </v-col>
            <v-col class="text-end" cols="6" md="6">
              <span v-bind:class="terminalStatusColors[item.$status]">{{item.status}}</span>
            </v-col>
          </template>

          <template v-slot:append="{ item }">
            <v-item-group borderless dense>
              <template v-if="$vuetify.breakpoint.smAndDown">
                <v-btn icon link :to="{name: 'EditTerminal', params: {id: item.$terminalID}}">
                <v-icon color="secondary">mdi-pencil</v-icon>
                </v-btn>
                <v-btn class="mx-1" color="error" icon v-if="item.$status != 'disabled'" @click="disable(item)">
                  <v-icon small>mdi-cancel</v-icon>
                </v-btn>
                <v-btn class="mx-1" color="success" icon v-if="item.$status == 'disabled'" @click="enable(item)">
                  <v-icon small>mdi-chevron-down-circle</v-icon>
                </v-btn>
              </template>
              <template v-else>
                <v-btn link small outlined :to="{name: 'EditTerminal', params: {id: item.$terminalID}}">
                  <v-icon color="secondary">mdi-pencil</v-icon>
                </v-btn>
                <v-btn class="mx-1" small outlined color="error" v-if="item.$status != 'disabled'" @click="disable(item)">
                  <v-icon small>mdi-cancel</v-icon>
                </v-btn>
                <v-btn class="mx-1" small outlined color="success" v-if="item.$status == 'disabled'" @click="enable(item)">
                  <v-icon small>mdi-chevron-down-circle</v-icon>
                </v-btn>
              </template>
            </v-item-group>
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
          <template v-slot:right="{ item }">
            <v-col cols="12" md="6" lg="6">
              <span v-if="item.displayName != item.email">
                <b>{{item.displayName}}</b>
              </span>
            </v-col>
            <v-col cols="12" md="6" lg="6" class="text-end">
              <span v-bind:class="userStatusColors[item.status]">{{userStatuses[item.status]}}</span>
            </v-col>
          </template>

          <template v-slot:append="{ item }">
            <div>
              <v-btn
                class="mx-1"
                color="primary"
                icon
                :title="$t('Invite')"
                :disabled="actionInProgress"
                @click="invokeAction('inviteUser', item)"
                v-if="item.status == 'invited'"
              >
                <v-icon>mdi-email</v-icon>
              </v-btn>
              <v-btn
                class="mx-1"
                color="error"
                icon
                :title="$t('Lock')"
                :disabled="actionInProgress"
                @click="invokeAction('lockUser',item)"
                v-if="item.status == 'active'"
              >
                <v-icon>mdi-lock</v-icon>
              </v-btn>
              <v-btn
                class="mx-1"
                color="success"
                icon
                :title="$t('Unlock')"
                :disabled="actionInProgress"
                @click="invokeAction('unlockUser',item)"
                v-if="item.status == 'locked'"
              >
                <v-icon>mdi-lock-open</v-icon>
              </v-btn>
              <v-btn
                class="mx-1"
                color="orange darken-3"
                icon
                :title="$t('ResetPassword')"
                :disabled="actionInProgress || item.status == 'invited'"
                @click="invokeAction('resetUserPassword', item)"
              >
                <v-icon>mdi-lock-reset</v-icon>
              </v-btn>
              <v-btn
                class="mx-1"
                color="deep-purple"
                icon
                link
                :title="$t('SeeHistory')"
                :to="{name:'Audits',params:{filters:{userID: item.userID}}}"
              >
                <v-icon>mdi-book-account</v-icon>
              </v-btn>
              <v-btn icon @click="invokeAction('unlinkFromMerchant', item)" :disabled="actionInProgress">
                <v-icon color="error">mdi-delete</v-icon>
              </v-btn>
              <v-btn
                class="mx-1"
                color="secondary"
                icon
                link
                :title="$t('EditUser')"
                @click="showEditRolesDialog(item)"
                v-on:ok="getMerchant()"
              >
                <v-icon small>mdi-card-account-details-outline</v-icon>
              </v-btn>
            </div>
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
    CreateUserDialog: () => import("../../components/users/CreateUserDialog"),
    EditUserDialog: () =>
      import("../../components/users/EditUserDialog")
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
      loginAsMerchantURL: null,
      editUserDialog: false,
      selectedUser: null,
      userStatusColors: {
        invited: "primary--text",
        active: "success--text",
        locked: "error--text"
      },
      terminalStatusColors: {
        'approved': 'success--text',
        'disabled': 'error--text',
        'pendingApproval': 'secondary--text',
      },
      userStatuses: {}
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
        return this.$router.push({ name: "Merchants" });
      }
      this.model = merchant;
    },
    async invokeAction(actionName, user){
      this.actionInProgress = true;
      try{
        var operation = await this[actionName](user);
        if (!this.$apiSuccess(operation)) return;

        await this.getMerchant();
      }finally{
        this.actionInProgress = false;
      }
    },
    unlinkFromMerchant(user) {
      return this.$api.users.unlinkUserFromMerchant(user.userID, this.$route.params.id);
    },
    inviteUser(user) {
      return this.$api.users.inviteUser({merchantID: this.$route.params.id, email: user.email});
    },
    lockUser(user) {
      return this.$api.users.lockUser(user.userID);
    },
    unlockUser(user) {
      return this.$api.users.unlockUser(user.userID);
    },
    resetUserPassword(user) {
      return this.$api.users.resetUserPassword(user.userID);
    },
    async loginAsMerchant() {
      let operation = await this.$api.merchants.loginAsMerchant(
        this.$route.params.id
      );
      if (operation.status === "success" && operation.message) {
        this.loginAsMerchantURL = operation.message;
        window.open(operation.message, "_blank");
      }
    },
    async showEditRolesDialog(user) {
      this.selectedUser = user;
      this.editUserDialog = true;
    },
    async closeEditRolesDialog() {
      this.editUserDialog = false;
      await this.getMerchant();
    },
    async enable(terminal){
      let opResult = await this.$api.terminals.enableTerminal(terminal.$terminalID);
      await this.getTerminals();
    },
    async disable(terminal){
      let opResult = await this.$api.terminals.disableTerminal(terminal.$terminalID);
      await this.getTerminals();
    }
  },
  async mounted() {
    let $dictionaries = await this.$api.dictionaries.$getMerchantDictionaries();
    this.userStatuses = $dictionaries["userStatusEnum"];

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
          },
          {
            text: this.$t("Transactions"),
            fn: () => {
              this.$router.push({
                name: "Transactions",
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