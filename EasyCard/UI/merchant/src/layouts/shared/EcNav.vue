<template>
  <v-navigation-drawer
    v-model="drawerObj"
    :clipped="$vuetify.breakpoint.lgAndUp"
    app
    :right="isRtl"
    :color="'ecbg lighten-4'"
  >
    <v-list class="py-0">
      <v-list-item two-line class="py-4">
        <v-list-item-avatar>
          <img src="https://randomuser.me/api/portraits/men/81.jpg" />
        </v-list-item-avatar>

        <v-list-item-content>
          <v-list-item-title>Name</v-list-item-title>
          <v-list-item-subtitle>Subtext</v-list-item-subtitle>
        </v-list-item-content>
      </v-list-item>

      <v-divider></v-divider>

      <template v-for="item in items">
        <v-list-group
          v-if="item.children"
          :key="item.text"
          v-model="item.expanded"
          :prepend-icon="item.expanded ? item.icon : item['icon-alt']"
          :color="'ecnavLink'"
          append-icon
        >
          <template v-slot:activator>
            <v-list-item-content>
              <v-list-item-title>{{ $t(item.text) }}</v-list-item-title>
            </v-list-item-content>
          </template>
          <v-list-item
            v-for="(child, i) in item.children"
            :key="i"
            link
            :to="child.to"
            class="px-6"
          >
            <v-list-item-action v-if="child.icon">
              <v-icon>{{ child.icon }}</v-icon>
            </v-list-item-action>
            <v-list-item-content>
              <v-list-item-title>{{ $t(child.text) }}</v-list-item-title>
            </v-list-item-content>
          </v-list-item>
        </v-list-group>
        <v-divider v-else-if="item.divider" :key="item.dividerArea"></v-divider>
        <v-list-item v-else-if="item.fn" :key="item.text" @click="item.fn()">
          <v-list-item-action>
            <v-icon>{{ item.icon }}</v-icon>
          </v-list-item-action>
          <v-list-item-content>
            <v-list-item-title>{{ $t(item.text) }}</v-list-item-title>
          </v-list-item-content>
        </v-list-item>
        <v-list-item v-else :key="item.text" link :to="item.to">
          <v-list-item-action>
            <v-icon>{{ item.icon }}</v-icon>
          </v-list-item-action>
          <v-list-item-content>
            <v-list-item-title>{{ $t(item.text) }}</v-list-item-title>
          </v-list-item-content>
        </v-list-item>
      </template>
      <v-list-item>
        <v-list-item-action>
          <v-switch class="px-2" :color="'accent'" label="RTL" v-model="$vuetify.rtl"></v-switch>
        </v-list-item-action>
        <v-list-item-content>
          <lang-switcher class></lang-switcher>
        </v-list-item-content>
      </v-list-item>

      <v-list-item>
        <v-list-item-action class="my-1 mx-0">
          <v-select
              :items="terminals"
              item-text="label"
              item-value="terminalID"
              return-object
              v-model="terminal"
              :label="$t('Terminal')"
              required
              outlined
            ></v-select>
        </v-list-item-action>
      </v-list-item>

      <v-list-item>
        <v-list-item-action class="my-1 mx-0">
          <v-select
          :items="currencies"
          item-text="description"
          return-object
          v-model="currency"
          :label="$t('Currency')"
          outlined
        ></v-select>
        </v-list-item-action>
      </v-list-item>
    </v-list>
  </v-navigation-drawer>
</template>
<script>
import LangSwitcher from "../../components/LanguageSwitcher";
import { mapState } from "vuex";
import mainAuth from '../../auth';

export default {
  name: "EcNav",
  props: ["drawer"],
  components: { LangSwitcher },
  data() {
    return {
      items: [
        {
          icon: "mdi-view-dashboard",
          text: "Dashboard",
          to: "/admin/dashboard"
        },
        {
          icon: "mdi-cash-minus",
          "icon-alt": "mdi-cash-plus",
          text: "Transactions",
          expanded: false,
          children: [
            {
              icon: "mdi-cash-multiple",
              text: "TransactionsList",
              to: "/admin/transactions/list"
            }
          ]
        },
        {
          icon: "mdi-account",
          "icon-alt": "mdi-account-outline",
          text: "Customers",
          expanded: false,
          children: [
            {
              icon: "mdi-account-group",
              text: "CustomersList",
              to: "/admin/customers/list"
            },
            {
              icon: "mdi-plus",
              text: "CreateCustomer",
              to: "/admin/customers/create"
            }
          ]
        },
        {
          icon: "mdi-basket",
          "icon-alt": "mdi-basket-outline",
          text: "Items",
          expanded: false,
          children: [
            {
              icon: "mdi-basket-outline",
              text: "ItemsList",
              to: "/admin/items/list"
            },
            {
              icon: "mdi-plus",
              text: "CreateItem",
              to: "/admin/items/create"
            }
          ]
        },
        { divider: true, dividerArea: "userAuth" },
        { icon: "mdi-account-cog", text: "Profile", to: "/admin/profile" },
        {
          icon: "mdi-logout",
          text: "Logout",
          fn: () => {
            mainAuth.signOut();
          }
        }
      ],
      terminals: [],
      currencies: []
    };
  },
  computed: {
    drawerObj: {
      get: function() {
        return this.drawer;
      },
      set: function(nv) {
        this.$emit("update:drawer", nv);
      }
    },
    isRtl: {
      cache: false,
      get: function() {
        return this.$vuetify.rtl === true;
      }
    },
    ...mapState({
      terminalStore: state => state.settings.terminal,
      currencyStore: state => state.settings.currency
    }),
    terminal: {
      get: function() {
        return this.terminalStore;
      },
      set: function(nv) {
        this.$store.commit("settings/changeTerminal", {
          vm: this,
          newTerminal: nv
        });
      }
    },
    currency: {
      get: function() {
        return this.currencyStore;
      },
      set: function(nv) {
        this.$store.commit("settings/changeCurrency", {
          vm: this,
          newCurrency: nv
        });
      }
    }
  },
  async mounted() {
    let terminals = (await this.$api.terminals.getTerminals());
    this.terminals = terminals ? terminals.data : [];

    //validate if stored terminal is still accessible. Clear it otherwise
    if (this.terminals.length > 0 && this.terminal) {
      let exists = this.lodash.some(this.terminals, t => t.terminalID === this.terminal.terminalID);
      if(!exists)
        this.terminal = null;
    }
    else if(this.terminals.length > 0 && !this.terminal){
      this.terminal = this.terminals[0];
    }
    else{
      this.terminal = null;
    }

    let dictionaries = await this.$api.dictionaries.getTransactionDictionaries();
    this.currencies = dictionaries ? dictionaries.currencyEnum : [];
    
    //same cache validation for currencies
    if (this.currencies.length > 0 && this.currency) {
      let exists = this.lodash.some(this.currencies, c => c.code === this.currency.code);
      if(!exists)
        this.currency = null;
    }
    else if(this.currencies.length > 0 && !this.currency){
      this.currency = this.currencies[0];
    }
    else{
      this.currency = null;
    }
  }
};
</script>