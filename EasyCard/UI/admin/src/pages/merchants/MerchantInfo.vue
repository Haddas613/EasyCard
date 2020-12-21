<template>
  <v-flex>
    <v-card class="mx-2 my-2">
      <v-card-title class="py-2">
        <v-row no-gutters class="py-0">
          <v-col cols="9" class="d-flex">
            <span class="pt-2 ecdgray--text subtitle-2 text-uppercase">{{$t('PersonalInformation')}}</span>
          </v-col>
          <v-col cols="3" class="d-flex justify-end">
            <v-btn
              text
              class="primary--text px-0"
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
        <create-terminal-dialog :show.sync="showCreateTerminalDialog" v-on:ok="getTerminals()" :merchant-id="$route.params.id"></create-terminal-dialog>
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
        <create-user-dialog :show.sync="showCreateUserDialog" v-on:ok="getMerchant()" :merchant-id="$route.params.id"></create-user-dialog>
        <ec-list :items="model.users" v-if="model.users && model.users.length > 0">
          <template v-slot:left="{ item }">
            <v-col cols="12" md="6" lg="6" class="caption ecgray--text">{{item.userID | guid}}</v-col>
            <v-col cols="12" md="6" lg="6">{{item.email}}</v-col>
          </template>

          <template v-slot:append="{ item }">
            <v-btn icon @click="unlinkFromMerchant(item.userID)">
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
    CreateTerminalDialog: () => import("../../components/terminals/CreateTerminalDialog"),
    CreateUserDialog: () => import("../../components/users/CreateUserDialog"),
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
    };
  },
  methods: {
    async getTerminals(){
      this.terminals = (await this.$api.terminals.get({ merchantID: this.$route.params.id }))
        .data || [];
    },
    async getMerchant(){
      let merchant = await this.$api.merchants.getMerchant(this.$route.params.id);

      if (!merchant) {
        return this.$router.push("/admin/merchants/list");
      }
      this.model = merchant;
    },
    async unlinkFromMerchant(userID){
      let operationResult = await this.$api.users.unlinkUserFromMerchant(userID, this.$route.params.id);
      if (operationResult.status === "success") {
        await this.getMerchant();
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
        text: { translate: false, value: this.model.businessName }
      }
    });
  }
};
</script>