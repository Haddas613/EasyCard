<template>
  <v-card class="mx-auto" outlined>
    <v-divider></v-divider>
    <div>
      <v-data-table
        :headers="headers"
        :items="users"
        :options.sync="options"
        :server-items-length="totalAmount"
        :loading="loading"
        class="elevation-1"
      >
        <template v-slot:item.merchantID="{ item }">
          <router-link
            class="text-decoration-none"
            link
            :to="{name: 'Merchant', params: {id: item.$merchantID}}"
          >
            {{item.merchantID}}
          </router-link>
        </template>
        <template v-slot:item.actions="{ item }">
          <v-btn class="mx-1" color="primary" outlined small :title="$t('Invite')" :loading="actionInProgress" @click="inviteUser(item)" v-if="item.$status == 'Invited'">
            <v-icon small>mdi-email</v-icon>
          </v-btn>
          <v-btn class="mx-1" color="error" outlined small :title="$t('Lock')" :loading="actionInProgress" @click="lockUser(item)" v-if="item.$status == 'Active'">
            <v-icon small>mdi-lock</v-icon>
          </v-btn>
          <v-btn class="mx-1" color="success" outlined small :title="$t('Unlock')" :loading="actionInProgress" @click="unlockUser(item)" v-if="item.$status == 'Locked'">
            <v-icon small>mdi-lock-open</v-icon>
          </v-btn>
          <v-btn class="mx-1" color="orange darken-3" outlined small :title="$t('ResetPassword')" :loading="actionInProgress" @click="resetUserPassword(item.$userID)">
            <v-icon small>mdi-lock-reset</v-icon>
          </v-btn>
        </template>
      </v-data-table>
    </div>
  </v-card>
</template>

<script>
export default {
  components: {},
  data() {
    return {
      totalAmount: 0,
      users: [],
      loading: true,
      options: {},
      pagination: {},
      headers: [],
      usersFilter: {},
      actionInProgress: false
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
  methods: {
    async getDataFromApi() {
      this.loading = true;
      let data = await this.$api.users.get({
        ...this.usersFilter,
        ...this.options
      });
      this.users = data.data;
      this.totalAmount = data.numberOfRecords;
      this.loading = false;

      if (!this.headers || this.headers.length === 0) {
        this.headers = [
          ...data.headers,
          { value: "actions", text: this.$t("Actions") }
        ];
      }
    },
    //TODO
    async applyFilter(filter) {
      this.usersFilter = filter;
      await this.getDataFromApi();
    },
    async inviteUser(user){
      this.actionInProgress = true;
      let operation = await this.$api.users.inviteUser({
        merchantID: user.$merchantID,
        email: user.email
      });

      if(operation.status == "success"){
        user.status = this.$merchantDictionaries['userStatusEnum']['invited'];
      }else{
        this.$toasted.show(operation.message, { type: "error" });
      }
      this.actionInProgress = false;
    },
    async lockUser(user){
      this.actionInProgress = true;
      let operation = await this.$api.users.lockUser(user.$userID);

      if(operation.status == "success"){
        user.status = this.$merchantDictionaries['userStatusEnum']['locked'];
      }
      this.actionInProgress = false;
    },
    async unlockUser(user){
      this.actionInProgress = true;
      let operation = await this.$api.users.unlockUser(user.$userID);

      if(operation.status == "success"){
        user.status = this.$merchantDictionaries['userStatusEnum']['active'];
      }
      this.actionInProgress = false;
    },
    async resetUserPassword(userID){
      this.actionInProgress = true;
      let operation = await this.$api.users.resetUserPassword(userID);
      this.actionInProgress = false;
    }
  }
};
</script>