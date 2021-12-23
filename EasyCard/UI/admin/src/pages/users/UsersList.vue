<template>
  <v-card class="mx-auto" outlined>
    <edit-user-dialog 
      v-if="selectedUser"
      :show.sync="editUserDialog"
      :key="selectedUser.userID"
      :user="selectedUser"
      v-on:ok="closeEditRolesDialog()"></edit-user-dialog>
    <v-expansion-panels :flat="true">
      <v-expansion-panel>
        <v-expansion-panel-header>{{$t('Filters')}}</v-expansion-panel-header>
        <v-expansion-panel-content>
          <users-filter :filter-data="usersFilter" v-on:apply="applyFilter($event)"></users-filter>
        </v-expansion-panel-content>
      </v-expansion-panel>
    </v-expansion-panels>
    <v-divider></v-divider>
    <div>
      <v-data-table
        :headers="headers"
        :items="users"
        :options.sync="options"
        :server-items-length="totalAmount"
        :loading="loading"
        :header-props="{ sortIcon: null }"
        class="elevation-1"
      >
        <template v-slot:item.merchantName="{ item }">
          <router-link
            class="text-decoration-none"
            link
            :to="{name: 'Merchant', params: {id: item.merchantID}}"
          >
            {{item.merchantName}}
          </router-link>
        </template>
        <template v-slot:item.status="{ item }">
          <span v-bind:class="statusColors[item.$status]">{{item.status}}</span>
        </template>
        <template v-slot:item.actions="{ item }">
          <v-btn class="mx-1" color="primary" outlined small :title="$t('Invite')" :disabled="actionInProgress" @click="invokeAction('inviteUser', item)" v-if="item.$status == 'invited'">
            <v-icon small>mdi-email</v-icon>
          </v-btn>
          <v-btn class="mx-1" color="error" outlined small :title="$t('Lock')" :disabled="actionInProgress" @click="invokeAction('lockUser', item)" v-if="item.$status == 'active'">
            <v-icon small>mdi-lock</v-icon>
          </v-btn>
          <v-btn class="mx-1" color="success" outlined small :title="$t('Unlock')" :disabled="actionInProgress" @click="invokeAction('unlockUser', item)" v-if="item.$status == 'locked'">
            <v-icon small>mdi-lock-open</v-icon>
          </v-btn>
          <v-btn class="mx-1" color="orange darken-3" outlined small :title="$t('ResetPassword')" v-if="item.$status != 'invited'" :disabled="actionInProgress" @click="invokeAction('resetUserPassword', item)">
            <v-icon small>mdi-lock-reset</v-icon>
          </v-btn>
          <v-btn class="mx-1" color="deep-purple" outlined link small :title="$t('SeeHistory')" :to="{name:'Audits',params:{filters:{userID: item.$userID}}}">
            <v-icon small>mdi-book-account</v-icon>
          </v-btn>
          <v-btn class="mx-1" color="secondary" outlined link small :title="$t('EditUser')" @click="showEditUserDialog(item)">
            <v-icon small>mdi-card-account-details-outline</v-icon>
          </v-btn>
        </template>
      </v-data-table>
    </div>
  </v-card>
</template>

<script>
export default {
  name: "Users",
  components: {
    UsersFilter: () => import("../../components/users/UsersFilter"),
    EditUserDialog: () => import("../../components/users/EditUserDialog")
  },
  data() {
    return {
      totalAmount: 0,
      users: [],
      loading: false,
      options: {},
      pagination: {},
      headers: [],
      usersFilter: {},
      actionInProgress: false,
      statusColors: {
        invited: "primary--text",
        active: "success--text",
        locked: "error--text",
      },
      editUserDialog: false,
      selectedUser: null
    };
  },
  watch: {
    options: {
      handler: async function() {
        await this.getDataFromApi();
      },
      deep: true
    }
  },
  async mounted () {
    this.$merchantDictionaries = await this.$api.dictionaries.$getMerchantDictionaries();
  },
  beforeRouteEnter(to, from, next) {
    next(vm => {
      vm.$store.commit("ui/setRefreshHandler", { value: vm.getDataFromApi});
    });
  },
  methods: {
    async getDataFromApi() {
      if(this.loading) { return; }
      this.loading = true;
      try{
        let data = await this.$api.users.get({
          ...this.usersFilter,
          ...this.options
        });
        this.users = data.data;
        this.totalAmount = data.numberOfRecords;

        if (!this.headers || this.headers.length === 0) {
          this.headers = [
            ...data.headers,
            { value: "actions", text: this.$t("Actions"), sortable: false  }
          ];
        }
      }finally{
        this.loading = false;
      }
    },
    async applyFilter(filter) {
      this.options.page = 1;
      this.usersFilter = filter;
      await this.getDataFromApi();
    },
    async invokeAction(actionName, user){
      this.actionInProgress = true;
      try{
        var operation = await this[actionName](user);
        if (!this.$apiSuccess(operation)) return;

        await this.getDataFromApi();
      }finally{
        this.actionInProgress = false;
      }
    },
    inviteUser(user) {
      return this.$api.users.inviteUser({merchantID: user.$merchantID, email: user.email});
    },
    lockUser(user) {
      return this.$api.users.lockUser(user.$userID);
    },
    unlockUser(user) {
      return this.$api.users.unlockUser(user.$userID);
    },
    resetUserPassword(user) {
      return this.$api.users.resetUserPassword(user.$userID);
    },
    async showEditUserDialog(user){
      this.selectedUser = user;
      this.editUserDialog = true;
    },
    async closeEditRolesDialog(){
      this.editUserDialog = false;
      await this.getDataFromApi();
    }
  }
};
</script>